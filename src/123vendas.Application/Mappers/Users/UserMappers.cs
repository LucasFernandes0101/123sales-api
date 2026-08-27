using _123vendas.Application.Commands.Users;
using _123vendas.Application.DTOs.Users;
using _123vendas.Application.Results.Users;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;

namespace _123vendas.Application.Mappers.Users;

public static class UserMappers
{
    public static User ToEntity(this CreateUserCommand command)
        => new()
        {
            Username = command.Username,
            Password = command.Password,
            Name = command.Name,
            Address = command.Address,
            Phone = command.Phone,
            Email = command.Email,
            Status = command.Status ?? UserStatus.Unknown,
            Role = command.Role ?? UserRole.None
        };

    public static User ToEntity(this GetUserCommand command)
        => new() { Id = command.Id };

    public static GetUserResult ToGetResult(this User entity)
        => new()
        {
            Id = entity.Id,
            Username = entity.Username,
            Name = entity.Name,
            Address = entity.Address,
            Email = entity.Email,
            Phone = entity.Phone,
            Role = entity.Role,
            Status = entity.Status
        };

    public static UserGetResponseDTO ToGetResponse(this GetUserResult entity)
        => new()
        {
            Id = entity.Id,
            Username = entity.Username,
            Name = entity.Name is not null
                ? new() { Firstname = entity.Name.Firstname, Lastname = entity.Name.Lastname }
                : null,
            Address = entity.Address is not null
                ? new()
                {
                    City = entity.Address.City,
                    Street = entity.Address.Street,
                    Number = entity.Address.Number,
                    Zipcode = entity.Address.Zipcode,
                    Geolocation = entity.Address.Geolocation is not null
                        ? new() { Lat = entity.Address.Geolocation.Lat, Long = entity.Address.Geolocation.Long }
                        : null
                }
                : null,
            Email = entity.Email,
            Phone = entity.Phone,
            Role = entity.Role,
            Status = entity.Status
        };

    public static UserPostResponseDTO ToPostResponse(this CreateUserResult entity)
        => new() { Id = entity.Id };

    public static CreateUserResult ToCreateResult(this User entity)
        => new() { Id = entity.Id };

    public static CreateUserCommand ToCommand(this UserPostRequestDTO dto)
        => new()
        {
            Username = dto.Username,
            Password = dto.Password,
            Phone = dto.Phone,
            Name = dto.Name is not null
                ? new() { Firstname = dto.Name.Firstname, Lastname = dto.Name.Lastname }
                : null,
            Address = dto.Address is not null
                ? new()
                {
                    City = dto.Address.City,
                    Street = dto.Address.Street,
                    Number = dto.Address.Number,
                    Zipcode = dto.Address.Zipcode,
                    Geolocation = dto.Address.Geolocation is not null
                        ? new() { Lat = dto.Address.Geolocation.Lat, Long = dto.Address.Geolocation.Long }
                        : null
                }
                : null,
            Email = dto.Email,
            Status = dto.Status,
            Role = dto.Role
        };
}