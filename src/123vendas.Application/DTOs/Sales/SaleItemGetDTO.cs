namespace _123vendas.Application.DTOs.Sales;

public record SaleItemGetDTO
{
    public short Sequence { get; init; }
    public int ProductId { get; init; }
    public string? ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Price { get; init; }
    public decimal? Discount { get; init; }
    public bool IsCancelled { get; init; }
}