using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.SF.Gateway.Contracts;
using VMS.SF.Infrastructure.Contracts.Security;

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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += Start;
            _redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            _authority = ConfigurationManager.AppSettings["Authority"];
            _clientIdentifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            _scope = ConfigurationManager.AppSettings["Scope"];
            _gatewayTokenUri = ConfigurationManager.AppSettings["GatewayTokenUri"];
        }

        private async void Start(object sender, RoutedEventArgs e)
        {

            var browser = new SystemBrowser(_redirectUri);
            var options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = _clientIdentifier,
                //ClientSecret = "mypocforrabbitmq",
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
            Message.Text += string.Format("{0} - VAIS tokens aquired for {1}\n", DateTime.Now, result.User.Identity.Name);

        }

        private void Get_Privileges(object sender, RoutedEventArgs e)
        {
            var getprivileges = new GetPrivilegesRequest();
            var getPrivilegeResponse = _processRequest(getprivileges, _accessToken);
            Message.Text += string.Format("{0} - GetPrivileges ok {1}{2}"
                , DateTime.Now
                , Environment.NewLine
                , _getPrivileges(getPrivilegeResponse));
        }

        private GetPrivilegesResponse _processRequest(Request request, string token)
        {
            //Request
            var requesttype = new[] { typeof(GetPrivilegesRequest) };
            var myWebRequest = HttpWebRequest.Create(_gatewayTokenUri);
            myWebRequest.Headers.Add("Authorization", "Bearer " + token);
            myWebRequest.Method = "POST";
            myWebRequest.ContentType = "Application/json";

            var ms = new MemoryStream();
            var dataContractSeriliser = new DataContractJsonSerializer(typeof(Request), requesttype);
            dataContractSeriliser.WriteObject(ms, request);
            string json = Encoding.UTF8.GetString(ms.ToArray());
            var bytearray = Encoding.UTF8.GetBytes(json);
            myWebRequest.ContentLength = bytearray.Length;
            myWebRequest.GetRequestStream().Write(bytearray, 0, bytearray.Length);

            //response
            var responsetype = new[] { typeof(GetPrivilegesResponse) };
            var r = (HttpWebResponse)myWebRequest.GetResponse();
            dataContractSeriliser = new DataContractJsonSerializer(typeof(Response), responsetype);
            var result = dataContractSeriliser.ReadObject(r.GetResponseStream());
            return result as GetPrivilegesResponse;
        }

        private string _getPrivileges(GetPrivilegesResponse response)
        {
            var result = new StringBuilder();
            result.AppendFormat("Attributes:{0}", Environment.NewLine);
            if (response.Attributes.Any())
            {
                foreach(var attribute in response.Attributes)
                {
                    result.AppendFormat("\t{0}: {1}{2}", attribute.Name, attribute.Value, Environment.NewLine);
                }
            }
            result.AppendFormat("Privileges:{0}", Environment.NewLine);
            foreach (var privilege in response.Privileges)
            {
                result.AppendFormat("\tPrivilege:{0}", Environment.NewLine);
                result.AppendFormat("\t\tApplications:");
                result.AppendFormat("{0}{1}"
                    , string.Join(", ", privilege.Applications)
                    , Environment.NewLine);

                result.AppendFormat("\t\tGroupIds:");
                result.AppendFormat("{0}{1}"
                    , string.Join(", ", privilege.GroupIds)
                    , Environment.NewLine);

                result.AppendFormat("\t\tPrivileges: category:{0}, group:{1}, id:{2}, name:{3}, version:{4}{5}"
                    ,  privilege.Privilege.Category
                    , privilege.Privilege.Group
                    , privilege.Privilege.Id
                    , privilege.Privilege.Name
                    , privilege.Privilege.Version
                    , Environment.NewLine);

                
            }
            return result.ToString();
        }

    }
}
