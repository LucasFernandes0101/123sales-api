using _123vendas.Application.DTOs.Sales;
using _123vendas.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Sales;

[ExcludeFromCodeCoverage]
public static class SaleMappers
{
    private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<SaleMapperProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static List<SaleGetResponseDTO> ToDTO(this List<Sale> entities)
        => _mapper.Map<List<SaleGetResponseDTO>>(entities);

    public static SaleGetDetailResponseDTO ToDetailDTO(this Sale entity)
        => _mapper.Map<SaleGetDetailResponseDTO>(entity);

    public static SalePostResponseDTO ToPostResponseDTO(this Sale entity)
        => entity is not null ? _mapper.Map<SalePostResponseDTO>(entity) : new SalePostResponseDTO();

    public static SalePutResponseDTO ToPutResponseDTO(this Sale entity)
        => entity is not null ? _mapper.Map<SalePutResponseDTO>(entity) : new SalePutResponseDTO();

    public static Sale ToEntity(this SalePostRequestDTO dto)
        => dto is not null ? _mapper.Map<Sale>(dto) : new Sale();

    public static Sale ToEntity(this SalePutRequestDTO dto)
        => dto is not null ? _mapper.Map<Sale>(dto) : new Sale();

    public static SaleItemGetDetailDTO ToDetailDTO(this SaleItem entity)
        => _mapper.Map<SaleItemGetDetailDTO>(entity);
}