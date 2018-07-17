using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using Extensions;
using VMS.SF.Infrastructure.Contracts.Settings;
using VMS.SF.Gateway.Contracts;
using System.Runtime.Serialization.Json;

namespace Helpers
{
    public class AppRoleHelper
    {
        private const string _RequestType = "GetSettingsRequest:http://services.varian.com/Foundation/SF/Infrastructure";

        public bool IsAppRolePasswordRetreived { get; set; }
        public string GatewayTokenUri { get; set; }

        public AppRoleHelper(string gatewayTokenUri)
        {
            IsAppRolePasswordRetreived = false;

            GatewayTokenUri = gatewayTokenUri;
        }

        public SecureString GetAppRolePassword(string appRole, string accessToken)
        {
            return DecryptAppRolePassword(GetAppRolePasswordEncrypted(appRole, accessToken)).GetSecureString();
        }

        #region Get Application Role Password Using Newtonsoft Json libraries
        public string GetAppRolePasswordEncrypted(string appRole, string accessToken)
        {
            var workstationId = RegistryHelper.GetWorkstationID();
            var getSettings = new GetSettingsRequest
            {
                Path = "<PathAttributes SettingName='approles' Xpath='" + appRole + "' Userid='" + workstationId + "' />"
            };
            var response = _processRequest(getSettings, accessToken) as GetSettingsResponse;
            return response?.Setting.Value;
        }

        private Response _processRequest(GetSettingsRequest request, string token)
        {
            //Request
            var requesttype = new[] { typeof(GetSettingsRequest) };
            var myWebRequest = WebRequest.Create(GatewayTokenUri);
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
            return result as Response;
        }

        #endregion Get Application Role Password Using Newtonsoft Json libraries

        private string DecryptAppRolePassword(string appRolePassword)
        {
            var cipherTextBytes = Convert.FromBase64String(appRolePassword);
            var cp = new CspParameters() { KeyContainerName = "VarianClient", Flags = CspProviderFlags.UseMachineKeyStore };

            using (var rsa = new RSACryptoServiceProvider(cp))
            {
                return Encoding.UTF8.GetString(rsa.Decrypt(cipherTextBytes, true));
            }
        }
    }
}
