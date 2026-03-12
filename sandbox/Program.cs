using Shiron.Honami;
using Shiron.Honami.Sandbox.Routes.API.Users;

var builder = new HonamiAppBuilder();
builder.RegisterRoute("/api/users", new Server());

var app = builder.Build();
app.PrintRouteTree();

app.Run();
