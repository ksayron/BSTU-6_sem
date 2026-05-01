using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelotCustom.json", optional: false, reloadOnChange: true);

builder
    .Services.AddOcelot()
    .AddCustomLoadBalancer<CustomLoadBalancer>(
        (serviceProvider, route, discoveryProvider) =>
            new CustomLoadBalancer(discoveryProvider.GetAsync)
    );

builder.WebHost.UseUrls("http://localhost:7000");

var app = builder.Build();
await app.UseOcelot();
await app.RunAsync();
