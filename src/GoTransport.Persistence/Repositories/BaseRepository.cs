using Ardalis.Specification.EntityFrameworkCore;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Persistence.Contexts;

namespace GoTransport.Persistence.Repositories;

/// <summary>
/// The one repository: Ardalis RepositoryBase over ApplicationDbContext. Queries go through specifications;
/// the DbContext is not exposed.
/// </summary>
internal class BaseRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : class
{
    public BaseRepository(ApplicationDbContext context) : base(context)
    {
    }
}