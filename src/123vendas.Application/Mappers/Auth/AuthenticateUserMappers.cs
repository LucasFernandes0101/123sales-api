using _123vendas.Application.Commands.Auth;
using _123vendas.Application.DTOs.Auth;
using _123vendas.Application.Results.Auth;
using _123vendas.Domain.Entities;

namespace _123vendas.Application.Mappers.Auth;

public static class AuthenticateUserMappers
{
    public static AuthenticateUserCommand ToCommand(this AuthenticateUserRequestDTO dto)
        => new()
        {
            Email = dto.Email,
            Password = dto.Password
        };

    public static AuthenticateUserResult ToResult(this User dto)
        => new()
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            Role = dto.Role.ToString()
        };

    public static AuthenticateUserResponseDTO ToResponseDTO(this AuthenticateUserResult result)
        => new()
        {
            Id = result.Id,
            Token = result.Token,
            Username = result.Username,
            Email = result.Email,
            Role = result.Role
        };
}