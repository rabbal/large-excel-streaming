namespace LargeExcelStreaming.Features.Base;

public delegate IEnumerable<T> PaginatedEnumerable<out T>(int page, int pageSize);