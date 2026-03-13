using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Routes;

public class RouteBuilder {
    private readonly Dictionary<string, IRoutes> _routes = [];
    private readonly Dictionary<string, ServerMiddleware> _middlewares = [];

    public void AddRoute(string path, IRoutes routes) {
        _routes.Add(path, routes);
    }
    public void AddMiddleware(string path, ServerMiddleware middleware) {
        _middlewares.Add(path, middleware);
    }

    public Router Build() {
        return new Router(_routes, _middlewares);
    }
}
