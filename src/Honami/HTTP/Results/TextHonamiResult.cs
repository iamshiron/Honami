using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Results;

public class TextHonamiResult : IHonamiResult {
    private readonly string _data;
    private readonly int _statusCode;

    public TextHonamiResult(string data, int statusCode) {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = _statusCode;
        context.Response.ContentType = "text/text";
        await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(_data));
    }
}
