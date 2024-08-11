namespace LargeExcelStreaming.Features.Exporting;

public record ExportMetadata(string Title, IReadOnlyList<ExportField> Fields);
public record ExportField(string Name, string Title);