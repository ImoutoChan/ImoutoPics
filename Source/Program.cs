global using MediatR;
global using Telegram.Bot;
using ImoutoPicsBot.Configuration;
using Quartz;

namespace ImoutoPicsBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureSerilog()
            .ConfigureServices(x => x.AddHostedService<TelegramHostedService>())
            .ConfigureServices(x => x.AddQuartzHostedService());
}
