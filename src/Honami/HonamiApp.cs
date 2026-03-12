using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shiron.Honami.Exceptions;
using Shiron.Honami.Routes;

namespace Shiron.Honami;

public class HonamiApp(Router router, WebApplication webApp) {
    public Router Router { get; } = router;
    public WebApplication WebApp { get; } = webApp;

    public void Run() {
        WebApp.Run(async context => {
            try {
                var result = Router.Match(context);
                await result.ExecuteAsync(context);
            } catch (RouterNotFoundException) {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
            } catch (RouterInvalidHttpMethodException e) {
                WebApp.Logger.LogError(e, $"Invalid HTTP method: '{e.Method}'");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid HTTP method");
            } catch (Exception e) {
                WebApp.Logger.LogError(e, "Unknown exception: {}", e.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
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
