using System.Reflection;
using Microsoft.AspNetCore.Http;
using Shiron.Honami.Exceptions;
using Shiron.Honami.HTTP;
using Shiron.Honami.HTTP.Results;
using Results = Microsoft.AspNetCore.Http.Results;

namespace Shiron.Honami.Routes;

public readonly struct RouteCallback(IRoutes instance, MethodInfo method) {
    private readonly object _instance = instance;
    private readonly RouteHandlerDelegate _handler = RouteCompiler.CompileRoute(instance.GetType(), method);

    public HonamiResult Execute() {
        return _handler(_instance);
    }
}

public class Router {
    public Dictionary<HTTPMethod, Dictionary<string, RouteCallback>> Endpoints { get; } = new() {
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

    public Router(Dictionary<string, IRoutes> routes) {
        foreach (var (path, route) in routes) {
            var type = route.GetType();
            foreach (var httpMethod in HTTPMethods.All) {
                var httpMethodName = httpMethod.ToString();
                var method = type.GetMethod(httpMethodName);
                if (method == null) continue;

                var implemented = method.DeclaringType == type;
                if (!implemented || method.IsAbstract) continue;

                Endpoints[httpMethod].Add(path, new RouteCallback(route, method));
            }
        }
    }

    public HonamiResult Match(HttpContext context) {
        var path = context.Request.Path;
        var methodString = context.Request.Method;

        var method = HTTPMethods.FromString(methodString);
        if (!method.HasValue) {
            throw new RouterInvalidHttpMethodException(methodString);
        }

        if (!Endpoints.TryGetValue(method.Value, out var endpoints)) {
            throw new RouterNotFoundException(path, method.Value);
        }
        if (!endpoints.TryGetValue(path, out var callback)) {
            throw new RouterNotFoundException(path, method.Value);
        }

        var result = callback.Execute();
        return result;
    }
}
