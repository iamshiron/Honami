using Shiron.Honami.HTTP.Results;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public class Server : ServerRoute {
    public override IResult Get() {
        Console.WriteLine("Get route called!");

        return Results.Ok(new {
            message = "Hello Honami!"
        });
    }
}
