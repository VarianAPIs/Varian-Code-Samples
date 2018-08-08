using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Windows;
using Helpers;

namespace ARIALocalServicesSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        private static bool Initialized = false;

        public MainWindow()
        {
            InitializeComponent();
            Initialized = true;
            
            SetInitialControlState();
        }

        private void SetInitialControlState()
        {
            txtURI.Text = ConfigurationManager.AppSettings.Get("gatewayrestapi");
        }
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            txtRequest.Text =
                "{\"__type\":\"GetDocumentsRequest:http://services.varian.com/Patient/Documents\",\"Attributes\":[],\"PatientId\":{\"PtId\":\"22000000101000000022\"}}";
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            UpdateParameters();
            string validationError = "";
            IArgumentValidator argumentValidator = new ArgumentValidatorFactory().CreateArgumentValidator(STSRadioBtn.IsChecked != null && STSRadioBtn.IsChecked.Value);
            
            if (!argumentValidator.ValidateAuthenticationArgs(ref validationError))
            {
                MessageBox.Show(validationError);
                return;
            }

            txtResponse.Text = WebRequestHelper.ProcessRequest(txtRequest.Text, STSRadioBtn.IsChecked.Value);
        }

        private void UpdateParameters()
        {
            ConnectivitySettings.GatewayRestUri = txtURI.Text;
            ConnectivitySettings.ApiKey = txtAPI.Text;
            ConnectivitySettings.ServiceUser = txtUser.Text;
            ConnectivitySettings.ServicePassword = PswPassword.SecurePassword;

            IdentityProviderSettings.STSURL = txtSTSURL.Text;
            IdentityProviderSettings.ClientID = txtClientID.Text;
            IdentityProviderSettings.ClientSecret = txtClientSecret.Text;
            IdentityProviderSettings.Scopes = txtScopes.Text;
            IdentityProviderSettings.CallbackURI = ConfigurationManager.AppSettings["STSCallback"]; 
        }

        private void btnAutoFill_Click(object sender, RoutedEventArgs e)
        {
            /*** In practice, do NOT store secrets, APIKeys, or user credentials in an appconfig ***/

            txtSTSURL.Text = ConfigurationManager.AppSettings["STSBaseAddress"];
            txtClientID.Text = ConfigurationManager.AppSettings["STSClientId"];
            txtClientSecret.Text = ConfigurationManager.AppSettings["STSSecret"];
            txtScopes.Text = ConfigurationManager.AppSettings["STSScopes"];
            IdentityProviderSettings.CallbackURI = ConfigurationManager.AppSettings["STSCallback"]; 

            txtAPI.Text = ConfigurationManager.AppSettings.Get("apikey");
            txtUser.Text = ConfigurationManager.AppSettings.Get("ADUser");
            PswPassword.Password = ConfigurationManager.AppSettings.Get("ADPass");

            txtGetToken.Text = string.Empty;
        }

        private async void btnGetToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateParameters();
                string validationError = "";
                TokenValidator argumentValidator = (TokenValidator)new ArgumentValidatorFactory().CreateArgumentValidator(true);
                if (!argumentValidator.ValidateTokenArgs(ref validationError))
                {
                    MessageBox.Show(validationError);
                    return;
                }

                // TODO: SUPPORT INTERACTIVE CLIENT. SOMETHING LIKE: 
                //bool success2 = await STSHelper.GetInteractiveTokens(IdentityProviderSettings.STSURL,
                //    IdentityProviderSettings.ClientID, IdentityProviderSettings.ClientSecret,
                //    IdentityProviderSettings.Scopes, IdentityProviderSettings.CallbackURI);

                bool success = await STSHelper.GetHeadlessToken(IdentityProviderSettings.STSURL, IdentityProviderSettings.ClientID, IdentityProviderSettings.ClientSecret, IdentityProviderSettings.Scopes);
                if (success && ConnectivitySettings.AccessToken != null)
                {
                    txtGetToken.Text =
                        $" Access Token : {Environment.NewLine}{JsonHelper.FormatJson(JwtTokenHandler.ReadJwtToken(ConnectivitySettings.AccessToken).ToString())}";
                }
                else
                {
                    txtGetToken.Text = "An error occured. " + STSHelper.GetLastError();
                }
            }
            catch (Exception xe)
            {
                MessageBox.Show(xe.Message + "\r\n" + xe.StackTrace);
            }
        }

        private void STSRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (Initialized)
            {
                txtURI.Text = ConfigurationManager.AppSettings["STSWebServerUri"];
            }
        }

        private void ADRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (Initialized)
            {
                txtURI.Text = ConfigurationManager.AppSettings["gatewayrestapi"];
            }
        }
    }
}
