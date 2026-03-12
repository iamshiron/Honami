namespace Shiron.Honami.Routes;

public enum RouteSegmentType {
    Literal,
    Dynamic,
    CatchAll
}

public readonly record struct RouteSegment(string Name, RouteSegmentType Type);
