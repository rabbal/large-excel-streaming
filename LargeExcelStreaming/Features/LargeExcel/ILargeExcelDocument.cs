using DNTPersianUtils.Core;
using LargeExcelStreaming.Features.Base;
using MiniExcelLibs;

namespace LargeExcelStreaming.Features.LargeExcel;

public interface ILargeExcelDocument : IAsyncDisposable, IDisposable
{
    Task Write<T>(
        PaginatedEnumerable<T> items,
        int count,
        int sizeLimit,
        CancellationToken cancellationToken = default) where T : notnull;
}

internal sealed class MiniExcelDocument(Stream stream, IEnumerable<ExcelColumn> columns) : ILargeExcelDocument
{
    private const int SheetLimit = 1_048_576;
    private bool _disposedValue;

    public async Task Write<T>(
        PaginatedEnumerable<T> items,
        int count,
        int sizeLimit,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        ThrowIfDisposed();
        
        // TODO: apply sizeLimit
        var properties = FastReflection.Instance.GetProperties(typeof(T))
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var sheets = new Dictionary<string, object>();
        var index = 1;
        while (count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Dictionary<string, object>> reader = items(index, SheetLimit)
                .Select(item =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return columns.ToDictionary(h => h.Title, h => ValueOf(item, h.Name, properties));
                });

            sheets.Add($"sheet_{index}", reader);
            count -= SheetLimit;
            index++;
        }

        // This part is forward-only, and we are pretty sure that streaming will happen without buffering.
        await stream.SaveAsAsync(sheets, cancellationToken: cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    ~MiniExcelDocument()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        await ValueTask.CompletedTask;
    }

    private void ThrowIfDisposed()
    {
        if (!_disposedValue) return;
        
        throw new ObjectDisposedException(nameof(MiniExcelDocument));
    }
    private static object ValueOf<T>(T record, string prop, IDictionary<string, FastPropertyInfo> properties)
        where T : notnull
    {
        var property = properties[prop] ??
                       throw new InvalidOperationException($"There is no property with given name [{prop}]");

        return NormalizeValue(property.GetValue?.Invoke(record));
    }

    private static object NormalizeValue(object? value)
    {
        if (value == null) return null!;

        return value switch
        {
            DateTime dateTime => dateTime.ToShortPersianDateTimeString(),
            TimeSpan time => time.ToString(@"hh\:mm\:ss"),
            DateOnly dateTime => dateTime.ToShortPersianDateString(false),
            TimeOnly time => time.ToString(@"hh\:mm\:ss"),
            bool boolean => boolean ? "بلی" : "خیر",
            IEnumerable<object> values => string.Join(',', values.Select(NormalizeValue).ToList()),
            Enum enumField => enumField.GetEnumStringValue(),
            _ => value
        };
    }
}