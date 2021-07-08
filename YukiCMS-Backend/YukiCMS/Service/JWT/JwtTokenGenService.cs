using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using YukiCMS.Models.JWT;

namespace YukiCMS.Service.JWT
{
    public class JwtTokenGenService
    {
        private readonly JWTTokenOptions _tokenOptions;
        private readonly IConfiguration _configuration;
        public JwtTokenGenService(IConfiguration configuration,JWTTokenOptions tokenOptions)
        {
            _tokenOptions = tokenOptions;
            _configuration = configuration;
        }
        public string CreateToken(int uid)
        {
            DateTime expire = DateTime.Now.AddDays(7);
            var handler = new JwtSecurityTokenHandler();
            string issuer = _configuration["YukiJwtSettings:issuer"];
            string audience = _configuration["YukiJwtSettings:audience"];
            string jti = audience + uid.ToString() + expire.ToString();

            var claims = new[] { new Claim("jti", jti, ClaimValueTypes.String) };
            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(uid.ToString(), "TokenAuth"), claims);
            var token = handler.CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Issuer = issuer, 
                Audience = audience, 
                SigningCredentials = _tokenOptions.Credentials,
                Subject = identity,
                Expires = expire
            });
            return token;
        }
    }
}
