using _123vendas.Application.Commands.Users;
using _123vendas.Application.Mappers.Users;
using _123vendas.Application.Results.Users;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace _123vendas.Application.Handlers.Users;

public class CreateUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserCommand> validator) : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        await ValidateUserAsync(command, cancellationToken);

        var result = await userRepository.GetActiveByEmailAsync(command.Email!);

        if (result is not null)
            throw new UserAlreadyExistsException($"User with email {command.Email} already exists");

        var user = command.ToEntity();

        user.Password = passwordHasher.HashPassword(command.Password!);

        var createdUser = await userRepository.AddAsync(user);

        return createdUser.ToCreateResult();
    }

    private async Task ValidateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);
    }
}