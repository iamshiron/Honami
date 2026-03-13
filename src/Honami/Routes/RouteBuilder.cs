using Shiron.Honami.HTTP;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Routes;

public class RouteBuilder {
    private readonly Dictionary<HTTPMethod, Dictionary<string, IEndpoint>> _endpoints = new() {
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

    private readonly List<(string path, ServerMiddleware middleware)> _middlewares = [];

    public RouteBuilder MapGet<T>(string path) where T : IEndpoint, new() {
        _endpoints[HTTPMethod.Get][path] = new T();
        return this;
    }

    public RouteBuilder MapPost<T>(string path) where T : IEndpoint, new() {
        _endpoints[HTTPMethod.Post][path] = new T();
        return this;
    }

    public RouteBuilder MapPut<T>(string path) where T : IEndpoint, new() {
        _endpoints[HTTPMethod.Put][path] = new T();
        return this;
    }

    public RouteBuilder MapDelete<T>(string path) where T : IEndpoint, new() {
        _endpoints[HTTPMethod.Delete][path] = new T();
        return this;
    }

    public RouteBuilder MapPatch<T>(string path) where T : IEndpoint, new() {
        _endpoints[HTTPMethod.Patch][path] = new T();
        return this;
    }

    public RouteBuilder MapGet(string path, IEndpoint endpoint) {
        _endpoints[HTTPMethod.Get][path] = endpoint;
        return this;
    }

    public RouteBuilder MapPost(string path, IEndpoint endpoint) {
        _endpoints[HTTPMethod.Post][path] = endpoint;
        return this;
    }

    public RouteBuilder MapPut(string path, IEndpoint endpoint) {
        _endpoints[HTTPMethod.Put][path] = endpoint;
        return this;
    }

    public RouteBuilder MapDelete(string path, IEndpoint endpoint) {
        _endpoints[HTTPMethod.Delete][path] = endpoint;
        return this;
    }

    public RouteBuilder MapPatch(string path, IEndpoint endpoint) {
        _endpoints[HTTPMethod.Patch][path] = endpoint;
        return this;
    }

    public RouteBuilder UseMiddleware(string path, ServerMiddleware middleware) {
        _middlewares.Add((path, middleware));
        return this;
    }

    public Router Build() {
        return new Router(_endpoints, _middlewares);
    }
}
