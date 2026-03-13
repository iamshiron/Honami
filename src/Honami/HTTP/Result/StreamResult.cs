using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public readonly record struct StreamResult(Stream Stream, string ContentType) : IHonamiResult {
    public int StatusCode { get; init; } = 200;

    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;
        context.Response.ContentType = ContentType;
        await Stream.CopyToAsync(context.Response.Body);
        await Stream.DisposeAsync();
    }
}
