using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Sandbox.Routes.API.Images;

public readonly record struct ImagesResultDTO(string[] Images);

public class Server : ServerRoutes {
    public override HonamiResult Get(HonamiRequest request) {
        return HonamiResults.Ok(new ImagesResultDTO([
            "image-1", "image-2", "image-3"
        ]));
    }
}
