using Microsoft.AspNetCore.Http;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users._userID_;

public readonly record struct UserResultDTO(string ID, string Name, string RequestUserAgent);

public readonly record struct GetUserRequest(string UserId) : IBindableRequest<GetUserRequest> {
    public static void Bind(HttpContext context, out GetUserRequest result) {
        var routeParams = context.Items["RouteParams"] as Dictionary<string, string>;
        var userId = routeParams!["userID"];
        result = new GetUserRequest(userId);
    }
}

public class GetUserEndpoint : Endpoint<GetUserRequest, JsonResult<UserResultDTO>> {
    public override JsonResult<UserResultDTO> Handle(GetUserRequest request) {
        return HonamiResults.Ok(new UserResultDTO(request.UserId, "Honami User",
            request.UserId));
    }
}
