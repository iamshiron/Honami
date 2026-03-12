using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shiron.Honami.HTTP;
using Shiron.Honami.Routes;
using IResult = Shiron.Honami.HTTP.Results.IResult;

namespace Shiron.Honami;

public class HonamiApp(Router router, WebApplication webApp) {
    public Router Router { get; } = router;
    public WebApplication WebApp { get; } = webApp;

    public void Run() {
        WebApp.Run(async context => {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
            var methodString = context.Request.Method;

            if (!Enum.TryParse<HTTPMethod>(methodString, true, out var httpMethod)) {
                context.Response.StatusCode = 405;
                await context.Response.WriteAsync("Method Not Allowed");
                return;
            }

            if (Router.Endpoints.TryGetValue(httpMethod, out var pathMap) &&
                pathMap.TryGetValue(path, out var routeInstance)) {
                var methodInfo = routeInstance.GetType().GetMethod(httpMethod.ToString());
                var result = (IResult?) methodInfo?.Invoke(routeInstance, null);

                if (result != null) {
                    await result.ExecuteAsync(context);
                } else {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Route returned an invalid result.");
                }
                return;
            }

            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Honami: No route found for {methodString} {path}");
        });

        WebApp.Run("http://127.0.0.1:5000");
    }

    public void PrintRouteTree() {
        foreach (var method in Router.Endpoints) {
            Console.WriteLine($"HTTP Method: {method.Key}");
            foreach (var route in method.Value) {
                Console.WriteLine($"  Path: {route.Key}");
            }
        }
    }
}
