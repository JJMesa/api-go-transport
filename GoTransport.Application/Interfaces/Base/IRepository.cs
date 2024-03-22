using Ardalis.Specification;

namespace GoTransport.Application.Interfaces.Base;

public interface IRepository<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{ }