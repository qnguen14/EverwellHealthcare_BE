using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
=======
using Everwell.DAL.Repositories.Interfaces;
>>>>>>> 8a97ac1725fc1a5d085520886ad240ee4e33dac9
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
<<<<<<< HEAD
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
=======
>>>>>>> 8a97ac1725fc1a5d085520886ad240ee4e33dac9
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
    }
}