using System.Linq.Expressions;
using System.Reflection;
using Shiron.Honami.HTTP.Results;

namespace Shiron.Honami.Routes;

public delegate HonamiResult RouteHandlerDelegate(object instance);

public static class RouteCompiler {
    public static RouteHandlerDelegate CompileRoute(Type routeType, MethodInfo methodInfo) {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var castInstance = Expression.Convert(instanceParam, routeType);
        var methodCall = Expression.Call(castInstance, methodInfo);
        var lambda = Expression.Lambda<RouteHandlerDelegate>(methodCall, instanceParam);
        return lambda.Compile();
    }
}
