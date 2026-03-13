using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public interface IResultFilter {
    IHonamiResult OnResultExecuting(HttpContext context, IHonamiResult result);
}
