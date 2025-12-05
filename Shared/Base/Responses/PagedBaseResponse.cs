using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Base.Responses
{
    public class PagedBaseResponse<T> : BaseResponse<T>
    {
        public PaginationMetadata PaginatedResponse { get; set; }

        public PagedBaseResponse(bool success, string code, string message, T data, int pageNumber, int pageSize, int totalItems, int totalPages)
            : base(success, code, message, data)
        {
            PaginatedResponse = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}
