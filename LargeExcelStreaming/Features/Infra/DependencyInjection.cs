using System.Reflection;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LargeExcelStreaming.Features.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services)
    {
        services.AddPooledDbContextFactory<AppDbContext>(ConfigureOptions)
            .AddDbContextPool<IDbContext, AppDbContext>(ConfigureOptions);

        return services;
    }
    
    private static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder builder)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("MainConnection");

        builder.UseExceptionProcessor();

        if (sp.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            builder.ConfigureWarnings(w =>
            {
                w.Log((RelationalEventId.CommandExecuted, LogLevel.Debug));

                // w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
            });

            // Not suitable for production
            builder.LogTo(Console.Write, new[] { DbLoggerCategory.Database.Command.Name });
            builder.EnableSensitiveDataLogging();
            builder.EnableDetailedErrors();
        }

        builder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("SchemaMigration");
            var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
            options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            options.CommandTimeout(minutes);

            // When a retrying execution strategy is in place, EF will itself buffer the resultset internally, regardless of how you evaluate your query (streaming or buffering)
            // options.EnableRetryOnFailure();
        });
    }
}