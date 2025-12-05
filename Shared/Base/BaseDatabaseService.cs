using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Base.Responses;

namespace Shared.Base
{
    public class BaseDatabaseService<TContext> : BaseService where TContext : DbContext
    {
        protected TContext _context;

        public BaseDatabaseService(TContext context)
        {
            _context = context;
        }

        protected async Task<PaginatedResult<TEntity>> GetPaginatedResultAsync<TEntity>(
            IQueryable<TEntity> query,
            int pageNumber,
            int pageSize, string? searchText = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                query = ApplySearchFilter(query, searchText);
            }
            //if (typeof(TEntity).GetProperty("IsActive") != null)
            //{
            //    query = query.AsNoTracking().Where(x => EF.Property<bool>(x, "IsActive"));
            //}

            var totalItems = await query.CountAsync();

            if (pageNumber != 0 && pageSize != 0)
            {
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var items = await query.ToListAsync();

            return new PaginatedResult<TEntity>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        private IQueryable<TEntity> ApplySearchFilter<TEntity>(IQueryable<TEntity> query, string searchText) where TEntity : class
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression combinedExpression = null;

            var properties = typeof(TEntity).GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var propertyExpression = Expression.Property(parameter, property.Name);
                    var textExpression = Expression.Constant(searchText.ToLower());
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                    var lowerPropertyExpression = Expression.Call(propertyExpression, toLowerMethod);
                    var containsExpression = Expression.Call(lowerPropertyExpression, containsMethod, textExpression);

                    if (combinedExpression == null)
                    {
                        combinedExpression = containsExpression;
                    }
                    else
                    {
                        combinedExpression = Expression.OrElse(combinedExpression, containsExpression);
                    }
                }
                else if (property.PropertyType == typeof(int) && int.TryParse(searchText, out int parsedText))
                {
                    var propertyExpression = Expression.Property(parameter, property.Name);
                    var textExpression = Expression.Constant(parsedText);
                    var equalsExpression = Expression.Equal(propertyExpression, textExpression);

                    if (combinedExpression == null)
                    {
                        combinedExpression = equalsExpression;
                    }
                    else
                    {
                        combinedExpression = Expression.OrElse(combinedExpression, equalsExpression);
                    }
                }
            }

            foreach (var property in properties.Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string)))
            {
                var propertyType = property.PropertyType;
                var nestedProperties = propertyType.GetProperties();

                foreach (var nestedProperty in nestedProperties)
                {
                    if (nestedProperty.PropertyType == typeof(string))
                    {
                        var navigationExpression = Expression.Property(parameter, property.Name);
                        var nestedPropertyExpression = Expression.Property(navigationExpression, nestedProperty.Name);
                        var textExpression = Expression.Constant(searchText.ToLower());
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                        var lowerNestedPropertyExpression = Expression.Call(nestedPropertyExpression, toLowerMethod);
                        var containsExpression = Expression.Call(lowerNestedPropertyExpression, containsMethod, textExpression);

                        if (combinedExpression == null)
                        {
                            combinedExpression = containsExpression;
                        }
                        else
                        {
                            combinedExpression = Expression.OrElse(combinedExpression, containsExpression);
                        }
                    }
                }
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }
    }
}
