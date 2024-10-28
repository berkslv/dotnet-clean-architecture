using System.Reflection;
using Application.Interfaces;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;
using Presentation.Filters;
using Presentation.Services;
using Serilog;
using Shared.Filters.Correlations;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddExceptionHandler<ExceptionHandleMiddleware>();
        services.AddProblemDetails();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = $"{configuration.GetValue<string>("ProjectName")} API",
            });
        });

        services.RegisterMassTransitServices(configuration);

        services.AddInternalization();

        return services;
    }

    private static IServiceCollection RegisterMassTransitServices(this IServiceCollection services, IConfiguration configuration)
    {
        var messageBroker = configuration.GetSection("MessageBroker");
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            cfg.AddConsumers(Assembly.GetExecutingAssembly());

            cfg.UsingRabbitMq((context, config) =>
            {
                config.UseSendFilter(typeof(CorrelationSendFilter<>), context);
                config.UsePublishFilter(typeof(CorrelationPublishFilter<>), context);
                config.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);

                config.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));

                config.Host(messageBroker["Host"], messageBroker["VirtualHost"], h =>
                {
                    h.Username(messageBroker["Username"]!);
                    h.Password(messageBroker["Password"]!);
                });

                config.ConfigureEndpoints(context);
            });

            cfg.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(5);
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                o.UseSqlServer().UseBusOutbox();
            });
        });

        return services;
    }

    private static void AddInternalization(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddSingleton<LocalizationMiddleware>();
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
    }

    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        var elasticSearch = builder.Configuration.GetSection("ElasticSearch");
        var index = $"{builder.Configuration.GetValue<string>("ProjectName")}-{DateTime.Today:yyyyMMdd}";
        Log.Logger = new LoggerConfiguration()
            .Enrich
            .FromLogContext()
            .WriteTo
            .Elasticsearch(
                new [] { new Uri(elasticSearch["Url"]!)}, opts =>
                {
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                })
            .ReadFrom
            .Configuration(builder.Configuration)
            .CreateLogger();

        builder.Logging.ClearProviders();

        builder.Host.UseSerilog(Log.Logger, true);

        return builder;
    }
}