using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Request;

public class HonamiRequest(Dictionary<string, string> urlParams, IHeaderDictionary headers) {
    public IHeaderDictionary Headers { get; } = headers;
    public Dictionary<string, string> UrlParams { get; } = urlParams;
}
