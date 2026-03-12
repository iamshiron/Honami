namespace Shiron.Honami.Routes;

public class RouteBuilder {
    private readonly Dictionary<string, IRoute> _routes = [];

    public void AddRoute(string path, IRoute route) {
        _routes.Add(path, route);
    }

    public Router Build() {
        return new Router(_routes);
    }
}
