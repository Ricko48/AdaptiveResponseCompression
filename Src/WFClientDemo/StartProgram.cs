using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WFClientDemo;

public class StartProgram : BackgroundService
{
    private readonly IServiceProvider _services;
    public StartProgram(IServiceProvider services)
    {
        _services = services;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(_services.GetRequiredService<Form1>());
        return Task.CompletedTask;
    }
}
