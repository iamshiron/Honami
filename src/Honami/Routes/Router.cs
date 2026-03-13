using Microsoft.AspNetCore.Http;
using Shiron.Honami.Exceptions;
using Shiron.Honami.HTTP;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Routes;

public readonly struct RouteCallback {
    private readonly IEndpoint _instance;
    private readonly RouteHandlerDelegate? _fastHandler;
    private readonly FlexibleRouteHandlerDelegate? _flexibleHandler;
    private readonly ServerMiddleware[] _middlewares;
    private readonly bool _hasResultFilters;

    public RouteCallback(
        IEndpoint instance,
        RouteHandlerDelegate? fastHandler,
        FlexibleRouteHandlerDelegate? flexibleHandler,
        ServerMiddleware[] middlewares
    ) {
        _instance = instance;
        _fastHandler = fastHandler;
        _flexibleHandler = flexibleHandler;
        _middlewares = middlewares;
        _hasResultFilters = middlewares.Any(m => m is IResultFilter);
    }

    public Task Execute(HttpContext context) {
        return _hasResultFilters ? ExecuteFlexiblePath(context) : ExecuteFastPath(context);
    }

    private Task ExecuteFastPath(HttpContext context) {
        var handler = _fastHandler!;
        var instance = _instance;
        var middlewares = _middlewares;

        async Task CoreHandler(HttpContext ctx) => await handler(instance, ctx);
        MiddlewareDelegate pipeline = CoreHandler;

        for (var i = middlewares.Length - 1; i >= 0; i--) {
            var middleware = middlewares[i];
            var currentNext = pipeline;
            pipeline = ctx => middleware.ExecuteAsync(ctx, currentNext);
        }

        return pipeline(context);
    }

    private async Task ExecuteFlexiblePath(HttpContext context) {
        IHonamiResult result = await _flexibleHandler!(_instance, context);

        foreach (var middleware in _middlewares) {
            if (middleware is IResultFilter filter) {
                result = filter.OnResultExecuting(context, result);
            }
        }

        await result.ExecuteAsync(context);
    }
}

public sealed class RouteTreeNode(
    RouteSegment segment,
    string[] staticSegmentNames,
    RouteTreeNode[] staticChildren,
    RouteTreeNode? parameterChild,
    RouteCallback? callback,
    int maxParamDepth
) {
    public RouteSegment Segment { get; } = segment;
    public string[] StaticSegmentNames { get; } = staticSegmentNames;
    public RouteTreeNode[] StaticChildren { get; } = staticChildren;
    public RouteTreeNode? ParameterChild { get; } = parameterChild;
    public RouteCallback? Callback { get; } = callback;
    public int MaxParamDepth { get; } = maxParamDepth;
}

file class RouteTreeBuilder {
    private sealed class Node {
        public RouteSegment Segment { get; }
        public Dictionary<string, Node> StaticChildren { get; } = [];
        public Node? ParameterChild { get; set; }
        public RouteCallback? Callback { get; set; }

        public Node(RouteSegment segment) {
            Segment = segment;
        }
    }

    private readonly Node _root = new(new RouteSegment("", RouteSegmentType.Literal));

    public void AddRoute(string path, RouteCallback callback) {
        var segments = ParseSegments(path);
        var current = _root;

        foreach (var segment in segments) {
            current = segment.Type switch {
                RouteSegmentType.Literal => GetOrCreateStaticChild(current, segment),
                RouteSegmentType.Dynamic => GetOrCreateParameterChild(current, segment),
                _ => throw new ArgumentException($"Unsupported segment type: {segment.Type}")
            };
        }

        current.Callback = callback;
    }

    private static Node GetOrCreateStaticChild(Node parent, RouteSegment segment) {
        if (!parent.StaticChildren.TryGetValue(segment.Name, out var child)) {
            child = new Node(segment);
            parent.StaticChildren[segment.Name] = child;
        }
        return child;
    }

    private static Node GetOrCreateParameterChild(Node parent, RouteSegment segment) {
        parent.ParameterChild ??= new Node(segment);
        return parent.ParameterChild;
    }

    private static RouteSegment[] ParseSegments(string path) {
        var parts = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        var segments = new RouteSegment[parts.Length];

        for (var i = 0; i < parts.Length; i++) {
            segments[i] = ParseSegment(parts[i]);
        }

        return segments;
    }

    private static RouteSegment ParseSegment(string part) {
        if (part.StartsWith('[') && part.EndsWith(']')) {
            var paramName = part[1..^1];
            return new RouteSegment(paramName, RouteSegmentType.Dynamic);
        }

        return new RouteSegment(part, RouteSegmentType.Literal);
    }

    public RouteTreeNode Build() {
        return BuildNode(_root);
    }

    private static RouteTreeNode BuildNode(Node node) {
        var staticSegmentNames = new string[node.StaticChildren.Count];
        var staticChildren = new RouteTreeNode[node.StaticChildren.Count];

        var i = 0;
        foreach (var (name, child) in node.StaticChildren) {
            staticSegmentNames[i] = name;
            staticChildren[i] = BuildNode(child);
            i++;
        }

        var parameterChild = node.ParameterChild != null ? BuildNode(node.ParameterChild) : null;
        var maxParamDepth = CalculateMaxParamDepth(node);

        return new RouteTreeNode(
            node.Segment,
            staticSegmentNames,
            staticChildren,
            parameterChild,
            node.Callback,
            maxParamDepth
        );
    }

    private static int CalculateMaxParamDepth(Node node) {
        var currentDepth = node.Segment.Type == RouteSegmentType.Dynamic ? 1 : 0;
        var maxChildDepth = 0;

        foreach (var child in node.StaticChildren.Values) {
            maxChildDepth = Math.Max(maxChildDepth, CalculateMaxParamDepth(child));
        }

        if (node.ParameterChild != null) {
            maxChildDepth = Math.Max(maxChildDepth, CalculateMaxParamDepth(node.ParameterChild));
        }

        return currentDepth + maxChildDepth;
    }
}

public class Router {
    public Dictionary<HTTPMethod, RouteTreeNode> Endpoints { get; } = new();
    private readonly List<(string path, ServerMiddleware middleware)> _middlewares;

    public Router(
        Dictionary<HTTPMethod, Dictionary<string, IEndpoint>> endpoints,
        List<(string path, ServerMiddleware middleware)> middlewares
    ) {
        _middlewares = middlewares;

        foreach (var (httpMethod, pathEndpoints) in endpoints) {
            Endpoints[httpMethod] = BuildTree(pathEndpoints, middlewares);
        }
    }

    private static RouteTreeNode BuildTree(
        Dictionary<string, IEndpoint> pathEndpoints,
        List<(string path, ServerMiddleware middleware)> middlewares
    ) {
        var builder = new RouteTreeBuilder();

        foreach (var (path, endpoint) in pathEndpoints) {
            var applicableMiddlewares = middlewares
                .Where(m => path.StartsWith(m.path, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.middleware)
                .ToArray();

            var hasResultFilters = applicableMiddlewares.Any(m => m is IResultFilter);

            var callback = hasResultFilters
                ? new RouteCallback(endpoint, null, RouteCompiler.CompileFlexiblePath(endpoint.GetType()), applicableMiddlewares)
                : new RouteCallback(endpoint, RouteCompiler.CompileFastPath(endpoint.GetType()), null, applicableMiddlewares);

            builder.AddRoute(path, callback);
        }

        return builder.Build();
    }

    public async Task Match(HttpContext context) {
        var path = context.Request.Path.Value ?? "/";
        var methodString = context.Request.Method;

        var method = HTTPMethods.FromString(methodString);
        if (!method.HasValue) {
            throw new RouterInvalidHttpMethodException(methodString);
        }

        if (!Endpoints.TryGetValue(method.Value, out var root)) {
            throw new RouterNotFoundException(path, method.Value);
        }

        var maxParams = root.MaxParamDepth;
        var paramNames = maxParams > 0 ? new string[maxParams] : null;
        var paramValues = maxParams > 0 ? new string[maxParams] : null;

        if (!TryMatch(path.AsSpan(), root, paramNames, paramValues, out var paramCount, out var callback)) {
            throw new RouterNotFoundException(path, method.Value);
        }

        if (paramCount > 0) {
            var routeParams = new Dictionary<string, string>(paramCount);
            for (var i = 0; i < paramCount; i++) {
                routeParams[paramNames![i]] = paramValues![i];
            }
            context.Items["RouteParams"] = routeParams;
        } else {
            context.Items["RouteParams"] = new Dictionary<string, string>();
        }

        await callback.Execute(context);
    }

    private static bool TryMatch(
        ReadOnlySpan<char> path,
        RouteTreeNode root,
        string[]? paramNames,
        string[]? paramValues,
        out int paramCount,
        out RouteCallback callback
    ) {
        Span<Range> segmentRanges = stackalloc Range[32];
        var segmentCount = ExtractSegments(path, segmentRanges);

        paramCount = 0;
        return TryMatchRecursive(root, path, segmentRanges, segmentCount, 0, paramNames, paramValues, ref paramCount, out callback);
    }

    private static bool TryMatchRecursive(
        RouteTreeNode node,
        ReadOnlySpan<char> path,
        Span<Range> segmentRanges,
        int segmentCount,
        int index,
        string[]? paramNames,
        string[]? paramValues,
        ref int paramCount,
        out RouteCallback callback
    ) {
        if (index >= segmentCount) {
            if (node.Callback != null) {
                callback = node.Callback.Value;
                return true;
            }
            callback = default;
            return false;
        }

        var segmentRange = segmentRanges[index];
        var segment = path[segmentRange];

        for (var i = 0; i < node.StaticChildren.Length; i++) {
            if (segment.SequenceEqual(node.StaticSegmentNames[i].AsSpan())) {
                if (TryMatchRecursive(node.StaticChildren[i], path, segmentRanges, segmentCount, index + 1, paramNames, paramValues, ref paramCount,
                    out callback)) {
                    return true;
                }
            }
        }

        if (node.ParameterChild != null) {
            var savedParamCount = paramCount;
            if (paramNames != null && paramValues != null && paramCount < paramNames.Length) {
                paramNames[paramCount] = node.ParameterChild.Segment.Name;
                paramValues[paramCount] = segment.ToString();
                paramCount++;
            }

            if (TryMatchRecursive(node.ParameterChild, path, segmentRanges, segmentCount, index + 1, paramNames, paramValues, ref paramCount, out callback)) {
                return true;
            }

            paramCount = savedParamCount;
        }

        callback = default;
        return false;
    }

    private static int ExtractSegments(ReadOnlySpan<char> path, Span<Range> ranges) {
        var count = 0;
        var start = 0;
        var inSegment = false;

        for (var i = 0; i < path.Length && count < ranges.Length; i++) {
            if (path[i] == '/') {
                if (inSegment) {
                    ranges[count++] = new Range(start, i);
                    inSegment = false;
                }
            } else if (!inSegment) {
                start = i;
                inSegment = true;
            }
        }

        if (inSegment && count < ranges.Length) {
            ranges[count++] = new Range(start, path.Length);
        }

        return count;
    }
}
