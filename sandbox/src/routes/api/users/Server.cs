using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public class Server : ServerRoute {
    public override void Get() {
        Console.WriteLine("Get route called!");
    }
}
