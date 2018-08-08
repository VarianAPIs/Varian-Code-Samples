using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class ADValidator : IArgumentValidator
    {
        public bool ValidateAuthenticationArgs(ref string validationDetails)
        {
            if (string.IsNullOrWhiteSpace(ConnectivitySettings.GatewayRestUri) ||
                string.IsNullOrWhiteSpace(ConnectivitySettings.ApiKey) ||
                string.IsNullOrWhiteSpace(ConnectivitySettings.ServiceUser) ||
                string.IsNullOrWhiteSpace(ConnectivitySettings.ServicePassword.GetNonSecureString()))
            {
                validationDetails = "Please populate the Web Server URI, APIKey, AD user, and password prior to sending the request.";
                return false;
            }

            return true;
        }
    }
}
