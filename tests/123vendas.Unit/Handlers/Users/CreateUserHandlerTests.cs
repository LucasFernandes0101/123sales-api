using _123vendas.Application.Commands.Users;
using _123vendas.Application.Handlers.Users;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Unit.Mocks.Entities;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace _123vendas.Unit.Handlers.Users;

public class CreateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _validator = Substitute.For<IValidator<CreateUserCommand>>();
        _handler = new CreateUserHandler(_userRepository, _passwordHasher, _validator);
    }

    [Fact(DisplayName = "Handle_Should_Create_User_Successfully")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Create_User_Successfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "plainPassword",
            Username = "TestUser"
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        _userRepository.GetActiveByEmailAsync(command.Email)
            .Returns(Task.FromResult<User?>(null));

        var mockedUser = new UserMock().Generate();
        _userRepository.AddAsync(Arg.Any<User>())
            .Returns(Task.FromResult(mockedUser));

        var hashedPassword = "hashedPassword";
        _passwordHasher.HashPassword(command.Password)
            .Returns(hashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(mockedUser.Id);
    }

    [Fact(DisplayName = "Handle_Should_Throw_UserAlreadyExistsException_When_User_Already_Exists")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Throw_UserAlreadyExistsException_When_User_Already_Exists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            Password = "plainPassword",
            Username = "ExistingUser"
        };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        var existingUser = new UserMock().Generate();
        _userRepository.GetActiveByEmailAsync(command.Email)
            .Returns(Task.FromResult<User?>(existingUser));

        // Act & Assert
        await Should.ThrowAsync<UserAlreadyExistsException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
    }

    [Fact(DisplayName = "Handle_Should_Throw_ValidationException_When_Command_Invalid")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Throw_ValidationException_When_Command_Invalid()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "invalid@example.com",
            Password = "plainPassword",
            Username = "InvalidUser"
        };

        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required")
        };

        var validationResult = new ValidationResult(failures);
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(validationResult));

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
    }
}