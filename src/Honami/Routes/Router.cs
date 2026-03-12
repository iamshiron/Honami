using System.Reflection;
using Shiron.Honami.HTTP;

namespace Shiron.Honami.Routes;

public class Router {
    public Dictionary<HTTPMethod, Dictionary<string, IRoute>> Endpoints { get; } = new() {
        [HTTPMethod.Get] = [],
        [HTTPMethod.Post] = [],
        [HTTPMethod.Put] = [],
        [HTTPMethod.Delete] = [],
        [HTTPMethod.Patch] = [],
        [HTTPMethod.Head] = [],
        [HTTPMethod.Options] = []
    };

    public Router(Dictionary<string, IRoute> routes) {
        foreach (var (path, route) in routes) {
            var type = route.GetType();
            Console.WriteLine(route.GetType().FullName);
            foreach (var httpMethod in HTTPMethods.All) {
                var httpMethodName = httpMethod.ToString();
                var method = type.GetMethod(httpMethodName);
                if (method == null) continue;

                var implemented = method.DeclaringType == type;
                if (!implemented || method.IsAbstract) continue;

                Endpoints[httpMethod].Add(path, route);
            }
        }
    }
}
