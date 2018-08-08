using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Helpers
{
    public static class WebRequestHelper
    {
        public static string ProcessRequest(string jsonRequest, bool STSMode = false)
        {
            try
            {
                HttpWebRequest request = CreateHttpWebRequest(STSMode);

                WriteJsonToRequestStream(request, jsonRequest);

                string responseText = GetHttpWebResponse(request);

                //JObject jsonResponseObject = DeserializeJson(responseText);

                return responseText;
            }
            catch (System.Net.WebException we)
            {
                string errorMessage = we.Message;
                if (errorMessage.Contains("(401) Unauthorized"))
                    errorMessage += " Please double check the API key and AD credentials.";
                else if (errorMessage.Contains("(400) Bad Request"))
                    errorMessage += " Please double check the Request JSON.";
                else if (errorMessage.Contains("(404) Not Found"))
                    errorMessage += " Please double check the Web Server URI.";

                return String.Format("ERROR: {0}", errorMessage);
            }
            catch (Exception e)
            {
                return String.Format("ERROR: {0}", e.Message);
            }
        }

        private static HttpWebRequest CreateHttpWebRequest(bool STSMode = false)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConnectivitySettings.GatewayRestUri);

            if (STSMode)
            {
                request.Headers.Add("Authorization", "Bearer " + ConnectivitySettings.AccessToken);
            }
            else
            {
                request.Headers.Add("Authorization", GetBasicAuthenticationHeaderContent(ConnectivitySettings.ServiceUser, ConnectivitySettings.ServicePassword));
                request.Headers.Add("ApiKey", ConnectivitySettings.ApiKey);
            }
            request.Method = "POST";
            request.ContentType = "Application/json";

            return request;
        }

        private static void WriteJsonToRequestStream(HttpWebRequest request, string json)
        {
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        private static string GetHttpWebResponse(HttpWebRequest request)
        {
            string responseText;

            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(resp.GetResponseStream()))
            {
                responseText = streamReader.ReadToEnd();
            }

            return responseText;
        }
        
        private static string GetBasicAuthenticationHeaderContent(string userName, SecureString password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userName, password.GetNonSecureString())));
        }
    }
}
