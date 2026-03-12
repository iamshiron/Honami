using System.Reflection;
using Microsoft.AspNetCore.Http;
using Shiron.Honami.Exceptions;
using Shiron.Honami.HTTP;
using Shiron.Honami.HTTP.Results;
using Results = Microsoft.AspNetCore.Http.Results;

namespace Shiron.Honami.Routes;

public readonly record struct RouteCallback(IRoutes Instance, MethodInfo Method);

public class Router {
    public Dictionary<HTTPMethod, Dictionary<string, RouteCallback>> Endpoints { get; } = new() {
        [HTTPMethod.Get] = [],
        [HTTPMethod.Post] = [],
        [HTTPMethod.Put] = [],
        [HTTPMethod.Delete] = [],
        [HTTPMethod.Patch] = [],
        [HTTPMethod.Head] = [],
        [HTTPMethod.Options] = []
    };

    public Router(Dictionary<string, IRoutes> routes) {
        foreach (var (path, route) in routes) {
            var type = route.GetType();
            Console.WriteLine(route.GetType().FullName);
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

    public IHonamiResult Match(HttpContext context) {
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

        var result = (IHonamiResult?) callback.Method.Invoke(callback.Instance, null);
        if (result == null) {
            throw new RouterInvalidResultException(callback.Instance, callback.Method);
        }

        return result;
    }
}
