namespace Application.DTOs.Product;

public sealed class ProductSummaryResponse
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
