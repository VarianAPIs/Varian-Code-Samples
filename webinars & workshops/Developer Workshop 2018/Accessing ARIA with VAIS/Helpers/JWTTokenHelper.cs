using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class JWTTokenHelper
    {
        public static string ReadToken(string token)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
        
            return JsonHelper.FormatJson(jwtTokenHandler.ReadToken(token).ToString());
        }
    }
}
