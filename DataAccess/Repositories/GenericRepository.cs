using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly HealthChildTrackerContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(HealthChildTrackerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> GetAllQueryable(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            return IncludeProperties(query, includeProperties);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            query = IncludeProperties(query, includeProperties);
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            query = IncludeProperties(query, includeProperties);
            return await query.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            query = IncludeProperties(query, includeProperties);
            return await query.Where(predicate).ToListAsync();
        }

        private IQueryable<T> IncludeProperties(IQueryable<T> query, string includeProperties)
        {
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return query;
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve count of entities: {ex.Message}");
            }
        }
    }
}
