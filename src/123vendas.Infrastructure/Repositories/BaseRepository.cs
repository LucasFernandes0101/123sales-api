using _123vendas.Domain;
using _123vendas.Domain.Base;
using _123vendas.Domain.Base.Interfaces;
using _123vendas.Domain.Exceptions;
using _123vendas.Infrastructure.Contexts;
using _123vendas.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace _123vendas.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public abstract class BaseRepository<T>(PostgreDbContext dbContext) : IBaseRepository<T> where T : class, IBaseEntity
{
    protected readonly PostgreDbContext _dbContext = dbContext;

    public async Task<PagedResult<T>> GetAsync(
        int page = 1,
        int maxResults = 10,
        Expression<Func<T, bool>>? criteria = default,
        string? orderByClause = default,
        CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        int count = (page - 1) * maxResults;

        IQueryable<T> query = _dbContext.Set<T>().AsQueryable();

        if (criteria is not null)
            query = query.Where(criteria);

        if (!string.IsNullOrWhiteSpace(orderByClause))
            query = query.ApplyOrdering(orderByClause);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.Skip(count).Take(maxResults).ToListAsync(cancellationToken);

        return new(totalRecords, items);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity.IsDeleted)
            throw new EntityAlreadyDeletedException("The entity is already deleted.");

        entity.IsDeleted = true;

        _dbContext.Set<T>().Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }
}