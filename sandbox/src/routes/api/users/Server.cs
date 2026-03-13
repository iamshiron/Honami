using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public readonly record struct ResponseDTO(string Message);

public class Server : ServerRoutes {
    public override HonamiResult Get(HonamiRequest request) {
        Console.WriteLine("Get route called!");

        return HonamiResults.Ok(new ResponseDTO("Hello Honami!"));
    }
}
