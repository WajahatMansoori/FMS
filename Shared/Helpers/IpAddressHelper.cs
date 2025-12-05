using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Shared.Helpers
{
    public static class IpAddressHelper
    {
        public static string GetClientIp(HttpContext context)
        {
            // Try common headers first (for proxies/load balancers)
            var headers = context.Request.Headers;

            if (headers.ContainsKey("X-Forwarded-For"))
            {
                var ip = headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(ip))
                    return ip.Split(',')[0]; // in case of multiple IPs
            }

            if (headers.ContainsKey("X-Real-IP"))
            {
                var ip = headers["X-Real-IP"].ToString();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            // Fallback to connection IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "217.76.53.78";
        }
    }

}
