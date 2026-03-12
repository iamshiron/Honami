namespace Shiron.Honami.HTTP.Request;

public class HonamiRequest(Dictionary<string, string> urlParams) {
    public Dictionary<string, string> UrlParams { get; } = urlParams;
}
