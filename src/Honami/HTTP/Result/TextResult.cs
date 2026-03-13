using System.Text;
using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public readonly record struct TextResult(int StatusCode, string Text) : IHonamiResult {
    public Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;
        if (string.IsNullOrEmpty(Text)) {
            return Task.CompletedTask;
        }
        context.Response.ContentType = "text/plain";
        var writer = context.Response.BodyWriter;
        var byteCount = Encoding.UTF8.GetByteCount(Text);
        var span = writer.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(Text, span);
        writer.Advance(byteCount);
        return Task.CompletedTask;
    }
}
