using _123vendas.Application.Commands.Users;
using _123vendas.Application.Handlers.Users;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Unit.Mocks.Entities;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace _123vendas.Unit.Handlers.Users;

public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<GetUserCommand> _validator;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _validator = Substitute.For<IValidator<GetUserCommand>>();
        _handler = new GetUserHandler(_userRepository, _validator);
    }

    [Fact(DisplayName = "Handle_Should_Return_GetUserResult_When_User_Exists")]
    [Trait("User", "Handler")]
    public async Task Handle_ShouldReturnGetUserResult_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var command = new GetUserCommand(userId);

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        var user = new UserMock().Generate();
        user.Id = userId;

        _userRepository.GetByIdAsync(userId).Returns(Task.FromResult<User?>(user));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe(user.Email);
        result.Username.ShouldBe(user.Username);
    }

    [Fact(DisplayName = "Handle_Should_Throw_ValidationException_When_Command_Invalid")]
    [Trait("User", "Handler")]
    public async Task Handle_ShouldThrowValidationException_WhenCommandInvalid()
    {
        // Arrange
        var userId = -1;
        var command = new GetUserCommand(userId);

        var failures = new List<ValidationFailure>
        {
            new("Id", "Id must be greater than 0")
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

    [Fact(DisplayName = "Handle_Should_Not_Throw_When_User_Not_Found")]
    [Trait("User", "Handler")]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var userId = 1;
        var command = new GetUserCommand(userId);

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        _userRepository.GetByIdAsync(command.Id).Returns(Task.FromResult<User?>(default));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}