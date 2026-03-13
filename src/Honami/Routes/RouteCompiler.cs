using System.Linq.Expressions;
using System.Reflection;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes;

public delegate HonamiResult RouteHandlerDelegate(object instance, HonamiRequest request);

public static class RouteCompiler {
    public static RouteHandlerDelegate CompileRoute(Type routeType, MethodInfo methodInfo) {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var requestParam = Expression.Parameter(typeof(HonamiRequest), "request");
        var castInstance = Expression.Convert(instanceParam, routeType);
        var methodCall = Expression.Call(castInstance, methodInfo, requestParam);
        var lambda = Expression.Lambda<RouteHandlerDelegate>(methodCall, instanceParam, requestParam);
        return lambda.Compile();
    }
}
