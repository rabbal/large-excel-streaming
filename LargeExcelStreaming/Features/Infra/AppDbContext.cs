using System.Reflection;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Polly;
using static System.Console;

namespace LargeExcelStreaming.Features.Infra;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public IQueryable<TResult> Query<TResult>(FormattableString queryString)
    {
        return Database.SqlQuery<TResult>(queryString);
    }

    public void Migrate(TimeSpan timeout)
    {
        Database.SetCommandTimeout(timeout);
        var retry = Policy.Handle<Exception>()
            .WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(value: 5), TimeSpan.FromSeconds(value: 10), TimeSpan.FromSeconds(value: 15)
            });

        retry.Execute(() =>
        {
            WriteLine(value: "Started MigrateDb");
            Database.Migrate();
            WriteLine(value: "Finished MigrateDb");
        });
    }

    public void ExecuteSqlInterpolatedCommand(FormattableString query)
    {
        Database.ExecuteSqlInterpolated(query);
    }

    public void ExecuteSqlRawCommand(string query, params object[] parameters)
    {
        Database.ExecuteSqlRaw(query, parameters);
    }

    public Task BulkInsertAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken)
        where T : class
    {
        return this.BulkInsertAsync(entities, new BulkConfig(), cancellationToken: cancellationToken);
    }
}