using _123vendas.Application.DTOs.Branches;
using _123vendas.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Branches;

[ExcludeFromCodeCoverage]
public static class BranchMappers
{
    public static List<BranchGetResponseDTO> ToDTO(this List<Branch> entities)
        => entities.ConvertAll(e => new BranchGetResponseDTO
        {
            Id = e.Id,
            CreatedAt = e.CreatedAt,
            Name = e.Name,
            Address = e.Address,
            Phone = e.Phone,
            IsActive = e.IsActive
        });

    public static BranchGetDetailResponseDTO ToDetailDTO(this Branch entity)
        => new()
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Name = entity.Name,
            Address = entity.Address,
            Phone = entity.Phone,
            IsActive = entity.IsActive
        };

    public static BranchPostResponseDTO ToPostResponseDTO(this Branch entity)
        => entity is not null
            ? new() { Id = entity.Id }
            : new();

    public static BranchPutResponseDTO ToPutResponseDTO(this Branch entity)
        => entity is not null
            ? new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Name = entity.Name,
                Address = entity.Address,
                Phone = entity.Phone,
                IsActive = entity.IsActive
            }
            : new();

    public static Branch ToEntity(this BranchPostRequestDTO dto)
        => dto is not null
            ? new()
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                IsActive = dto.IsActive
            }
            : new();

    public static Branch ToEntity(this BranchPutRequestDTO dto)
        => dto is not null
            ? new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                IsActive = dto.IsActive
            }
            : new();
}