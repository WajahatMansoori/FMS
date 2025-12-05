using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Helpers;

namespace Shared.Base
{
    public class TokenMiddleware : IMiddleware
    {
        private readonly ITokenHelper _token;

        public TokenMiddleware(ITokenHelper token)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

            if (token != null)
            {
                var claims = _token.GetClaims(token);

                var userIdClaim = claims.FirstOrDefault(c => c.Type == "nameid");
                //var emailClaim = claims.FirstOrDefault(c => c.Type == "email");
                var userRoleIdClaim = claims.FirstOrDefault(c => c.Type == "role");

                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var userRoleId = int.Parse(userRoleIdClaim.Value);

                    //var isAuthenticated = await _userService.GetByIdAsync(userId);

                    //if (isAuthenticated.Data == null)
                    //{
                    //    context.Response.StatusCode = 403;
                    //    return;
                    //}

                    //if (isAuthenticated.Data.IsBlocked)
                    //{
                    //    context.Response.StatusCode = 999;
                    //    return;
                    //}

                    context.Items["UserClaims"] = new UserClaimsDTO
                    {
                        UserID = userId,
                        UserRole = userRoleId,
                    };
                }
            }

            await next(context);
            return;
        }
    }
}
