using System.ComponentModel.DataAnnotations;

namespace LargeExcelStreaming.Features.Customers;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public bool IsActive { get; set; }
    public CustomerType Type { get; set; }
}

public enum CustomerType
{
    [Display(Name="برنز")]
    Bronze,
    [Display(Name="نقره‌ای")]
    Silver,
    [Display(Name="طلایی")]
    Gold
}