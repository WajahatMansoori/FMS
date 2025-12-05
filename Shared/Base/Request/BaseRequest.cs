using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Base.Request
{
    public class BaseRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Text { get; set; }
    }
}
