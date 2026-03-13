using Microsoft.AspNetCore.Http;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public class Middleware : ServerMiddleware {
    public override async Task ExecuteAsync(HttpContext context, MiddlewareDelegate next) {
        Console.WriteLine("Middleware executed!");
        await next(context);
    }
}
