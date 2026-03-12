namespace Shiron.Honami.HTTP.Results;

public static class HonamiResults {
    public static IHonamiResult Ok(string message) {
        return new TextHonamiResult(message, 200);
    }
    public static IHonamiResult BadRequest(string message) {
        return new TextHonamiResult(message, 400);
    }
    public static IHonamiResult NotFound(string message) {
        return new TextHonamiResult(message, 404);
    }

    public static IHonamiResult Ok(object data) {
        return new JsonHonamiResult(data, 200);
    }
    public static IHonamiResult BadRequest(object data) {
        return new JsonHonamiResult(data, 400);
    }
    public static IHonamiResult NotFound(object data) {
        return new JsonHonamiResult(data, 404);
    }
}
