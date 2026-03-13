using System.Reflection;
using Shiron.Honami.HTTP;
using Shiron.Honami.Routes.RouteTypes;

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

public class RouterInvalidResultException(IRoutes Instance, MethodInfo Method)
    : RouterException($"Invalid result for {Method.Name} in {Instance.GetType().Name}") {
    public IRoutes Instance { get; init; } = Instance;
    public MethodInfo Method { get; init; } = Method;
}
