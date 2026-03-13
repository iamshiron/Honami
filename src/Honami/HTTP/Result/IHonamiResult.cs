using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Result;

public enum ResultType {
    Json,
    Text,
    Utf8Text,
    StatusCodeOnly,
    File,
    Stream,
    MiddlewarePass
}

public readonly record struct HonamiResult(int StatusCode, object? Payload, ResultType Type) {
    public async Task ExecuteAsync(HttpContext context) {
        context.Response.StatusCode = StatusCode;

        switch (Type) {
            case ResultType.Json:
                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, Payload);
                break;

            case ResultType.Text:
                if (Payload is null) {
                    break;
                }
                if (Payload is not string textValue) {
                    throw new InvalidOperationException(
                        $"HonamiResult state corrupted: Expected string for ResultType.Text, but got {Payload.GetType().Name}.");
                }

                if (textValue.Length == 0) {
                    break;
                }

                var writer = context.Response.BodyWriter;
                var byteCount = Encoding.UTF8.GetByteCount(textValue);
                var span = writer.GetSpan(byteCount);
                Encoding.UTF8.GetBytes(textValue, span);
                writer.Advance(byteCount);
                break;

            case ResultType.File:
                if (Payload is Tuple<string, string> fileData) {
                    var (path, contentType) = fileData;
                    context.Response.ContentType = contentType;
                    await context.Response.SendFileAsync(path);
                }
                break;

            case ResultType.Stream:
                if (Payload is Tuple<Stream, string> streamData) {
                    var (stream, contentType) = streamData;
                    context.Response.ContentType = contentType;
                    await stream.CopyToAsync(context.Response.Body);
                    await stream.DisposeAsync();
                }
                break;
        }
    }
}
