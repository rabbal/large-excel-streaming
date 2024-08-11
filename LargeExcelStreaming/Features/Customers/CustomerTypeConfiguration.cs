using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LargeExcelStreaming.Features.Customers;

public class CustomerTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(nameof(Customer));

        builder.Property(c => c.FirstName).HasMaxLength(64);
        builder.Property(c => c.LastName).HasMaxLength(64);
    }
}