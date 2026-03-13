using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes;

public interface IEndpoint;

public abstract class Endpoint<TReq, TRes> : IEndpoint
    where TReq : IBindableRequest<TReq>
    where TRes : IHonamiResult {
    public abstract TRes Handle(TReq request);
}
