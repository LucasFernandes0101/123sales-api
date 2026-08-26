using _123vendas.Application.DTOs.BranchProducts;
using _123vendas.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.BranchProducts;

[ExcludeFromCodeCoverage]
public static class BranchProductMappers
{
    private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<BranchProductMapperProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static List<BranchProductGetResponseDTO> ToDTO(this List<BranchProduct> entities)
        => _mapper.Map<List<BranchProductGetResponseDTO>>(entities);

    public static BranchProductGetDetailResponseDTO ToDetailDTO(this BranchProduct entity)
        => _mapper.Map<BranchProductGetDetailResponseDTO>(entity);

    public static BranchProductPostResponseDTO ToPostResponseDTO(this BranchProduct entity)
        => entity is not null ? _mapper.Map<BranchProductPostResponseDTO>(entity) : new BranchProductPostResponseDTO();

    public static BranchProductPutResponseDTO ToPutResponseDTO(this BranchProduct entity)
        => entity is not null ? _mapper.Map<BranchProductPutResponseDTO>(entity) : new BranchProductPutResponseDTO();

    public static BranchProduct ToEntity(this BranchProductPostRequestDTO dto)
        => dto is not null ? _mapper.Map<BranchProduct>(dto) : new BranchProduct();

    public static BranchProduct ToEntity(this BranchProductPutRequestDTO dto)
        => dto is not null ? _mapper.Map<BranchProduct>(dto) : new BranchProduct();
}