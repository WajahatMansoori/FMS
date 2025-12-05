using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Base.Responses
{
    public static class MessageResponse
    {
        private static Dictionary<string, (bool, string, string)> messageResponses = new Dictionary<string, (bool, string, string)>
        {
            { "Success", (true, "200", "Success" ) },
            { "BadRequest", (false, "400", "Bad Request") },
            { "Unauthorized", (false, "401", "Unauthorized") },
            { "Duplicate", (false, "409", "Duplicate Exist" ) },
            { "Forbidden", (false, "403","Forbidden Request") },
            { "NotFound", (true, "404", "Data Not Found" ) },
            { "ServerError", (false, "500", "Internal Server Error" ) },
            { "ServiceUnavailable", (false, "503","Service Unavailable") }
        };


        //data == null
        //InitMessageResponse("
        //", "Invalid Credetials");

        public static Dictionary<string, (bool, string, string)> Responses
        {
            get { return messageResponses; }
        }
    }
}
