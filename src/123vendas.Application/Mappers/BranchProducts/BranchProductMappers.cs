using _123vendas.Application.DTOs.BranchProducts;
using _123vendas.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.BranchProducts;

[ExcludeFromCodeCoverage]
public static class BranchProductMappers
{
    public static List<BranchProductGetResponseDTO> ToDTO(this List<BranchProduct> entities)
        => entities.ConvertAll(e => new BranchProductGetResponseDTO
        {
            Id = e.Id,
            CreatedAt = e.CreatedAt,
            BranchId = e.BranchId,
            ProductId = e.ProductId,
            ProductTitle = e.ProductTitle,
            ProductCategory = e.ProductCategory,
            Price = e.Price,
            IsActive = e.IsActive
        });

    public static BranchProductGetDetailResponseDTO ToDetailDTO(this BranchProduct entity)
        => new()
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            BranchId = entity.BranchId,
            ProductId = entity.ProductId,
            ProductTitle = entity.ProductTitle,
            ProductCategory = entity.ProductCategory,
            Price = entity.Price,
            StockQuantity = entity.StockQuantity,
            IsActive = entity.IsActive
        };

    public static BranchProductPostResponseDTO ToPostResponseDTO(this BranchProduct entity)
        => entity is not null
            ? new() { Id = entity.Id }
            : new();

    public static BranchProductPutResponseDTO ToPutResponseDTO(this BranchProduct entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                BranchId = entity.BranchId,
                ProductId = entity.ProductId,
                ProductTitle = entity.ProductTitle,
                ProductCategory = entity.ProductCategory,
                Price = entity.Price,
                StockQuantity = entity.StockQuantity,
                IsActive = entity.IsActive
            }
            : new();

    public static BranchProduct ToEntity(this BranchProductPostRequestDTO dto)
        => dto is not null
            ? new()
            {
                BranchId = dto.BranchId,
                ProductId = dto.ProductId,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                IsActive = dto.IsActive
            }
            : new();

    public static BranchProduct ToEntity(this BranchProductPutRequestDTO dto)
        => dto is not null
            ? new()
            {
                Id = dto.Id,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                IsActive = dto.IsActive
            }
            : new();
}