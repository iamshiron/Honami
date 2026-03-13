using Microsoft.AspNetCore.Http;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users;

public readonly record struct ResponseDTO(string Message);

public readonly record struct GetUsersRequest : IBindableRequest<GetUsersRequest> {
    public static void Bind(HttpContext context, out GetUsersRequest result) {
        result = default;
    }
}

public class GetUsersEndpoint : Endpoint<GetUsersRequest, JsonResult<ResponseDTO>> {
    public override JsonResult<ResponseDTO> Handle(GetUsersRequest request) {
        Console.WriteLine("Get route called!");
        return HonamiResults.Ok(new ResponseDTO("Hello Honami!"));
    }
}
