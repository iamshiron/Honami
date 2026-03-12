namespace Shiron.Honami.HTTP.Results;

public static class HonamiResults {
    public static HonamiResult Ok(string message) {
        return new HonamiResult(200, message, ResultType.Text);
    }
    public static HonamiResult BadRequest(string message) {
        return new HonamiResult(400, message, ResultType.Text);
    }
    public static HonamiResult NotFound(string message) {
        return new HonamiResult(404, message, ResultType.Text);
    }
    public static HonamiResult InternalServerError(string message) {
        return new HonamiResult(500, message, ResultType.Text);
    }

    public static HonamiResult Ok(object data) {
        return new HonamiResult(200, data, ResultType.Json);
    }
    public static HonamiResult BadRequest(object data) {
        return new HonamiResult(400, data, ResultType.Json);
    }
    public static HonamiResult NotFound(object data) {
        return new HonamiResult(404, data, ResultType.Json);
    }
    public static HonamiResult InternalServerError(object data) {
        return new HonamiResult(500, data, ResultType.Json);
    }
}
