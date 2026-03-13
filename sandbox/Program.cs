using Shiron.Honami;
using Shiron.Honami.Sandbox.Routes.API.Users;

var builder = new HonamiAppBuilder();
builder.RegisterAPIRoute("/api/users", new Server());
builder.RegisterAPIRoute("/api/users/[userID]", new Shiron.Honami.Sandbox.Routes.API.Users._userID_.Server());

var app = builder.Build();
app.PrintRouteTree();

app.Run();
