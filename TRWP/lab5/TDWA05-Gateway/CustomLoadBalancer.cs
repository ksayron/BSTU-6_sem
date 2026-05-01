using Microsoft.AspNetCore.Http;
using Ocelot.LoadBalancer.Interfaces;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;

public class CustomLoadBalancer : ILoadBalancer
{
    private readonly Func<Task<List<Service>>> _services;
    private readonly object _lock = new();
    private readonly Random _rng = new();

    // Weights: index 0 = X = 50%, index 1 = Y = 30%, index 2 = Z = 20%
    private readonly int[] _weights = [50, 30, 20];
    private readonly int _total = 100;

    public string Type => nameof(CustomLoadBalancer);

    public CustomLoadBalancer(Func<Task<List<Service>>> services)
    {
        _services = services;
    }

    public async Task<Response<ServiceHostAndPort>> LeaseAsync(HttpContext httpContext)
    {
        var services = await _services.Invoke();

        lock (_lock)
        {
            var roll = _rng.Next(_total);
            var cumulative = 0;

            for (int i = 0; i < services.Count; i++)
            {
                var weight = i < _weights.Length ? _weights[i] : _total / services.Count;
                cumulative += weight;

                if (roll < cumulative)
                    return new OkResponse<ServiceHostAndPort>(services[i].HostAndPort);
            }

            return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
        }
    }

    public void Release(ServiceHostAndPort hostAndPort) { }
}