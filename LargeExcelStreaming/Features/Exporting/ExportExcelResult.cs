using System.Net.Mime;
using DNTPersianUtils.Core;
using LargeExcelStreaming.Features.Base;
using LargeExcelStreaming.Features.LargeExcel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LargeExcelStreaming.Features.Exporting;

public static class ExportResultExtensions
{
    public static ActionResult Export<T>(
        this ControllerBase controller,
        PaginatedEnumerable<T> items,
        int count,
        ExportMetadata metadata)
        where T : notnull
    {
        return new ExcelExportResult<T>(items, count, metadata);
    }
}

public class ExcelExportResult<T>(PaginatedEnumerable<T> items, int count, ExportMetadata metadata) : ActionResult
    where T : notnull
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string Extension = ".xlsx";
    private const int SizeLimit = int.MaxValue;

    private readonly IReadOnlyList<FastPropertyInfo> _properties = FastReflection.Instance.GetProperties(typeof(T));

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var sp = context.HttpContext.RequestServices;
        var factory = sp.GetRequiredService<IExcelDocumentFactory>();

        var disposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment);
        disposition.SetHttpFileName(MakeFilename());

        context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = disposition.ToString();
        context.HttpContext.Response.Headers.Append(HeaderNames.ContentType, ContentType);
        context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

        //TODO: deal with exception, because our global exception handling cannot take into account while the response is started.

        await using var bodyStream = context.HttpContext.Response.BodyWriter.AsStream();
        await context.HttpContext.Response.StartAsync(context.HttpContext.RequestAborted);
        await using (var document = factory.CreateLargeDocument(MakeColumns(), bodyStream))
        {
            await document.Write(items, count, SizeLimit, context.HttpContext.RequestAborted);
        }

        await context.HttpContext.Response.CompleteAsync();
    }

    private string MakeFilename()
    {
        return
            $"{metadata.Title} - {DateTime.UtcNow.ToEpochSeconds()}{Extension}";
    }

    private IEnumerable<ExcelColumn> MakeColumns()
    {
        var types = _properties.ToDictionary(p => p.Name, p => p.PropertyType, StringComparer.OrdinalIgnoreCase);
        return metadata.Fields.Select(f =>
        {
            var type = types[f.Name];

            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum ||
                type == typeof(DateOnly) ||
                type == typeof(TimeOnly) ||
                type == typeof(bool) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTime))
            {
                type = typeof(string);
            }

            return new ExcelColumn(f.Name, f.Title, type);
        });
    }
}