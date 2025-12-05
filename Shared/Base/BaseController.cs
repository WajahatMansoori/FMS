using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.Base.Responses;

namespace Shared.Base
{
    public abstract class BaseController : ControllerBase
    {
        protected UserClaimsDTO UserClaims
        {
            get
            {
                var claims = HttpContext.Items["UserClaims"] as UserClaimsDTO;

                // Reason of commenting: Needs a check if the authorized user is accessing or non authorized.
                // In case of authorized, I want to get the email
                // Otherwise null

                //if (claims == null) throw new UnauthorizedAccessException("User is not authorized.");

                return claims;
            }
        }

        protected IActionResult GenerateResponse<T>(BaseResponse<T> response)
        {
            HttpStatusCode statusCode;
            switch (response.Code)
            {
                case "400":
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case "404":
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case "409":
                    statusCode = HttpStatusCode.Conflict;
                    break;
                case "401":
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case "403":
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                case "500":
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
                default:
                    statusCode = HttpStatusCode.OK;
                    break;
            }

            return StatusCode((int)statusCode, response);
        }

        protected IActionResult GenerateBaseResponse(string code, string message)
        {
            return GenerateResponse(new BaseResponse<object>(false, code, message, null));
        }
    }
}
