using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes.RouteTypes;

public abstract class ServerMiddleware {
    public abstract Task<HonamiResult> ExecuteAsync(HonamiRequest request);
}
