using Ardalis.Specification;

namespace GoTransport.Domain.Interfaces;

/// <summary>
/// The generic repository: queries go through Ardalis specifications. Declared here, implemented in
/// Persistence, so Persistence needs nothing from Application.
/// </summary>
public interface IRepository<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{ }
