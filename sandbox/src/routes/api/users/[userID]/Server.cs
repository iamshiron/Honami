using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Sandbox.Routes.API.Users._userID_;

public readonly record struct UserResultDTO(string ID, string Name);

public class Server : ServerRoutes {
    public override HonamiResult Get(HonamiRequest request) {
        var userID = request.UrlParams["userID"];
        return HonamiResults.Ok(new UserResultDTO(userID, "Honami User"));
    }
}
