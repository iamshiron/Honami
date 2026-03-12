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
}
