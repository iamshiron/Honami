using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes.RouteTypes;

namespace Shiron.Honami.Sandbox.Routes.API.Images._imageID_;

public readonly record struct ImageResultDTO(string ImageID, int Width, int Height);

public class Server : ServerRoutes {
    public override HonamiResult Get(HonamiRequest request) {
        return HonamiResults.Ok(new ImageResultDTO(request.UrlParams["imageID"], 1910, 280));
    }
}
