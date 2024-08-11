using LargeExcelStreaming.Features.Exporting;
using LargeExcelStreaming.Features.Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LargeExcelStreaming.Features.Customers;

[ApiController, Route("api/customers")]
public class CustomersController(IDbContext dbContext) : ControllerBase
{
    [HttpGet("export")]
    public async Task<ActionResult> ExportCustomers([FromQuery] ExportMetadata metadata,
        CancellationToken cancellationToken)
    {
        var count = await dbContext.Set<Customer>().CountAsync(cancellationToken);
        return this.Export(
            (page, pageSize) => dbContext.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsEnumerable(), // Enable streaming instead of buffering through deferred execution
            count,
            metadata);
    }
}