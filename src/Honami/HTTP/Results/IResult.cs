using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Results;

public interface IResult {
    Task ExecuteAsync(HttpContext context);
}
