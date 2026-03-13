using Shiron.Honami.HTTP;

namespace Shiron.Honami.Exceptions;

public class RouterException(string message) : Exception(message) {
}

public class RouterNotFoundException(string path, HTTPMethod method) : RouterException($"Route not found for {method} {path}") {
    public string Path { get; init; } = path;
    public HTTPMethod Method { get; init; } = method;
}

public class RouterInvalidHttpMethodException(string method) : RouterException($"Invalid HTTP method: {method}") {
    public string Method { get; init; } = method;
}
