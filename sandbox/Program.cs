using Shiron.Honami;
using Shiron.Honami.Sandbox.Routes.API.Users;

var builder = new HonamiAppBuilder();
builder.RegisterAPIRoute("/api/users", new Server());
builder.RegisterAPIRoute("/api/users/[userID]", new Shiron.Honami.Sandbox.Routes.API.Users._userID_.Server());
builder.RegisterMiddleware("/api/users", new Middleware());

builder.RegisterAPIRoute("/api/images", new Shiron.Honami.Sandbox.Routes.API.Images.Server());
builder.RegisterAPIRoute("/api/images/[imageID]", new Shiron.Honami.Sandbox.Routes.API.Images._imageID_.Server());

var app = builder.Build();
app.PrintRouteTree();

app.Run();
