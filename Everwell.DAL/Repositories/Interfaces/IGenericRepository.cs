using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Repositories.Interfaces
{
    public interface IGenericRepository<T> : IDisposable where T : class
    {
        #region Get Async

        Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

        Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

        Task<ICollection<T>> GetListAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int? take = null);

        Task<ICollection<TResult>> GetListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int? take = null);

        //Task<PagingResponse<T>> GetPagingListAsync(
        //    Expression<Func<T, bool>> predicate = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //    Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        //    int page = 1,
        //    int size = 10);

        //Task<PagingResponse<TResult>> GetPagingListAsync<TResult>(
        //    Expression<Func<T, TResult>> selector,
        //    Expression<Func<T, bool>> predicate = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //    Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        //    int page = 1,
        //    int size = 10);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        Task<T> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

        Task<TResult> SingleOrDefaultAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

        #endregion

        #region Insert

        Task InsertAsync(T entity);

        Task InsertRangeAsync(IEnumerable<T> entities);

        #endregion

        #region Update

        void UpdateAsync(T entity);

        void UpdateRange(IEnumerable<T> entities);

        #endregion

        #region Insert
        void DeleteAsync(T entity);
        void DeleteRangeAsync(IEnumerable<T> entities);
        #endregion

        #region queryable
        IQueryable<T> CreateBaseQuery(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        bool asNoTracking = true);
        #endregion
    }
}