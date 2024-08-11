namespace LargeExcelStreaming.Features.LargeExcel;

public interface IExcelDocumentFactory
{
    ILargeExcelDocument CreateLargeDocument(IEnumerable<ExcelColumn> headers, Stream stream);
}

internal sealed class ExcelDocumentFactory : IExcelDocumentFactory
{
    public ILargeExcelDocument CreateLargeDocument(IEnumerable<ExcelColumn> columns, Stream stream)
    {
        return new MiniExcelDocument(stream, columns);
    }
}