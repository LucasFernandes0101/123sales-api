using _123vendas.Application.Commands.Users;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace _123vendas.Application.Handlers.Users;

public class DeleteUserHandler(
    IUserRepository userRepository,
    IValidator<DeleteUserCommand> validator) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(request.Id)
            ?? throw new EntityNotFoundException($"User with ID {request.Id} not found");

        await userRepository.DeleteAsync(user);
    }

    private async Task ValidateRequestAsync(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);
    }
}