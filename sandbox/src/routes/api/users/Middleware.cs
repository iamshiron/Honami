using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public class Middleware : ServerMiddleware {
    public override Task<HonamiResult> ExecuteAsync(HonamiRequest request) {
        Console.WriteLine($"Middleware executed!");

        return Task.FromResult(HonamiResults.MiddlewarePass());
    }
}
