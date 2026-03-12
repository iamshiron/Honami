using Shiron.Honami.HTTP.Results;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public readonly record struct ResponseDTO(string Message);

public class Server : ServerRoutes {
    public override HonamiResult Get() {
        Console.WriteLine("Get route called!");

        return HonamiResults.Ok(new ResponseDTO("Hello Honami!"));
    }
}
