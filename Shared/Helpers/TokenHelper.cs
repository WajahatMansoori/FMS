using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Shared.Helpers
{
    public interface ITokenHelper
    {
        string GetAccessToken(List<Claim> claim, DateTime expiry, string issuer, string audience);

        IEnumerable<Claim> GetClaims(string token);
    }

    public class TokenHelper : ITokenHelper
    {
        protected readonly IConfiguration _configuration;

        public TokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetAccessToken(List<Claim> claim, DateTime expiry, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var securitytokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claim),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(securitytokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return JsonConvert.SerializeObject(new
            {
                access_token = tokenString,
            });

        }
        public IEnumerable<Claim> GetClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null) return Enumerable.Empty<Claim>();

            return jsonToken.Claims;
        }

    }
}
