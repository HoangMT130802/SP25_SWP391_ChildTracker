using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Synchronous methods
        IQueryable<T> GetAllQueryable(string includeProperties = "");     
        void Update(T entity);
        void Delete(T entity);  
        // Asynchronous methods
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
        Task<IEnumerable<T>> GetAllAsync(string includeProperties = "");
        Task<T> GetByIdAsync(int id);   
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    }
}
