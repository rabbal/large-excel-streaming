namespace LargeExcelStreaming.Features.LargeExcel;

public static class DependencyInjection
{
    public static IServiceCollection AddLargeExcel(this IServiceCollection services)
    {
        services.AddScoped<IExcelDocumentFactory, ExcelDocumentFactory>();
        return services;
    }
}