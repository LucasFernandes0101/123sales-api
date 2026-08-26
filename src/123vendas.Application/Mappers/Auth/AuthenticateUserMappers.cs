using _123vendas.Application.Commands.Auth;
using _123vendas.Application.DTOs.Auth;
using _123vendas.Application.Results.Auth;
using _123vendas.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace _123vendas.Application.Mappers.Auth;

public static class AuthenticateUserMappers
{
    private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<AuthenticateUserProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static AuthenticateUserCommand ToCommand(this AuthenticateUserRequestDTO dto)
        => _mapper.Map<AuthenticateUserCommand>(dto);

    public static AuthenticateUserResult ToResult(this User dto)
        => _mapper.Map<AuthenticateUserResult>(dto);

    public static AuthenticateUserResponseDTO ToResponseDTO(this AuthenticateUserResult result)
        => _mapper.Map<AuthenticateUserResponseDTO>(result);
}