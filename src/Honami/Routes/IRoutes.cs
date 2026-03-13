using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes;

public interface IRoutes {
    HonamiResult Get(HonamiRequest request);
    HonamiResult Post(HonamiRequest request);
    HonamiResult Put(HonamiRequest request);
    HonamiResult Delete(HonamiRequest request);
    HonamiResult Patch(HonamiRequest request);
    HonamiResult Head(HonamiRequest request);
    HonamiResult Options(HonamiRequest request);
    HonamiResult Trace(HonamiRequest request);
    HonamiResult Connect(HonamiRequest request);
}
