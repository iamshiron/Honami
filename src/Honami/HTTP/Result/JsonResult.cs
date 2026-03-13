using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public readonly record struct JsonResult<T>(int StatusCode, T Payload) : IHonamiResult {
    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;
        context.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(context.Response.Body, Payload);
    }
}
