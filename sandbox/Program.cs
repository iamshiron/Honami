using Shiron.Honami;
using Shiron.Honami.Sandbox.Routes.API.Images;
using Shiron.Honami.Sandbox.Routes.API.Users;
using Shiron.Honami.Sandbox.Routes.API.Users._userID_;
using Shiron.Honami.Sandbox.Routes.API.Images._imageID_;

var builder = new HonamiAppBuilder();
builder.MapGet<GetUsersEndpoint>("/api/users");
builder.MapGet<GetUserEndpoint>("/api/users/[userID]");
builder.UseMiddleware("/api/users", new Middleware());

builder.MapGet<GetImagesEndpoint>("/api/images");
builder.MapGet<GetImageEndpoint>("/api/images/[imageID]");

var app = builder.Build();
app.PrintRouteTree();

app.Run();
