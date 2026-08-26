using _123vendas.Application.Commands.Users;
using _123vendas.Application.Handlers.Users;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Tests.Mocks.Entities;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace _123vendas.Unit.Handlers.Users;

public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<DeleteUserCommand> _validator;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _validator = Substitute.For<IValidator<DeleteUserCommand>>();
        _handler = new DeleteUserHandler(_userRepository, _validator);
    }

    [Fact(DisplayName = "Handle_Should_Delete_User_Successfully")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Delete_User_Successfully()
    {
        // Arrange
        var userId = 1;
        var command = new DeleteUserCommand(1);

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        var user = new UserMock().Generate();
        user.Id = userId;

        _userRepository.GetByIdAsync(userId)
            .Returns(Task.FromResult<User?>(user));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).DeleteAsync(user);
    }

    [Fact(DisplayName = "Handle_Should_Throw_ValidationException_When_Command_Invalid")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Throw_ValidationException_When_Command_Invalid()
    {
        // Arrange
        var userId = -1;
        var command = new DeleteUserCommand(userId);

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

    [Fact(DisplayName = "Handle_Should_Throw_EntityNotFoundException_When_User_Not_Found")]
    [Trait("User", "Handler")]
    public async Task Handle_Should_Throw_EntityNotFoundException_When_User_Not_Found()
    {
        // Arrange
        var userId = 1;
        var command = new DeleteUserCommand(userId);

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        _userRepository.GetByIdAsync(userId).Returns(Task.FromResult<User?>(default));

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
    }
}