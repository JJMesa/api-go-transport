using Ardalis.Specification.EntityFrameworkCore;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Persistence.Contexts;

namespace GoTransport.Persistence.Repositories;

public class BaseRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : class
{
    public ApplicationDbContext Context { get; }

    public BaseRepository(ApplicationDbContext context) : base(context)
    {
        Context = context;
    }
}