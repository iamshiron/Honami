namespace Shiron.Honami.Routes;

public class RouteBuilder {
    private readonly Dictionary<string, IRoutes> _routes = [];

    public void AddRoute(string path, IRoutes routes) {
        _routes.Add(path, routes);
    }

    public Router Build() {
        return new Router(_routes);
    }
}
