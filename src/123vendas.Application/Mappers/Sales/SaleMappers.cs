using _123vendas.Application.DTOs.Sales;
using _123vendas.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Sales;

[ExcludeFromCodeCoverage]
public static class SaleMappers
{
    public static List<SaleGetResponseDTO> ToDTO(this List<Sale> entities)
        => entities.ConvertAll(e => new SaleGetResponseDTO
        {
            Id = e.Id,
            Status = e.Status,
            Date = e.Date,
            UserId = e.UserId,
            BranchId = e.BranchId,
            TotalAmount = e.TotalAmount,
            CancelledAt = e.CancelledAt
        });

    public static SaleGetDetailResponseDTO ToDetailDTO(this Sale entity)
        => new()
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Status = entity.Status,
            Date = entity.Date,
            UserId = entity.UserId,
            BranchId = entity.BranchId,
            TotalAmount = entity.TotalAmount,
            CancelledAt = entity.CancelledAt,
            Items = entity.Items?.ConvertAll(i => new SaleItemGetDTO
            {
                Sequence = i.Sequence,
                ProductId = i.ProductId,
                ProductName = i.ProductTitle,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Price = i.Price,
                Discount = i.Discount,
                IsCancelled = i.IsCancelled
            })
        };

    public static SalePostResponseDTO ToPostResponseDTO(this Sale entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Status = entity.Status,
                Date = entity.Date,
                UserId = entity.UserId,
                BranchId = entity.BranchId,
                TotalAmount = entity.TotalAmount,
                CancelledAt = entity.CancelledAt,
                Items = entity.Items?.ConvertAll(i => new SaleItemGetDTO
                {
                    Sequence = i.Sequence,
                    ProductId = i.ProductId,
                    ProductName = i.ProductTitle,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Price = i.Price,
                    Discount = i.Discount,
                    IsCancelled = i.IsCancelled
                })
            }
            : new();

    public static SalePutResponseDTO ToPutResponseDTO(this Sale entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                Sequence = entity.Items?.FirstOrDefault()?.Sequence ?? 0,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Status = entity.Status,
                Date = entity.Date,
                UserId = entity.UserId,
                BranchId = entity.BranchId,
                TotalAmount = entity.TotalAmount,
                CancelledAt = entity.CancelledAt
            }
            : new();

    public static Sale ToEntity(this SalePostRequestDTO dto)
        => dto is not null
            ? new()
            {
                UserId = dto.UserId,
                BranchId = dto.BranchId,
                Items = dto.Items?.ConvertAll(i => new SaleItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Discount = i.Discount
                })
            }
            : new();

    public static Sale ToEntity(this SalePutRequestDTO dto)
        => dto is not null
            ? new()
            {
                Id = dto.Id,
                Status = dto.Status,
                Date = dto.Date,
                UserId = dto.UserId,
                BranchId = dto.BranchId,
                TotalAmount = dto.TotalAmount,
                CancelledAt = dto.CancelledAt,
                Items = dto.Items?.ConvertAll(i => new SaleItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Discount = i.Discount
                })
            }
            : new();

    public static SaleItemGetDetailDTO ToDetailDTO(this SaleItem entity)
        => new()
        {
            Id = entity.Id,
            Sequence = entity.Sequence,
            SaleId = entity.SaleId,
            ProductId = entity.ProductId,
            ProductName = entity.ProductTitle,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Price = entity.Price,
            Discount = entity.Discount,
            IsCancelled = entity.IsCancelled,
            CancelledAt = entity.CancelledAt
        };
}