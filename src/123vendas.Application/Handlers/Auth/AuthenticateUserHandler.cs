using _123vendas.Application.Commands.Auth;
using _123vendas.Application.Results.Auth;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace _123vendas.Application.Handlers.Auth;

public class AuthenticateUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<AuthenticateUserCommand> validator) : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var user = await userRepository.GetActiveByEmailAsync(request.Email!, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password!, user.Password!))
            throw new UnauthorizedUserException("Email or password is invalid.");

        var token = await jwtTokenGenerator.GenerateTokenAsync(user, cancellationToken);

        return new()
        {
            Id = user.Id,
            Token = token,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    private async Task ValidateRequestAsync(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);
    }
}