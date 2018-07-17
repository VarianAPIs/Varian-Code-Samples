using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using VMS.SF.Gateway.Contracts;
using VMS.SF.Infrastructure.Contracts.Security;
using VMS.SF.Infrastructure.Contracts.Settings;

namespace Common
{
    public class SharedFrameworkReader
    {
        public string GatewayTokenUri { get; private set; }

        public SharedFrameworkReader(string gatewayTokenUri)
        {
            GatewayTokenUri = gatewayTokenUri;
        }

        #region Settings
        public GetSettingsResponse GetSettings(string settingName, string accessToken)
        {
            var getSettings = new GetSettingsRequest
            {
                Path = "<PathAttributes SettingName='" + settingName + "'/>"
            };
            return _processRequest(getSettings, accessToken) as GetSettingsResponse;

        }

        public GetSettingsResponse GetSettings(string settingName, string xPath, string userId, string accessToken)
        {
            var getSettings = new GetSettingsRequest
            {
                Path = "<PathAttributes SettingName='approles' Xpath='" + xPath + "' Userid='" + userId + "' />"
            };
            return _processRequest(getSettings, accessToken) as GetSettingsResponse;

        }

        public DataSource GetDataSource(string accessToken)
        {
            var settings = GetSettings("datasource", accessToken);
            var serializer = new XmlSerializer(typeof(DataSource));
            DataSource dataSource;

            using (TextReader reader = new StringReader(settings.Setting.Value))
            {
                dataSource = (DataSource)serializer.Deserialize(reader);
            }
            return dataSource;
        }
        #endregion

        private Response _processRequest(Request request, string token)
        {
            //Request
            var requesttype = new[] { typeof(GetSettingsRequest), typeof(GetPrivilegesRequest) };
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

            //Response
            var responsetype = new[] { typeof(GetSettingsResponse), typeof(GetPrivilegesResponse), typeof(ApplicationError), typeof(GatewayError) };
            var r = (HttpWebResponse)myWebRequest.GetResponse();
            dataContractSeriliser = new DataContractJsonSerializer(typeof(Response), responsetype);
            var result = dataContractSeriliser.ReadObject(r.GetResponseStream());
            return result as Response;
        }


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
