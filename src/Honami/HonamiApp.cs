using Shiron.Honami.Routes;

namespace Shiron.Honami;

public class HonamiApp(Router router) {
    public Router Router { get; } = router;

    public void Run() {
        Console.WriteLine("Running app...");
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
