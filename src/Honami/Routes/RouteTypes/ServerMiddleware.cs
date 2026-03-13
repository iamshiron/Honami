using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.Routes.RouteTypes;

public delegate Task MiddlewareDelegate(HttpContext context);

public abstract class ServerMiddleware {
    public abstract Task ExecuteAsync(HttpContext context, MiddlewareDelegate next);
}
