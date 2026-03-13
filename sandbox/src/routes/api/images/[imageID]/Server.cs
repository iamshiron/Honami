using Microsoft.AspNetCore.Http;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Images._imageID_;

public readonly record struct ImageResultDTO(string ImageID, int Width, int Height);

public readonly record struct GetImageRequest(string ImageId) : IBindableRequest<GetImageRequest> {
    public static void Bind(HttpContext context, out GetImageRequest result) {
        var routeParams = context.Items["RouteParams"] as Dictionary<string, string>;
        var imageId = routeParams!["imageID"];
        result = new GetImageRequest(imageId);
    }
}

public class GetImageEndpoint : Endpoint<GetImageRequest, JsonResult<ImageResultDTO>> {
    public override JsonResult<ImageResultDTO> Handle(GetImageRequest request) {
        return HonamiResults.Ok(new ImageResultDTO(request.ImageId, 1910, 280));
    }
}
