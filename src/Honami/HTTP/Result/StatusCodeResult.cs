using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public readonly record struct StatusCodeResult(int StatusCode) : IHonamiResult {
    public Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;
        return Task.CompletedTask;
    }
}
