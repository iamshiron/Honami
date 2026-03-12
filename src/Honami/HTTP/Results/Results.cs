namespace Shiron.Honami.HTTP.Results;

public static class Results {
    public static IResult Ok(object data) {
        return new JsonResult(data, 200);
    }
    public static IResult BadRequest(object data) {
        return new JsonResult(data, 400);
    }
}
