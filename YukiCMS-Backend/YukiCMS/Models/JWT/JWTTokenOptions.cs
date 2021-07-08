using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.JWT
{
    public class JWTTokenOptions
    {

        public string Audience { get; set; }

        public RsaSecurityKey Key { get; set; }

        public SigningCredentials Credentials { get; set; }

        public string Issuer { get; set; }
    }
}
