namespace Shiron.Honami.HTTP.Result;

public static class HonamiResults {
    public static TextResult Ok(string message) => new(200, message);
    public static TextResult BadRequest(string message) => new(400, message);
    public static TextResult NotFound(string message) => new(404, message);
    public static TextResult InternalServerError(string message) => new(500, message);

    public static JsonResult<T> Ok<T>(T data) => new(200, data);
    public static JsonResult<T> Created<T>(T data) => new(201, data);
    public static JsonResult<T> BadRequest<T>(T data) => new(400, data);
    public static JsonResult<T> NotFound<T>(T data) => new(404, data);
    public static JsonResult<T> InternalServerError<T>(T data) => new(500, data);

    public static StatusCodeResult NoContent() => new(204);
    public static StatusCodeResult Unauthorized() => new(401);
    public static StatusCodeResult Forbidden() => new(403);

    public static FileResult File(string path, string contentType) => new(path, contentType);
    public static StreamResult Stream(Stream stream, string contentType) => new(stream, contentType);
}
