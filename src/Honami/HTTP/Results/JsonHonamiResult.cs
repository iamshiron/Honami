using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Results;

public class JsonHonamiResult : IHonamiResult {
    private readonly object _data;
    private readonly int _statusCode;

    public JsonHonamiResult(object data, int statusCode) {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = _statusCode;
        context.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(context.Response.Body, _data);
    }
}
