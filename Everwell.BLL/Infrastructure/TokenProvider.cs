using Everwell.DAL.Data.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Everwell.DAL.Data.Infrastructure
{
    public class TokenProvider(IConfiguration config)
    {
        public string Create(User user)
        {
            string secretKey = config["Jwt:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(config.GetValue<int>("ExpirationTimeInMinutes")), // Token expiration time
                SigningCredentials = credentials,
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]

            };

            var tokenHandler = new JsonWebTokenHandler();

            string token = tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }
    } 
}
