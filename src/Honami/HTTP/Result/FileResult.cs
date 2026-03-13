using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public readonly record struct FileResult(string Path, string ContentType) : IHonamiResult {
    public int StatusCode { get; init; } = 200;

    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;
        context.Response.ContentType = ContentType;
        await context.Response.SendFileAsync(Path);
    }
}
