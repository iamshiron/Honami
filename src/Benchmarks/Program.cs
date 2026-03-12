using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Shiron.Honami.Exceptions;
using Shiron.Honami.HTTP;
using Shiron.Honami.HTTP.Results;
using Shiron.Honami.Routes;

namespace Shiron.Honami.Benchmarks;

public class Program {
    public static void Main(string[] args) {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddColumn(TargetMethodColumn.Method)
            .AddColumn(StatisticColumn.Mean)
            .AddColumn(StatisticColumn.StdDev)
            .AddColumn(StatisticColumn.Error)
            .AddColumn(StatisticColumn.Min)
            .AddColumn(StatisticColumn.Max)
            .AddColumn(StatisticColumn.OperationsPerSecond)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

        BenchmarkRunner.Run<RouterBenchmarks>(config, args);
    }
}

[MemoryDiagnoser]
[RankColumn]
public class RouterBenchmarks {
    private Router _router = null!;
    private Router _largeRouter = null!;
    private Router _hugeRouter = null!;
    private HttpContext _getContext = null!;
    private HttpContext _notFoundContext = null!;

    private Dictionary<string, IRoutes> _smallRoutes = null!;
    private Dictionary<string, IRoutes> _mediumRoutes = null!;
    private Dictionary<string, IRoutes> _largeRoutes = null!;
    private Dictionary<string, IRoutes> _hugeRoutes = null!;

    [GlobalSetup]
    public void GlobalSetup() {
        _smallRoutes = GenerateRoutes(10);
        _mediumRoutes = GenerateRoutes(100);
        _largeRoutes = GenerateRoutes(1000);
        _hugeRoutes = GenerateRoutes(10000);

        _router = new Router(_smallRoutes);
        _largeRouter = new Router(_largeRoutes);
        _hugeRouter = new Router(_hugeRoutes);

        _getContext = CreateMockHttpContext("/route-0", "GET");
        _notFoundContext = CreateMockHttpContext("/nonexistent-route", "GET");
    }

    private static Dictionary<string, IRoutes> GenerateRoutes(int count) {
        var routes = new Dictionary<string, IRoutes>();
        for (var i = 0; i < count; i++) {
            var path = $"/route-{i}";
            routes[path] = new BenchmarkRoute(i);
        }
        return routes;
    }

    private static HttpContext CreateMockHttpContext(string path, string method) {
        var features = new FeatureCollection();
        var requestFeature = new HttpRequestFeature {
            Path = path,
            Method = method
        };
        var responseFeature = new HttpResponseFeature();

        features.Set<IHttpRequestFeature>(requestFeature);
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));

        return new DefaultHttpContext(features);
    }

    #region Registration Benchmarks
    [Benchmark(Description = "Register 10 routes")]
    public Router Register_10_Routes() {
        return new Router(_smallRoutes);
    }

    [Benchmark(Description = "Register 100 routes")]
    public Router Register_100_Routes() {
        return new Router(_mediumRoutes);
    }

    [Benchmark(Description = "Register 1000 routes")]
    public Router Register_1000_Routes() {
        return new Router(_largeRoutes);
    }

    [Benchmark(Description = "Register 10000 routes")]
    public Router Register_10000_Routes() {
        return new Router(_hugeRoutes);
    }
    #endregion

    #region Match Benchmarks - Small Router (10 routes)
    [Benchmark(Description = "Match GET on 10-route router")]
    public IHonamiResult Match_Get_SmallRouter() {
        return _router.Match(_getContext);
    }
    #endregion

    #region Match Benchmarks - Large Router (1000 routes)
    [Benchmark(Description = "Match GET on 1000-route router (first route)")]
    public IHonamiResult Match_FirstRoute_LargeRouter() {
        return _largeRouter.Match(CreateMockHttpContext("/route-0", "GET"));
    }

    [Benchmark(Description = "Match GET on 1000-route router (middle route)")]
    public IHonamiResult Match_MiddleRoute_LargeRouter() {
        return _largeRouter.Match(CreateMockHttpContext("/route-500", "GET"));
    }

    [Benchmark(Description = "Match GET on 1000-route router (last route)")]
    public IHonamiResult Match_LastRoute_LargeRouter() {
        return _largeRouter.Match(CreateMockHttpContext("/route-999", "GET"));
    }

    [Benchmark(Description = "Match not found on 1000-route router")]
    public IHonamiResult Match_NotFound_LargeRouter() {
        try {
            return _largeRouter.Match(_notFoundContext);
        } catch (RouterNotFoundException) {
            return new TextHonamiResult("Not Found", 404);
        }
    }
    #endregion

    #region Match Benchmarks - Huge Router (10000 routes)
    [Benchmark(Description = "Match GET on 10000-route router")]
    public IHonamiResult Match_HugeRouter() {
        return _hugeRouter.Match(CreateMockHttpContext("/route-5000", "GET"));
    }
    #endregion

    #region Throughput Benchmarks
    [Benchmark(Description = "Theoretical max throughput (1000 matches)")]
    public int Throughput_1000_Matches() {
        var count = 0;
        for (var i = 0; i < 1000; i++) {
            var path = $"/route-{i % 1000}";
            var context = CreateMockHttpContext(path, "GET");
            _largeRouter.Match(context);
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Mixed methods throughput (1000 matches)")]
    public int Throughput_MixedMethods() {
        var count = 0;
        var methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
        for (var i = 0; i < 1000; i++) {
            var path = $"/route-{i % 1000}";
            var method = methods[i % methods.Length];
            var context = CreateMockHttpContext(path, method);
            _largeRouter.Match(context);
            count++;
        }
        return count;
    }
    #endregion

    #region HTTPMethod Parsing Benchmarks
    [Benchmark(Description = "Parse GET method string")]
    public HTTPMethod? Parse_Get_Method() {
        return HTTPMethods.FromString("GET");
    }

    [Benchmark(Description = "Parse POST method string")]
    public HTTPMethod? Parse_Post_Method() {
        return HTTPMethods.FromString("POST");
    }

    [Benchmark(Description = "Parse unknown method string")]
    public HTTPMethod? Parse_Unknown_Method() {
        return HTTPMethods.FromString("UNKNOWN");
    }
    #endregion
}

public class BenchmarkRoute : IRoutes {
    private readonly int _id;

    public BenchmarkRoute(int id) {
        _id = id;
    }

    public IHonamiResult Get() {
        return new TextHonamiResult($"GET {_id}", 200);
    }
    public IHonamiResult Post() {
        return new TextHonamiResult($"POST {_id}", 201);
    }
    public IHonamiResult Put() {
        return new TextHonamiResult($"PUT {_id}", 200);
    }
    public IHonamiResult Delete() {
        return new TextHonamiResult($"DELETE {_id}", 200);
    }
    public IHonamiResult Patch() {
        return new TextHonamiResult($"PATCH {_id}", 200);
    }
    public IHonamiResult Head() {
        return new TextHonamiResult("", 200);
    }
    public IHonamiResult Options() {
        return new TextHonamiResult("OPTIONS", 200);
    }
    public IHonamiResult Trace() {
        return new TextHonamiResult("TRACE", 200);
    }
    public IHonamiResult Connect() {
        return new TextHonamiResult("CONNECT", 200);
    }
}

[MemoryDiagnoser]
public class RouterRegistrationBenchmarks {
    private Dictionary<string, IRoutes> _routes = null!;

    [Params(10, 50, 100, 500, 1000, 2000, 5000, 10000)]
    public int RouteCount { get; set; }

    [IterationSetup]
    public void IterationSetup() {
        _routes = new Dictionary<string, IRoutes>();
        for (var i = 0; i < RouteCount; i++) {
            _routes[$"/api/v{i / 100}/resource/{i % 100}"] = new BenchmarkRoute(i);
        }
    }

    [Benchmark(Description = "Register routes with varying counts")]
    public Router RegisterRoutes() {
        return new Router(_routes);
    }
}

[MemoryDiagnoser]
public class RouterMatchBenchmarks {
    private Router _router = null!;
    private HttpContext[] _contexts = null!;
    private int _index;

    [Params(10, 100, 1000, 10000)] public int RouteCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup() {
        var routes = new Dictionary<string, IRoutes>();
        for (var i = 0; i < RouteCount; i++) {
            routes[$"/route-{i}"] = new BenchmarkRoute(i);
        }
        _router = new Router(routes);

        _contexts = new HttpContext[RouteCount];
        for (var i = 0; i < RouteCount; i++) {
            _contexts[i] = CreateMockHttpContext($"/route-{i}", "GET");
        }
    }

    [Benchmark(Description = "Match random route")]
    public IHonamiResult MatchRandomRoute() {
        _index = (_index + 1) % RouteCount;
        return _router.Match(_contexts[_index]);
    }

    private static HttpContext CreateMockHttpContext(string path, string method) {
        var features = new FeatureCollection();
        var requestFeature = new HttpRequestFeature {
            Path = path,
            Method = method
        };
        var responseFeature = new HttpResponseFeature();

        features.Set<IHttpRequestFeature>(requestFeature);
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));

        return new DefaultHttpContext(features);
    }
}

[MemoryDiagnoser]
public class EndpointLookupBenchmarks {
    private Dictionary<string, RouteCallback> _endpoints = null!;
    private string[] _keys = null!;
    private int _index;

    [Params(100, 1000, 10000)] public int EndpointCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup() {
        _endpoints = new Dictionary<string, RouteCallback>();
        _keys = new string[EndpointCount];
        var route = new BenchmarkRoute(0);
        var method = typeof(BenchmarkRoute).GetMethod("Get")!;

        for (var i = 0; i < EndpointCount; i++) {
            var key = $"/route-{i}";
            _endpoints[key] = new RouteCallback(route, method);
            _keys[i] = key;
        }
    }

    [Benchmark(Description = "Dictionary lookup by key")]
    public bool DictionaryLookup() {
        _index = (_index + 1) % EndpointCount;
        return _endpoints.TryGetValue(_keys[_index], out _);
    }

    [Benchmark(Description = "Dictionary lookup with non-existent key")]
    public bool DictionaryLookupNotFound() {
        return _endpoints.TryGetValue("/nonexistent-route", out _);
    }
}

[MemoryDiagnoser]
public class ReflectionBenchmarks {
    private BenchmarkRoute _route = null!;
    private MethodInfo _method = null!;
    private readonly object?[] _emptyArgs = Array.Empty<object?>();

    [GlobalSetup]
    public void GlobalSetup() {
        _route = new BenchmarkRoute(0);
        _method = typeof(BenchmarkRoute).GetMethod("Get")!;
    }

    [Benchmark(Description = "MethodInfo.Invoke with no arguments")]
    public object? ReflectionInvoke() {
        return _method.Invoke(_route, _emptyArgs);
    }

    [Benchmark(Description = "MethodInfo.Invoke with null args")]
    public object? ReflectionInvokeNull() {
        return _method.Invoke(_route, null);
    }
}

[MemoryDiagnoser]
public class HttpContextCreationBenchmarks {
    [Benchmark(Description = "Create mock HttpContext")]
    public HttpContext CreateHttpContext() {
        var features = new FeatureCollection();
        var requestFeature = new HttpRequestFeature {
            Path = "/test",
            Method = "GET"
        };
        var responseFeature = new HttpResponseFeature();

        features.Set<IHttpRequestFeature>(requestFeature);
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));

        return new DefaultHttpContext(features);
    }

    [Benchmark(Description = "Create HttpContext and match")]
    public IHonamiResult CreateAndMatch() {
        var router = new Router(new Dictionary<string, IRoutes> {
            ["/test"] = new BenchmarkRoute(0)
        });

        var context = CreateHttpContext();
        return router.Match(context);
    }
}

[MemoryDiagnoser]
public class MaxThroughputBenchmarks {
    private Router _router = null!;
    private readonly string[] _paths = new string[1000];
    private readonly HttpContext[] _cachedContexts = new HttpContext[1000];

    [GlobalSetup]
    public void GlobalSetup() {
        var routes = new Dictionary<string, IRoutes>();
        for (var i = 0; i < 1000; i++) {
            var path = $"/route-{i}";
            routes[path] = new BenchmarkRoute(i);
            _paths[i] = path;
        }
        _router = new Router(routes);

        for (var i = 0; i < 1000; i++) {
            _cachedContexts[i] = CreateMockHttpContext(_paths[i], "GET");
        }
    }

    [Benchmark(Description = "Max throughput with cached HttpContexts")]
    public int MaxThroughput_CachedContexts() {
        var count = 0;
        for (var i = 0; i < 1000; i++) {
            _router.Match(_cachedContexts[i]);
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Max throughput with new HttpContexts")]
    public int MaxThroughput_NewContexts() {
        var count = 0;
        for (var i = 0; i < 1000; i++) {
            var context = CreateMockHttpContext(_paths[i], "GET");
            _router.Match(context);
            count++;
        }
        return count;
    }

    private static HttpContext CreateMockHttpContext(string path, string method) {
        var features = new FeatureCollection();
        var requestFeature = new HttpRequestFeature {
            Path = path,
            Method = method
        };
        var responseFeature = new HttpResponseFeature();

        features.Set<IHttpRequestFeature>(requestFeature);
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));

        return new DefaultHttpContext(features);
    }
}
