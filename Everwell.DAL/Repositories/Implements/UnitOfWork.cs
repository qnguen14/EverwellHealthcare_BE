using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Repositories.Implements
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        public TContext Context { get; }
        private Dictionary<Type, object> _repositories;

        public UnitOfWork(TContext context)
        {
            Context = context;
        }

        #region Repository Management
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            _repositories ??= new Dictionary<Type, object>();
            if (_repositories.TryGetValue(typeof(TEntity), out object repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }

            repository = new GenericRepository<TEntity>(Context);
            _repositories.Add(typeof(TEntity), repository);
            return (IGenericRepository<TEntity>)repository;
        }
        #endregion

        #region Transaction Management
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            var executionStrategy = Context.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Context.Database.BeginTransactionAsync();
                try
                {
                    var result = await operation();
                    await Context.SaveChangesAsync(); // Automatically handle validation
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            var executionStrategy = Context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Context.Database.BeginTransactionAsync();
                try
                {
                    await operation();
                    await Context.SaveChangesAsync(); // Automatically handle validation
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        #endregion

        #region Save Changes
        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Context?.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}