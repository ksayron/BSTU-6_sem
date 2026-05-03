using System.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("CelebritiesDb")
    ?? "Server=localhost,1433;Database=Celebrities;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=True;Encrypt=False";

builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
