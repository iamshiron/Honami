using Shiron.Honami.HTTP.Results;

namespace Shiron.Honami.Routes;

public interface IRoutes {
    HonamiResult Get();
    HonamiResult Post();
    HonamiResult Put();
    HonamiResult Delete();
    HonamiResult Patch();
    HonamiResult Head();
    HonamiResult Options();
    HonamiResult Trace();
    HonamiResult Connect();
}
