using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public interface IHonamiResult {
    int StatusCode { get; }
    Task ExecuteAsync(HttpContext context);
}
