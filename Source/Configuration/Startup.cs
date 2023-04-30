using ImoutoPicsBot.Data;
using ImoutoPicsBot.ImageProcessing;
using ImoutoPicsBot.QuartzJobs;
using LiteDB;
using Microsoft.OpenApi.Models;
using Quartz;

namespace ImoutoPicsBot.Configuration;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ImoutoPicsBot", Version = "v1" });
        });
        services.AddHealthChecks();

        services.AddHttpClient();
        services.AddTransient<ITelegramBotClient>(
            x =>
            {
                var client = x.GetRequiredService<HttpClient>();
                var apiKey = Configuration.GetRequiredValue<string>("TelegramApiKey");

                return new TelegramBotClient(apiKey, client);
            });
        services.AddMemoryCache();

        // mediator
        services.AddMediatR(typeof(Startup));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // quartz
        services.AddQuartz(c =>
        {
            c.UseMicrosoftDependencyInjectionJobFactory();

            c.AddJob<PostingJob>(j => j.WithIdentity(nameof(PostingJob)));
            c.AddTrigger(t => t
                .ForJob(nameof(PostingJob))
                .StartNow()
                .WithSimpleSchedule(b => b.WithIntervalInSeconds(20).RepeatForever()));
        });

        // data

        var file = new FileInfo(Path.Combine("idata", "media.db"));
        file.Directory?.Create();

        var connectionString = $"Filename={file.FullName};Mode=Shared";
        services.AddSingleton<ILiteDatabase>(x => new LiteDatabase(connectionString));
        services.AddTransient<IPostInfoRepository, PostInfoRepository>();
        services.AddTransient<IMediaRepository, MediaRepository>();
        services.AddTransient<IFileStorageRepository, FileStorageRepository>();
        services.AddTransient<ITelegramPreparer, TelegramPreparer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Imouto Pics Bot"));
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}
