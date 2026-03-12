using Shiron.Honami.Routes;

namespace Shiron.Honami;

public class HonamiAppBuilder {
    private readonly RouteBuilder _routeBuilder = new();

    public HonamiAppBuilder RegisterRoute(string path, IRoute route) {
        _routeBuilder.AddRoute(path, route);
        return this;
    }

    public HonamiApp Build() {
        return new HonamiApp(_routeBuilder.Build());
    }
}
