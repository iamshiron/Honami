using System.Reflection;
using Microsoft.AspNetCore.Http;
using Shiron.Honami.Exceptions;
using Shiron.Honami.HTTP;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes;

public readonly struct RouteCallback(IRoutes instance, MethodInfo method) {
    private readonly object _instance = instance;
    private readonly RouteHandlerDelegate _handler = RouteCompiler.CompileRoute(instance.GetType(), method);

    public HonamiResult Execute(HonamiRequest request) {
        return _handler(_instance, request);
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

    public Router(Dictionary<string, IRoutes> routes) {
        Dictionary<HTTPMethod, Dictionary<string, RouteCallback>> endpoints = new() {
            [HTTPMethod.Get] = [],
            [HTTPMethod.Post] = [],
            [HTTPMethod.Put] = [],
            [HTTPMethod.Delete] = [],
            [HTTPMethod.Patch] = [],
            [HTTPMethod.Head] = [],
            [HTTPMethod.Options] = [],
            [HTTPMethod.Trace] = [],
            [HTTPMethod.Connect] = []
        };

        foreach (var (path, route) in routes) {
            var type = route.GetType();

            foreach (var httpMethod in HTTPMethods.All) {
                var httpMethodName = httpMethod.ToString();
                var method = type.GetMethod(httpMethodName);
                if (method == null) continue;

                var implemented = method.DeclaringType == type;
                if (!implemented || method.IsAbstract) continue;

                endpoints[httpMethod].Add(path, new RouteCallback(route, method));
            }
        }

        foreach (var (httpMethod, pathCallbacks) in endpoints) {
            Endpoints[httpMethod] = BuildTree(pathCallbacks);
        }
    }

    private static RouteTreeNode BuildTree(Dictionary<string, RouteCallback> pathCallbacks) {
        var builder = new RouteTreeBuilder();

        foreach (var (path, callback) in pathCallbacks) {
            builder.AddRoute(path, callback);
        }

        return builder.Build();
    }

    public HonamiResult Match(HttpContext context) {
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

        HonamiRequest request;
        if (paramCount > 0) {
            var routeParams = new Dictionary<string, string>(paramCount);
            for (var i = 0; i < paramCount; i++) {
                routeParams[paramNames![i]] = paramValues![i];
            }
            request = new HonamiRequest(routeParams);
        } else {
            request = new HonamiRequest([]);
        }

        context.Items["HonamiRequest"] = request;
        return callback.Execute(request);
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
