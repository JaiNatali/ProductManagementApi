namespace Application.DTOs.Product;

public sealed class ProductDetailsResponse
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public IReadOnlyList<ProductItemDto> Items { get; set; } = Array.Empty<ProductItemDto>();
}

public sealed class ProductItemDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}
