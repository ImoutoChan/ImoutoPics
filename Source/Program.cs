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
            .ConfigureAppConfiguration((context, builder) =>
            {
                var azureAppConfiguration = builder.Build().GetConnectionString("AzureConfig");

                if (context.HostingEnvironment.IsProduction())
                    builder.AddAzureAppConfiguration(x => x
                        .Connect(azureAppConfiguration)
                        .Select("*", Environments.Production));

                builder
                    .AddJsonFile("appsettings.Cache.json.backup", true)
                    .AddJsonFile("appsettings.Cache.json", true);
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureSerilog()
            .ConfigureServices(x => x.AddHostedService<TelegramHostedService>())
            .ConfigureServices(x => x.AddQuartzHostedService());
}
