using Shiron.Honami.HTTP.Results;

namespace Shiron.Honami.Routes;

public interface IRoute {
    IResult Get();
    IResult Post();
    IResult Put();
    IResult Delete();
    IResult Patch();
    IResult Head();
    IResult Options();
    IResult Trace();
    IResult Connect();
}
