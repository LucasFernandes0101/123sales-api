using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Services;
using FluentValidation;
using System.Linq.Expressions;

namespace _123vendas.Application.Services;

public class BranchService(
    IBranchRepository repository,
    IValidator<Branch> validator) : IBranchService
{
    public async Task<Branch> CreateAsync(Branch request, CancellationToken cancellationToken = default)
    {
        try
        {
            await ValidateBranchAsync(request, cancellationToken);

            return await repository.AddAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new ServiceException("An error occurred while creating a branch.", ex);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await FindBranchOrThrowAsync(id, cancellationToken);

            await repository.DeleteAsync(branch, cancellationToken);
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while deleting the branch.", ex);
        }
    }

    public async Task<PagedResult<Branch>> GetAllAsync(
        int? id = default,
        bool? isActive = default,
        string? name = default,
        DateTimeOffset? startDate = default,
        DateTimeOffset? endDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page <= 0 || maxResults <= 0)
                throw new InvalidPaginationParametersException("Page number and max results must be greater than zero.");

            var criteria = BuildCriteria(id, isActive, name, startDate, endDate);

            var result = await repository.GetAsync(page, maxResults, criteria, orderByClause, cancellationToken);

            return result;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving branches.", ex);
        }
    }

    public async Task<Branch?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await repository.GetByIdAsync(id, cancellationToken);

            return branch;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the branch.", ex);
        }
    }

    public async Task<Branch> UpdateAsync(int id, Branch request, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await UpdateBranchAsync(id, request, cancellationToken);

            await ValidateBranchAsync(branch, cancellationToken);

            return await repository.UpdateAsync(branch, cancellationToken);
        }
        catch (Exception ex) when (ex is ValidationException || ex is BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while updating the branch.", ex);
        }
    }

    private async Task<Branch> UpdateBranchAsync(int id, Branch request, CancellationToken cancellationToken)
    {
        var existingBranch = await FindBranchOrThrowAsync(id, cancellationToken);

        existingBranch.Name = request.Name;
        existingBranch.Address = request.Address;
        existingBranch.Phone = request.Phone;
        existingBranch.IsActive = request.IsActive;

        return existingBranch;
    }

    private static Expression<Func<Branch, bool>> BuildCriteria(
        int? id,
        bool? isActive,
        string? name,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!isActive.HasValue || b.IsActive == isActive.Value) &&
            (string.IsNullOrEmpty(name) || b.Name!.Contains(name)) &&
            (!startDate.HasValue || b.CreatedAt >= startDate.Value) &&
            (!endDate.HasValue || b.CreatedAt <= endDate.Value);

    private async Task<Branch> FindBranchOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Branch with ID {id} not found.");

    private async Task ValidateBranchAsync(Branch branch, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(branch, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }
}