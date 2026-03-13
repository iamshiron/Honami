using Microsoft.AspNetCore.Builder;
using Shiron.Honami.Routes;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami;

public class HonamiAppBuilder {
    private readonly RouteBuilder _routeBuilder = new();
    private readonly WebApplicationBuilder _webBuilder;

    public HonamiAppBuilder() {
        _webBuilder = WebApplication.CreateBuilder();
    }

    public HonamiAppBuilder MapGet<T>(string path) where T : IEndpoint, new() {
        _routeBuilder.MapGet<T>(path);
        return this;
    }

    public HonamiAppBuilder MapPost<T>(string path) where T : IEndpoint, new() {
        _routeBuilder.MapPost<T>(path);
        return this;
    }

    public HonamiAppBuilder MapPut<T>(string path) where T : IEndpoint, new() {
        _routeBuilder.MapPut<T>(path);
        return this;
    }

    public HonamiAppBuilder MapDelete<T>(string path) where T : IEndpoint, new() {
        _routeBuilder.MapDelete<T>(path);
        return this;
    }

    public HonamiAppBuilder MapPatch<T>(string path) where T : IEndpoint, new() {
        _routeBuilder.MapPatch<T>(path);
        return this;
    }

    public HonamiAppBuilder UseMiddleware(string path, ServerMiddleware middleware) {
        _routeBuilder.UseMiddleware(path, middleware);
        return this;
    }

    public HonamiApp Build() {
        return new HonamiApp(_routeBuilder.Build(), _webBuilder.Build());
    }
}
