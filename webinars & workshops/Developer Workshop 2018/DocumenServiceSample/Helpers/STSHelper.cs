using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Browser;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

namespace Helpers
{
    public class STSHelper
    {
        private static string _interactiveAccessToken;
        private static string _interactiveIdentityToken;
        private static string _interactiveRefreshToken;

        private static string LastError { get; set; }

        public static async Task<bool> GetHeadlessToken(string STSURL, string clientID, string clientSecret, string scopes)
        {
            ConnectivitySettings.AccessToken = null;
            try
            {
                var discoveryClient = new DiscoveryClient(STSURL);
                var doc = await discoveryClient.GetAsync();

                if (string.IsNullOrEmpty(doc.TokenEndpoint))
                {
                    LastError = "Faled to retreive the token end point.";
                    return false;
                }

                TokenClient stsTokenClient = new TokenClient(doc.TokenEndpoint);

                var credentials = new
                {
                    client_id = clientID.Trim(),
                    client_secret = clientSecret.Trim()
                };
                var tokenResponse = await stsTokenClient.RequestClientCredentialsAsync(scopes, credentials);

                if (tokenResponse.IsError)
                {
                    MessageBox.Show($"{tokenResponse.Error} : {tokenResponse.ErrorDescription}");
                    return false;
                }

                ConnectivitySettings.AccessToken = tokenResponse.AccessToken;
                return true;

            }
            catch (Exception ex)
            {
                // An unexpected error occurred.
                LastError = ex.Message;
                if (ex.InnerException != null)
                {
                    LastError += "Inner Exception : " + ex.InnerException.Message;
                }

                return false;
            }
        }

        public static async Task<bool> GetInteractiveTokens(string STSURL, string clientID, string clientSecret, string scopes, string callbackURI)
        {
            try
            {
                IBrowser browser = new SystemBrowser();

                var options = new OidcClientOptions
                {
                    Authority = STSURL,
                    ClientId = clientID,
                    ClientSecret = clientSecret,
                    RedirectUri = callbackURI,
                    Scope = scopes,
                    FilterClaims = false,
                    Browser = browser
                };

                var oidcClient = new OidcClient(options);
                var loginRequest = new LoginRequest();
                var result = await oidcClient.LoginAsync(loginRequest);
                if (result.IsError)
                {
                    LastError = "login failed: " + result.Error;
                    return false;
                }
                // else authenticated result.User.Identity.Name;

                _interactiveAccessToken = result.AccessToken;
                _interactiveIdentityToken = result.IdentityToken;
                _interactiveRefreshToken = result.RefreshToken;

                return true;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred.
                LastError = ex.Message;
                if (ex.InnerException != null)
                {
                    LastError += "Inner Exception : " + ex.InnerException.Message;
                }

                return false;
            }
        }

        public static string GetLastError()
        {
            return LastError;
        }
    }
}
