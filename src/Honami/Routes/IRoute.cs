namespace Shiron.Honami.Routes;

public interface IRoute {
    void Get();
    void Post();
    void Put();
    void Delete();
    void Patch();
    void Head();
    void Options();
    void Trace();
    void Connect();
}
