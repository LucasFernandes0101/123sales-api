using _123vendas.Application.DTOs.Carts;
using _123vendas.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Application.Mappers.Carts;

[ExcludeFromCodeCoverage]
public static class CartMappers
{
    private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<CartMapperProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static List<CartGetResponseDTO> ToDTO(this List<Cart> entities)
    {
        return _mapper.Map<List<CartGetResponseDTO>>(entities);
    }

    public static CartGetDetailResponseDTO ToDetailDTO(this Cart entity)
    {
        return _mapper.Map<CartGetDetailResponseDTO>(entity);
    }

    public static CartPostResponseDTO ToPostResponseDTO(this Cart entity)
    {
        return entity is not null ? _mapper.Map<CartPostResponseDTO>(entity) : new CartPostResponseDTO();
    }

    public static CartPutResponseDTO ToPutResponseDTO(this Cart entity)
    {
        return entity is not null ? _mapper.Map<CartPutResponseDTO>(entity) : new CartPutResponseDTO();
    }

    public static Cart ToEntity(this CartPostRequestDTO dto)
    {
        return dto is not null ? _mapper.Map<Cart>(dto) : new Cart();
    }

    public static Cart ToEntity(this CartPutRequestDTO dto)
    {
        return dto is not null ? _mapper.Map<Cart>(dto) : new Cart();
    }
}