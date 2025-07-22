using System.Linq.Expressions;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Interface;

namespace SEP490_Robot_FoodOrdering.Domain.Interface
{
    public interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        #region READ DATA

        ///  Default Procedures
        Task<IEnumerable<TEntity>> GetAllAsync(bool tracked = true);
        Task<IEnumerable<TEntity>> GetAllWithIncludeAsync(bool tracked = true, params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> GetByIdAsync(TKey id);
        Task<TEntity?> GetByIdWithIncludeAsync(
      Expression<Func<TEntity, bool>> predicate,
      bool tracked = true,
      params Expression<Func<TEntity, object>>[] includes
            );

        /// Retrieve with specifications
        Task<TEntity?> GetWithSpecAsync(ISpecification<TEntity> specification, bool tracked = true);
        Task<TResult?> GetWithSpecAndSelectorAsync<TResult>(ISpecification<TEntity> specification,
            Expression<Func<TEntity, TResult>> selector, bool tracked = true);
        Task<IEnumerable<TEntity>> GetAllWithSpecAsync(ISpecification<TEntity> specification, bool tracked = true);
        Task<IEnumerable<TEntity>> GetAllWithSpecWithInclueAsync(ISpecification<TEntity> specification, bool tracked = true, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TResult>> GetAllWithSpecAndSelectorAsync<TResult>(ISpecification<TEntity> specification,
            Expression<Func<TEntity, TResult>> selector, bool tracked = true);
        Task<int> CountAsync();
        Task<int> CountAsync(ISpecification<TEntity> specification);
        Task<int> SumAsync(Expression<Func<TEntity, int>> predicate);
        Task<int> SumWithSpecAsync(ISpecification<TEntity> specification, Expression<Func<TEntity, int>> predicate);
        Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(ISpecification<TEntity> specification);

        #endregion

        #region WRITE DATA

        /// Synchronous operation
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Delete(TKey id);
        void Update(TEntity entity);

        /// Asynchronous operation
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task DeleteAsync(TKey id);
        Task DeleteRangeAsync(TKey[] ids);
        Task UpdateAsync(TEntity entity);

        #endregion

        #region OTHERS
        bool HasChanges(TEntity original, TEntity modified);
        bool HasChanges(TEntity entity);
        #endregion
    }
}
