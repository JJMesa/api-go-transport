using GoTransport.Domain.Interfaces;
using GoTransport.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// The production model with the SQL Server details SQLite cannot create: the database collation,
/// varchar(max) columns and identity keys typed int/bigint (SQLite auto-increments only INTEGER).
/// Everything else (tables, keys, indexes, queries, transactions) is the real model.
/// </summary>
internal sealed class SqliteApplicationDbContext : ApplicationDbContext
{
    private SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options, new NoSignedInUser(), TimeProvider.System)
    {
    }

    /// <summary>
    /// Creates the schema on an open in-memory SQLite connection and loads the component-test seed.
    /// The connection owns the database, so it must stay open for as long as the context is used.
    /// </summary>
    public static SqliteApplicationDbContext CreateSeeded(SqliteConnection connection,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptions = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection, sqliteOptions)
            .Options;

        var context = new SqliteApplicationDbContext(options);
        context.Database.EnsureCreated();
        DatabaseSeeder.Seed(context);
        context.ChangeTracker.Clear();
        return context;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Model.RemoveAnnotation("Relational:Collation");
        foreach (var property in builder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.GetColumnType()?.Contains("(max)") == true)
                property.SetColumnType("TEXT");
        }

        foreach (var key in builder.Model.GetEntityTypes().Select(entity => entity.FindPrimaryKey()).OfType<IMutableKey>())
        {
            if (key.Properties is [{ ClrType: var type } single] && (type == typeof(int) || type == typeof(long)))
                single.SetColumnType("INTEGER");
        }
    }

    private sealed class NoSignedInUser : ICurrentUser
    {
        public string? UserId => null;
    }
}
