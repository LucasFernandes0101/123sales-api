using _123vendas.Application.DTOs.Products;
using _123vendas.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Products;

[ExcludeFromCodeCoverage]
public static class ProductMappers
{
    private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<ProductMapperProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static List<ProductGetResponseDTO> ToDTO(this List<Product> entities)
        => _mapper.Map<List<ProductGetResponseDTO>>(entities);

    public static ProductGetDetailResponseDTO ToDetailDTO(this Product entity)
        => _mapper.Map<ProductGetDetailResponseDTO>(entity);

    public static ProductPostResponseDTO ToPostResponseDTO(this Product entity)
        => entity is not null ? _mapper.Map<ProductPostResponseDTO>(entity) : new ProductPostResponseDTO();

    public static ProductPutResponseDTO ToPutResponseDTO(this Product entity)
        => entity is not null ? _mapper.Map<ProductPutResponseDTO>(entity) : new ProductPutResponseDTO();

    public static Product ToEntity(this ProductPostRequestDTO dto)
        => dto is not null ? _mapper.Map<Product>(dto) : new Product();

    public static Product ToEntity(this ProductPutRequestDTO dto)
        => dto is not null ? _mapper.Map<Product>(dto) : new Product();
}
