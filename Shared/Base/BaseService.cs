using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Base.Responses;

namespace Shared.Base
{
    public class BaseService
    {
       // protected readonly ILogger<T> _logger;

        private bool _success;
        private string _code;
        private string _message;

        public BaseService(
            
            //ILogger<T> logger
            )
        {
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public void InitMessageResponse(string messageResponseKey = "Success", string? customMessage = null)
        {
            var result = MessageResponse.Responses[messageResponseKey];

            _success = result.Item1;
            _code = result.Item2;
            _message = customMessage == null ? result.Item3 : customMessage;
        }

        protected async Task<BaseResponse<TResult>> HandleActionAsync<TResult>(Func<Task<TResult>> action)
        {
            TResult result = default;
            try
            {
                InitMessageResponse();
                result = await action();
                if ((result == null || result is ICollection && (result as ICollection).Count == 0) && _code == "200")
                {
                    InitMessageResponse("NotFound");
                }
            }
            catch (Exception ex)
            {
                InitMessageResponse("ServerError",ex.Message);
            }

            return new BaseResponse<TResult>(_success, _code, _message, result);
        }

        protected async Task<BaseResponse<dynamic>> HandleActionAsync(Func<Task<dynamic>> action)
        {
            dynamic result = default;
            try
            {
                InitMessageResponse();
                result = await action();
                if ((result == null || result is ICollection && (result as ICollection).Count == 0) && _code == "200")
                {
                    InitMessageResponse("NotFound");
                }
            }
            catch (Exception ex)
            {
                InitMessageResponse("ServerError");
            }

            return new BaseResponse<dynamic>(_success, _code, _message, result);
        }

        protected async Task<BaseResponse<Task>> HandleVoidActionAsync(Func<Task> action)
        {
            try
            {
                InitMessageResponse();
                await action();
            }
            catch (Exception ex)
            {
                InitMessageResponse("ServerError");
            }

            return new BaseResponse<Task>(_success, _code, _message, default);
        }

        protected async Task<PagedBaseResponse<List<TResult>>> HandlePaginatedActionAsync<TEntity, TResult>(
            Func<Task<PaginatedResult<TEntity>>> action,
            Func<TEntity, TResult>? mapper = null)
        {
            try
            {
                InitMessageResponse();
                var paginatedResult = await action();

                if (paginatedResult?.Items == null || !paginatedResult.Items.Any())
                {
                    InitMessageResponse("NotFound");
                    return CreateEmptyPagedResponse<TResult>();
                }

                List<TResult> mappedItems;
                if (mapper != null)
                {
                    mappedItems = paginatedResult.Items.Select(mapper).ToList();
                }
                else
                {
                    mappedItems = paginatedResult.Items.Cast<TResult>().ToList();
                }

                int totalPages = paginatedResult.PageSize > 0
                    ? (int)Math.Ceiling((double)paginatedResult.TotalItems / paginatedResult.PageSize)
                    : 1;

                return new PagedBaseResponse<List<TResult>>(
                    _success,
                    _code,
                    _message,
                    mappedItems ?? new List<TResult>(),
                    paginatedResult.PageNumber,
                    paginatedResult.PageSize,
                    paginatedResult.TotalItems,
                    totalPages
                );
            }
            catch (Exception ex)
            {
                InitMessageResponse("ServerError");
                return CreateEmptyPagedResponse<TResult>();
            }
        }

        protected async Task<PagedBaseResponse<List<dynamic>>> HandlePaginatedActionAsync<TEntity>(
            Func<Task<PaginatedResult<TEntity>>> action,
            Func<TEntity, dynamic>? mapper = null)
        {
            try
            {
                InitMessageResponse();
                var paginatedResult = await action();

                if (paginatedResult?.Items == null || !paginatedResult.Items.Any())
                {
                    InitMessageResponse("NotFound");
                    return CreateEmptyDynamicPagedResponse();
                }

                List<dynamic> mappedItems = mapper != null
                    ? paginatedResult.Items.Select(mapper).ToList<dynamic>()
                    : paginatedResult.Items.Cast<dynamic>().ToList();

                int totalPages = paginatedResult.PageSize > 0
                    ? (int)Math.Ceiling((double)paginatedResult.TotalItems / paginatedResult.PageSize)
                    : 1;

                return new PagedBaseResponse<List<dynamic>>(
                    _success,
                    _code,
                    _message,
                    mappedItems ?? new List<dynamic>(),
                    paginatedResult.PageNumber,
                    paginatedResult.PageSize,
                    paginatedResult.TotalItems,
                    totalPages
                );
            }
            catch (Exception ex)
            {
                InitMessageResponse("ServerError");
                return CreateEmptyDynamicPagedResponse();
            }
        }

        private PagedBaseResponse<List<TResult>> CreateEmptyPagedResponse<TResult>()
        {
            return new PagedBaseResponse<List<TResult>>(
                _success,
                _code,
                _message,
                new List<TResult>(),
                0,
                0,
                0,
                0
            );
        }

        private PagedBaseResponse<List<dynamic>> CreateEmptyDynamicPagedResponse()
        {
            return new PagedBaseResponse<List<dynamic>>(
                _success,
                _code,
                _message,
                new List<dynamic>(),
                0,
                0,
                0,
                0
            );
        }
    }
}
