using Shiron.Honami.HTTP.Results;

namespace Shiron.Honami.Routes;

public interface IRoutes {
    IHonamiResult Get();
    IHonamiResult Post();
    IHonamiResult Put();
    IHonamiResult Delete();
    IHonamiResult Patch();
    IHonamiResult Head();
    IHonamiResult Options();
    IHonamiResult Trace();
    IHonamiResult Connect();
}
