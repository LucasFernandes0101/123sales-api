using _123vendas.Application.Commands.Auth;
using _123vendas.Application.Handlers.Auth;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace _123vendas.Unit.Handlers.Auth;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<AuthenticateUserCommand> _validator;
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _validator = Substitute.For<IValidator<AuthenticateUserCommand>>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator, _validator);
    }

    [Fact(DisplayName = "Handle_Should_Return_AuthenticateUserResult_When_Valid_Credentials")]
    [Trait("Auth", "Handler")]
    public async Task Handle_Should_Return_AuthenticateUserResult_When_Valid_Credentials()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            Email = "john_doe@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Email = command.Email,
            Username = "Jhon Doe",
            Password = "hashedPassword",
            Role = UserRole.Manager
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        _userRepository.GetActiveByEmailAsync(command.Email).Returns(Task.FromResult<User?>(user));
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(true);
        _jwtTokenGenerator.GenerateTokenAsync(user).Returns(Task.FromResult("valid_token"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.Token.Should().Be("valid_token");
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact(DisplayName = "Handle_Should_Throw_ValidationException_When_Request_Invalid")]
    [Trait("Auth", "Handler")]
    public async Task Handle_Should_Throw_ValidationException_When_Request_Invalid()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required")
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult(failures)));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Handle_Should_Throw_UnauthorizedUserException_When_User_Not_Found")]
    [Trait("Auth", "Handler")]
    public async Task Handle_Should_Throw_UnauthorizedUserException_When_User_Not_Found()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        _userRepository.GetActiveByEmailAsync(command.Email).Returns(Task.FromResult<User?>(default));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedUserException>()
            .WithMessage("Email or password is invalid.");
    }

    [Fact(DisplayName = "Handle_Should_Throw_UnauthorizedUserException_When_Password_Invalid")]
    [Trait("Auth", "Handler")]
    public async Task Handle_Should_Throw_UnauthorizedUserException_When_Password_Invalid()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Email = command.Email,
            Username = "TestUser",
            Password = "hashedPassword",
            Role = UserRole.Manager
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));
        _userRepository.GetActiveByEmailAsync(command.Email).Returns(Task.FromResult<User?>(user));
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(false);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedUserException>()
            .WithMessage("Email or password is invalid.");
    }
}