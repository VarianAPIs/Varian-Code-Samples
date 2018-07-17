using Common;
using Extensions;
using Helpers;
using IdentityModel.OidcClient;
using System;
using System.Configuration;
using System.Windows;

namespace InteractiveClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string _accessToken;
        static string _identityToken;
        static string _refreshToken;

        private string _redirectUri;
        private string _authority;
        private string _clientIdentifier;
        private string _scope;
        private string _gatewayTokenUri;
        private SharedFrameworkReader _sharedFrameworkReader;

        public MainWindow()
        {
            InitializeComponent();

            _redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            _authority = ConfigurationManager.AppSettings["Authority"];
            _clientIdentifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            _scope = ConfigurationManager.AppSettings["Scope"];
            _gatewayTokenUri = ConfigurationManager.AppSettings["GatewayTokenUri"];

            _sharedFrameworkReader = new SharedFrameworkReader(_gatewayTokenUri);
        }

        private async void Authenticate(object sender, RoutedEventArgs e)
        {
            var browser = new SystemBrowser(_redirectUri);
            var options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = _clientIdentifier,
                RedirectUri = _redirectUri,
                Scope = "openid profile offline_access " + _scope,
                FilterClaims = false,
                Browser = browser
            };
            var oidcClient = new OidcClient(options);
            var loginRequest = new LoginRequest();
            var result = await oidcClient.LoginAsync(loginRequest);
            if (result.IsError)
            {
                _accessToken = null;
                _identityToken = null;
                _refreshToken = null;
            }
            else
            {
                _accessToken = result.AccessToken;
                _identityToken = result.IdentityToken;
                _refreshToken = result.RefreshToken;
            }

            Message.Text = string.Format("{0} - Identity token {1}\n Access token {2}\n"
                , DateTime.Now
                , JWTTokenHelper.ReadToken(_identityToken)
                , JWTTokenHelper.ReadToken(_accessToken));
        }

        private async void RefreshTokens(object sender, RoutedEventArgs e)
        {
            var options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = _clientIdentifier,
                RedirectUri = _redirectUri
            };
            var oidcClient = new OidcClient(options);
            var result = await oidcClient.RefreshTokenAsync(_refreshToken);
            if (result.IsError)
            {
                Message.Text += string.Format("{0} - Refresh Tokens error: {1}\n", DateTime.Now, result.Error);
            }
            else
            {
                _accessToken = result.AccessToken;
                _refreshToken = result.RefreshToken;
                Message.Text = string.Format("{0} - Refresh completed successfully\n", DateTime.Now);
                Message.Text += string.Format("{0} - Identity token {1}\n Access token {2}\n"
                    , DateTime.Now
                    , JWTTokenHelper.ReadToken(_identityToken)
                    , JWTTokenHelper.ReadToken(_accessToken));
            }
        }

        private void GetPrivileges(object sender, RoutedEventArgs e)
        {
            try
            {
                var privileges = _sharedFrameworkReader.GetPrivileges( _accessToken);
                Message.Text = string.Format("{0} - GetPrivileges ok {1}{2}"
                    , DateTime.Now
                    , Environment.NewLine
                    , privileges.ToFormattedString());
            }
            catch (Exception ex)
            {
                Message.Text = string.Format("{0} - GetPrivileges error {1}{2}"
                   , DateTime.Now
                   , Environment.NewLine
                   , ex.Message);
            }
        }

        private void GetSharedSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                var sharedSettings = _sharedFrameworkReader.GetSharedSettings(_accessToken);
                Message.Text = string.Format("{0} - GetSharedSetting ok {1}{2}"
                   , DateTime.Now
                   , Environment.NewLine
                   , sharedSettings.ToString());
            }
            catch (Exception ex)
            {
                Message.Text = string.Format("{0} - GetSharedSetting error {1}{2}"
                   , DateTime.Now
                   , Environment.NewLine
                   , ex.Message);
            }
        }

        private void GetDataSource(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataSource = _sharedFrameworkReader.GetDataSource(_accessToken);
                Message.Text = string.Format("{0} - GetDataSource ok {1}{2}"
                   , DateTime.Now
                   , Environment.NewLine
                   , dataSource.ToString());
            }
            catch (Exception ex)
            {
                Message.Text = string.Format("{0} - GetDataSource error {1}{2}"
                   , DateTime.Now
                   , Environment.NewLine
                   , ex.Message);
            }
        }

    }
}
