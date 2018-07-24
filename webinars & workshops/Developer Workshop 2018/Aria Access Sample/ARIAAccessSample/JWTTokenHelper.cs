using NodaTime;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevWorkshop2018AriaAccess
{
    public class JWTTokenHelper
    {
        public static string ReadToken(string token)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
        
            return JsonHelper.FormatJson(jwtTokenHandler.ReadToken(token).ToString());
        }

        public static bool IsTokenExpired(string token, long expiry = 120)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }

            var expirationInstant = SystemClock.Instance.GetCurrentInstant();
            try
            {
                var securityToken = new JwtSecurityToken(token);
                
                //extract expiration time
                string tokenExpiresAt = securityToken.Claims?
                    .FirstOrDefault(x => x.Type == "exp")?.Value;
                if (!string.IsNullOrWhiteSpace(tokenExpiresAt))
                {
                    expirationInstant = Instant.FromUnixTimeSeconds(Convert.ToInt64(tokenExpiresAt));
                }
            }
            catch (FormatException)
            {
                return true;
            }
            catch (OverflowException)
            {
                return true;
            }

            var expiryInstant = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(expiry));
            return expirationInstant.Minus(expiryInstant).TotalTicks <= 0;
            
        }
    }
}
