using LargeExcelStreaming.Features.Customers;
using LargeExcelStreaming.Features.Infra;
using LargeExcelStreaming.Features.LargeExcel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services
    .AddInfra()
    .AddLargeExcel();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
    dbContext.Migrate(TimeSpan.FromMinutes(1));
    if (await dbContext.Set<Customer>().AnyAsync() == false)
    {
        var chunks = Enumerable.Range(1, 10_000_000).Select(i => new Customer
        {
            Id = i,
            FirstName = $"FirstName{i}",
            LastName = $"LastName{i}",
            BirthDate = new DateOnly(1970 + i % 50, i % 12 + 1, i % 28 + 1),
            IsActive = i % 2 == 0,
            Type = (CustomerType)(i % 3)
        }).Chunk(100_000);

        foreach (var items in chunks)
        {
            await dbContext.BulkInsertAsync(items, CancellationToken.None);
        }
    }
}

app.UseHttpsRedirection();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}