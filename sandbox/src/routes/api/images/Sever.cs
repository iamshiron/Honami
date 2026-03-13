using Microsoft.AspNetCore.Http;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Images;

public readonly record struct ImagesResultDTO(string[] Images);

public readonly record struct GetImagesRequest : IBindableRequest<GetImagesRequest> {
    public static void Bind(HttpContext context, out GetImagesRequest result) {
        result = default;
    }
}

public class GetImagesEndpoint : Endpoint<GetImagesRequest, JsonResult<ImagesResultDTO>> {
    public override JsonResult<ImagesResultDTO> Handle(GetImagesRequest request) {
        return HonamiResults.Ok(new ImagesResultDTO([
            "image-1", "image-2", "image-3"
        ]));
    }
}
