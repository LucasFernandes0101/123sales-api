using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;

namespace _123vendas.Application.Events.Sales;

public class SaleItemCancelledEvent(SaleItem saleItem) : BaseEvent("Sale")
{
    public int SaleId { get; set; } = saleItem.SaleId;
    public int SaleItemId { get; set; } = saleItem.Id;
    public short Sequence { get; set; } = saleItem.Sequence;
    public DateTimeOffset CancelledAt { get; set; } = saleItem.CancelledAt ?? DateTimeOffset.Now;
}
