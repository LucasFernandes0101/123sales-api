using _123vendas.Application.DTOs.Carts;
using _123vendas.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Carts;

[ExcludeFromCodeCoverage]
public static class CartMappers
{
    public static List<CartGetResponseDTO> ToDTO(this List<Cart> entities)
        => entities.ConvertAll(e => new CartGetResponseDTO
        {
            Id = e.Id,
            UserId = e.UserId,
            Date = e.Date,
            Products = e.Products?.ConvertAll(p => new CartProductGetResponseDTO
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        });

    public static CartGetDetailResponseDTO ToDetailDTO(this Cart entity)
        => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Date = entity.Date,
            Products = entity.Products?.ConvertAll(p => new CartProductGetDetailResponseDTO
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        };

    public static CartPostResponseDTO ToPostResponseDTO(this Cart entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Date = entity.Date,
                Products = entity.Products?.ConvertAll(p => new CartProductPostResponseDTO
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                })
            }
            : new();

    public static CartPutResponseDTO ToPutResponseDTO(this Cart entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Date = entity.Date,
                Products = entity.Products?.ConvertAll(p => new CartProductPutResponseDTO
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                })
            }
            : new();

    public static Cart ToEntity(this CartPostRequestDTO dto)
        => dto is not null
            ? new()
            {
                UserId = dto.UserId,
                Date = dto.Date,
                Products = dto.Products?.ConvertAll(p => new CartProduct
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                })
            }
            : new();

    public static Cart ToEntity(this CartPutRequestDTO dto)
        => dto is not null
            ? new()
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Date = dto.Date,
                Products = dto.Products?.ConvertAll(p => new CartProduct
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                })
            }
            : new();
}