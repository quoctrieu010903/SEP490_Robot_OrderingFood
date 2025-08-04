﻿
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Interface;

namespace SEP490_Robot_FoodOrdering.Domain
{
    public class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class
    {
        // Default constructor
        public BaseSpecification() { }

        // Constructor with specific criteria
        public BaseSpecification(Expression<Func<TEntity, bool>> criteria) => Criteria = criteria;

        #region Query, Filtering, Order Data
        // Filtering criteria
        public Expression<Func<TEntity, bool>> Criteria { get; set; } = null!;

        // Additional filters
        public List<Expression<Func<TEntity, bool>>> Filters { get; } = new();

        // Include related tables
        public List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> Includes { get; } = new();

        // Sorting
        public Expression<Func<TEntity, object>> OrderBy { get; private set; } = null!;
        public Expression<Func<TEntity, object>> OrderByDescending { get; private set; } = null!;

        // Grouping
        public Expression<Func<TEntity, object>> GroupBy { get; private set; } = null!;

        #endregion

        #region Pagination
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public bool AsSplitQuery { get; private set; } = false;
        public bool IsDistinct { get; set; } = false;
        #endregion

        #region Add Specification Properties
        public void AddFilter(Expression<Func<TEntity, bool>> filterExpression)
        {
            Filters.Add(filterExpression);
        }

        public void ApplyInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        public void ApplyPaging(int take, int skip)
        {
            Take = take;
            Skip = skip;
            IsPagingEnabled = true;
        }

        public void AddGroupBy(Expression<Func<TEntity, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        public void AddCriteria(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria;
        }

        public void EnableSplitQuery()
        {
            AsSplitQuery = true;
        }

        public BaseSpecification<TEntity> And(BaseSpecification<TEntity> other)
        {
            if (other == null!) return this;

            var parameter = Expression.Parameter(typeof(TEntity));
            var combined = Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(
                    Expression.Invoke(Criteria, parameter),
                    Expression.Invoke(other.Criteria, parameter)),
                parameter);

            return new BaseSpecification<TEntity>(combined);
        }

        public BaseSpecification<TEntity> Or(BaseSpecification<TEntity> other)
        {
            if (other == null!) return this;

            var parameter = Expression.Parameter(typeof(TEntity));
            var combined = Expression.Lambda<Func<TEntity, bool>>(
                Expression.OrElse(
                    Expression.Invoke(Criteria, parameter),
                    Expression.Invoke(other.Criteria, parameter)),
                parameter);

            return new BaseSpecification<TEntity>(combined);
        }
        #endregion
    }
}
