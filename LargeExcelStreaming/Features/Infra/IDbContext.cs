using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LargeExcelStreaming.Features.Infra;

/// <summary>
/// The intention behind this interface is not to wrap and hide EFCore at all. it's used for inverting
/// dependency between infra and application.
/// </summary>
public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IQueryable<TResult> Query<TResult>([NotParameterized] FormattableString queryString);
    void Migrate(TimeSpan timeout);
    
    void ExecuteSqlInterpolatedCommand(FormattableString query);
    void ExecuteSqlRawCommand(string query, params object[] parameters);

    Task BulkInsertAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken)
        where T : class;
}

/// <summary>
/// The intention behind this interface is not to wrap and hide EFCore at all. it's used for inverting
/// dependency between infra and application (in future after moving Base.* to respective projects).
/// </summary>
public interface IDbContextFactory
{
    IDbContext CreateDbContext(bool unwritable = false);
    Task<IDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
    Task<IDbContext> CreateDbContextAsync(bool unwritable = false, CancellationToken cancellationToken = default);
}