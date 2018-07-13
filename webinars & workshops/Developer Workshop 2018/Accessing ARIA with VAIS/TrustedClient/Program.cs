using Common;
using IdentityModel.Client;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VMS.SF.Gateway.Contracts;
using VMS.SF.Infrastructure.Contracts.Settings;

namespace TrustedClient
{
    class Program
    {
        private static string _accessToken;
        private static string _refreshToken;

        private static string _authority;
        private static string _clientIdentifier;
        private static string _clientSecret;
        private static string _scope;
        private static string _gatewayTokenUri;

        static void Main(string[] args)
        {
            _authority = ConfigurationManager.AppSettings["Authority"];
            _clientIdentifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            _clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            _scope = ConfigurationManager.AppSettings["Scope"];
            _gatewayTokenUri = ConfigurationManager.AppSettings["GatewayTokenUri"];

            RequestTokenAsync().GetAwaiter().GetResult();
            _getSettings();
            Console.ReadLine();
        }

        private async static Task RequestTokenAsync()
        {
            var disco = await DiscoveryClient.GetAsync(_authority);
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            var client = new TokenClient(disco.TokenEndpoint, _clientIdentifier,_clientSecret);
            var tokens = await client.RequestClientCredentialsAsync(_scope);
            _accessToken = tokens.AccessToken;
            _refreshToken = tokens.RefreshToken;
        }

        private static void _getSettings()
        {
            /** some possible settings
             * datasource
             * environmentsettingskey
             * globalconfig
             * languages
             * radiationtherapy
             * sharedsettings
             * windowsintegartednauthenticationsetting
             * */

            var getSettings = new GetSettingsRequest
            {
                Path = "<PathAttributes SettingName='sharedsettings'/>"
            };
            var getSettingsResponse = _processRequest(getSettings, _accessToken) as GetSettingsResponse;
            var serializer = new XmlSerializer(typeof(SharedSettings));
            SharedSettings sharedSettings;

            using (TextReader reader = new StringReader(getSettingsResponse.Setting.Value))
            {
                sharedSettings = (SharedSettings)serializer.Deserialize(reader);
            }

            Console.WriteLine("SharedSettings - DoseUnits: " + sharedSettings.DoseUnits);
        }

        private static VMS.SF.Gateway.Contracts.Response _processRequest(GetSettingsRequest request, string token)
        {
            //Request
            var requesttype = new[] { typeof(GetSettingsRequest) };
            var myWebRequest = WebRequest.Create(_gatewayTokenUri);
            myWebRequest.Headers.Add("Authorization", "Bearer " + token);
            myWebRequest.Method = "POST";
            myWebRequest.ContentType = "Application/json";

            var ms = new MemoryStream();
            var dataContractSeriliser = new DataContractJsonSerializer(typeof(VMS.SF.Gateway.Contracts.Request), requesttype);
            dataContractSeriliser.WriteObject(ms, request);
            string json = Encoding.UTF8.GetString(ms.ToArray());
            var bytearray = Encoding.UTF8.GetBytes(json);
            myWebRequest.ContentLength = bytearray.Length;
            myWebRequest.GetRequestStream().Write(bytearray, 0, bytearray.Length);

            //response
            var responsetype = new[] { typeof(GetSettingsResponse), typeof(ApplicationError) };
            var r = (HttpWebResponse)myWebRequest.GetResponse();
            dataContractSeriliser = new DataContractJsonSerializer(typeof(VMS.SF.Gateway.Contracts.Response), responsetype);
            var result = dataContractSeriliser.ReadObject(r.GetResponseStream());
            return result as VMS.SF.Gateway.Contracts.Response;
        }
    }
}
