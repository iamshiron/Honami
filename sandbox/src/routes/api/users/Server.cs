using Shiron.Honami.HTTP.Results;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public class Server : ServerRoutes {
    public override IHonamiResult Get() {
        Console.WriteLine("Get route called!");

        return HonamiResults.Ok(new {
            message = "Hello Honami!"
        });
    }
}
