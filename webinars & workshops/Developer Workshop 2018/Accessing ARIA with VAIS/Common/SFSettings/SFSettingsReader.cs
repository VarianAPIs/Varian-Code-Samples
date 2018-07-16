using Extensions;
using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using VMS.SF.Gateway.Contracts;
using VMS.SF.Infrastructure.Contracts.Settings;

namespace Common.SFSettings
{
    public class SFSettingsReader
    {
        public string GatewayTokenUri {get; private set;}

        public SFSettingsReader(string gatewayTokenUri)
        {
            GatewayTokenUri = gatewayTokenUri;
        }

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

        public SharedSettings GetSharedSettings(string settingName, string accessToken)
        {
            var settings = GetSettings(settingName, accessToken);
            var serializer = new XmlSerializer(typeof(SharedSettings));
            SharedSettings sharedSettings;

            using (TextReader reader = new StringReader(settings.Setting.Value))
            {
                sharedSettings = (SharedSettings)serializer.Deserialize(reader);
            }
            return sharedSettings;
        }

        public SecureString GetAppRole(string xPath, string accessToken)
        {
            var workstationId = RegistryHelper.GetWorkstationID();
            var getSettings = new GetSettingsRequest
            {
                Path = "<PathAttributes SettingName='approles' Xpath='" + xPath + "' Userid='" + workstationId + "' />"
            };
            var response = _processRequest(getSettings, accessToken) as GetSettingsResponse;
            var encryptedAppRole = response.Setting.Value;
            var appRole = DecryptAppRolePassword(encryptedAppRole);
            return appRole.GetSecureString();
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
            return result as VMS.SF.Gateway.Contracts.Response;
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
