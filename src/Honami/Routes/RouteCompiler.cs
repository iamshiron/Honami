using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Shiron.Honami.HTTP.Request;
using Shiron.Honami.HTTP.Result;

namespace Shiron.Honami.Routes;

public delegate Task RouteHandlerDelegate(IEndpoint instance, HttpContext context);
public delegate Task<IHonamiResult> FlexibleRouteHandlerDelegate(IEndpoint instance, HttpContext context);

public static class RouteCompiler {
    public static RouteHandlerDelegate CompileFastPath(Type endpointType) {
        var baseType = endpointType.BaseType!;
        var reqType = baseType.GetGenericArguments()[0];
        var resType = baseType.GetGenericArguments()[1];

        var instanceParam = Expression.Parameter(typeof(IEndpoint), "instance");
        var contextParam = Expression.Parameter(typeof(HttpContext), "context");

        var typedInstance = Expression.Convert(instanceParam, endpointType);
        var requestVar = Expression.Variable(reqType, "request");

        var bindMethod = reqType.GetMethod("Bind")!;
        var bindCall = Expression.Call(bindMethod, contextParam, requestVar);

        var handleMethod = endpointType.GetMethod("Handle")!;
        var handleCall = Expression.Call(typedInstance, handleMethod, requestVar);

        var executeMethod = resType.GetMethod("ExecuteAsync")!;
        var executeCall = Expression.Call(handleCall, executeMethod, contextParam);

        var block = Expression.Block(new[] { requestVar }, bindCall, executeCall);

        var lambda = Expression.Lambda<RouteHandlerDelegate>(block, instanceParam, contextParam);
        return lambda.Compile();
    }

    public static FlexibleRouteHandlerDelegate CompileFlexiblePath(Type endpointType) {
        var baseType = endpointType.BaseType!;
        var reqType = baseType.GetGenericArguments()[0];

        var instanceParam = Expression.Parameter(typeof(IEndpoint), "instance");
        var contextParam = Expression.Parameter(typeof(HttpContext), "context");

        var typedInstance = Expression.Convert(instanceParam, endpointType);
        var requestVar = Expression.Variable(reqType, "request");

        var bindMethod = reqType.GetMethod("Bind")!;
        var bindCall = Expression.Call(bindMethod, contextParam, requestVar);

        var handleMethod = endpointType.GetMethod("Handle")!;
        var handleCall = Expression.Call(typedInstance, handleMethod, requestVar);
        var boxedResult = Expression.Convert(handleCall, typeof(IHonamiResult));

        var taskFromResultCall = Expression.Call(
            typeof(Task),
            nameof(Task.FromResult),
            new[] { typeof(IHonamiResult) },
            boxedResult
        );

        var block = Expression.Block(new[] { requestVar }, bindCall, taskFromResultCall);

        var lambda = Expression.Lambda<FlexibleRouteHandlerDelegate>(block, instanceParam, contextParam);
        return lambda.Compile();
    }
}
