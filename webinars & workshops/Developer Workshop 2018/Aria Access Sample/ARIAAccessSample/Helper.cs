using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using VMS.SF.Gateway.Contracts;

namespace DevWorkshop2018AriaAcess
{
    class ARIAAccessHelper
    {

        private static string GetRequestJsonString(object request)
        {
            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(object),
                new List<Type> { request.GetType(), typeof(MessageAttribute) });
            var memoryStream = new MemoryStream();
            dataContractJsonSerializer.WriteObject(memoryStream, request);
            memoryStream.Flush();

            var sJsonString = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
            return sJsonString;
        }

        public static string SendRequestData(object objRequest, String gatewayUrl, String accessToken)
        {
            var request = GetRequestJsonString(objRequest);            
            return SendRequestData(request, gatewayUrl, accessToken);

        }

        public static string SendRequestData(string request, String gatewayUrl, String accessToken)
        {
           var sResponse = String.Empty;
            using (var c = new HttpClient())
            {
                //c.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["QppApiKeyWithLicense"]);
                c.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var task = c.PostAsync(gatewayUrl, new StringContent(request, Encoding.UTF8, "application/json"));
                Task.WaitAll(task);
                var responseTask = task.Result.Content.ReadAsStringAsync();
                Task.WaitAll(responseTask);
                sResponse = responseTask.Result;
            }
            return sResponse;

        }
    }
}
