using Microsoft.AspNetCore.Builder;
using Shiron.Honami.Routes;

namespace Shiron.Honami;

public class HonamiAppBuilder {
    private readonly RouteBuilder _routeBuilder = new();
    private readonly WebApplicationBuilder _webBuilder;

    public HonamiAppBuilder() {
        _webBuilder = WebApplication.CreateBuilder();
    }

    public HonamiAppBuilder RegisterAPIRoute(string path, IRoutes routes) {
        _routeBuilder.AddRoute(path, routes);
        return this;
    }

    public HonamiApp Build() {
        return new HonamiApp(_routeBuilder.Build(), _webBuilder.Build());
    }
}
