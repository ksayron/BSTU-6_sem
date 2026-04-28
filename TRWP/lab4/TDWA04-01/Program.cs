var nick = args.FirstOrDefault(a => a.StartsWith("--Nick="))?.Split('=')[1] ?? "Default";
var port = args.FirstOrDefault(a => a.StartsWith("--Port="))?.Split('=')[1] ?? "5000";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://localhost:{port}");

var app = builder.Build();

app.MapGet("/A", () => Results.Json(new { Nick = nick, Method = "GET" }));
app.MapPost("/A", () => Results.Json(new { Nick = nick, Method = "POST" }));
app.MapPut("/A", () => Results.Json(new { Nick = nick, Method = "PUT" }));
app.MapDelete("/A", () => Results.Json(new { Nick = nick, Method = "DELETE" }));

app.Run();
