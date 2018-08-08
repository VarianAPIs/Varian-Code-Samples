using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class TokenValidator : IArgumentValidator
    {
        public bool ValidateAuthenticationArgs(ref string validationDetails)
        {
            if (string.IsNullOrWhiteSpace(ConnectivitySettings.AccessToken))
            {
                validationDetails = "Please get an access token before invoking API.";
                return false;
            }

            return true;
        }

        public bool ValidateTokenArgs(ref string validationDetails)
        {
            if (string.IsNullOrWhiteSpace(IdentityProviderSettings.STSURL) ||
                string.IsNullOrWhiteSpace(IdentityProviderSettings.ClientID) ||
                string.IsNullOrWhiteSpace(IdentityProviderSettings.ClientSecret) || 
                string.IsNullOrWhiteSpace(IdentityProviderSettings.Scopes))
            {
                validationDetails = "Please populate the Identity Server URL, client ID, client secret, and scopes prior to requesting an access token.";
                return false;
            }

            return true;
        }
    }
}
