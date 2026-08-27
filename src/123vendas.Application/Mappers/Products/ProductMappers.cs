using _123vendas.Application.DTOs.Products;
using _123vendas.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Products;

[ExcludeFromCodeCoverage]
public static class ProductMappers
{
    public static List<ProductGetResponseDTO> ToDTO(this List<Product> entities)
        => entities.ConvertAll(e => new ProductGetResponseDTO
        {
            Id = e.Id,
            CreatedAt = e.CreatedAt,
            Title = e.Title,
            Description = e.Description,
            Image = e.Image,
            Category = e.Category,
            Price = e.Price,
            Rating = e.Rating is not null
                ? new() { Rate = e.Rating.Rate, Count = e.Rating.Count }
                : null,
            IsActive = e.IsActive
        });

    public static ProductGetDetailResponseDTO ToDetailDTO(this Product entity)
        => new()
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Title = entity.Title,
            Description = entity.Description,
            Image = entity.Image,
            Category = entity.Category,
            Price = entity.Price,
            Rating = entity.Rating is not null
                ? new() { Rate = entity.Rating.Rate, Count = entity.Rating.Count }
                : null,
            IsActive = entity.IsActive
        };

    public static ProductPostResponseDTO ToPostResponseDTO(this Product entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                Title = entity.Title,
                Description = entity.Description,
                Image = entity.Image,
                Category = entity.Category,
                Price = entity.Price,
                Rating = entity.Rating is not null
                    ? new() { Rate = entity.Rating.Rate, Count = entity.Rating.Count }
                    : null,
                IsActive = entity.IsActive
            }
            : new();

    public static ProductPutResponseDTO ToPutResponseDTO(this Product entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Title = entity.Title,
                Description = entity.Description,
                Image = entity.Image,
                Category = entity.Category,
                Price = entity.Price,
                Rating = entity.Rating is not null
                    ? new() { Rate = entity.Rating.Rate, Count = entity.Rating.Count }
                    : null,
                IsActive = entity.IsActive
            }
            : new();

    public static Product ToEntity(this ProductPostRequestDTO dto)
        => dto is not null
            ? new()
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                Image = dto.Image,
                Rating = new()
                {
                    Rate = dto.Rating,
                    Count = dto.RateCount
                },
                IsActive = dto.IsActive
            }
            : new();

    public static Product ToEntity(this ProductPutRequestDTO dto)
        => dto is not null
            ? new()
            {
                Id = dto.Id,
                Title = dto.Title,
                Image = dto.Image,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                Rating = new()
                {
                    Rate = dto.Rating,
                    Count = dto.RateCount
                },
                IsActive = dto.IsActive
            }
            : new();
}
