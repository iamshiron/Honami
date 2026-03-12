namespace Shiron.Honami.HTTP;

public enum HTTPMethod {
    Get,
    Post,
    Put,
    Delete,
    Patch,
    Head,
    Options,
    Trace,
    Connect
}

public static class HTTPMethods {
    public static HTTPMethod[] All { get; } = [
        HTTPMethod.Get,
        HTTPMethod.Post,
        HTTPMethod.Put,
        HTTPMethod.Delete,
        HTTPMethod.Patch,
        HTTPMethod.Head,
        HTTPMethod.Options,
        HTTPMethod.Trace,
        HTTPMethod.Connect
    ];

    public static HTTPMethod? FromString(string s) {
        switch (s.ToLower()) {
            case "get":
                return HTTPMethod.Get;
            case "post":
                return HTTPMethod.Post;
            case "put":
                return HTTPMethod.Put;
            case "delete":
                return HTTPMethod.Delete;
            case "patch":
                return HTTPMethod.Patch;
            case "head":
                return HTTPMethod.Head;
            case "options":
                return HTTPMethod.Options;
            case "trace":
                return HTTPMethod.Trace;
            case "connect":
                return HTTPMethod.Connect;
            default:
                return null;
        }
    }
}
