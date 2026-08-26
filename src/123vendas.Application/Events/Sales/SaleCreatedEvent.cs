using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;

namespace _123vendas.Application.Events.Sales
{
public class SaleCreatedEvent(Sale sale) : BaseEvent("Sale")
{
    public int Id { get; set; } = sale.Id;
    public DateTimeOffset Date { get; set; } = sale.Date;
}
}
