using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Results;

public interface IHonamiResult {
    Task ExecuteAsync(HttpContext context);
}
