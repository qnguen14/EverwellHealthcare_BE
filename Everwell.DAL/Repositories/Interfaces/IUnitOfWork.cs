using Microsoft.EntityFrameworkCore;
using Everwell.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IGenericRepositoryFactory, IDisposable
    {
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
        Task ExecuteInTransactionAsync(Func<Task> operation);
        Task<int> SaveChangesAsync();
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
    }
}