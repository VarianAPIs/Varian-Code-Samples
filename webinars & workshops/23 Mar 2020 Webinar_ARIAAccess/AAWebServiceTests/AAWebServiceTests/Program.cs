using Newtonsoft.Json;
using services.varian.com.AriaWebConnect.Link;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VMSType = services.varian.com.AriaWebConnect.Common;

namespace AAWebServiceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            string apiKey = "YourAPIKey";
            //string request = "{\"__type\":\"GetMachineListRequest:http://services.varian.com/AriaWebConnect/Link\",\"Attributes\":null,\"DepartmentID\":{\"Value\":\"Radiation Oncology\"}}";
            //string response = SendData(request, true, apiKey);
            //Console.WriteLine(response);
            //var response_machines = JsonConvert.DeserializeObject<GetMachineListResponse>(response);
            //foreach(var machine in response_machines.Machines)
            //{
            //    Console.WriteLine(machine.MachineId.Value);
            //}
            //Console.ReadLine();

            //**new request starts here **

            string departmentName = "Radiation Oncology";
            var hospitalName = "Varian Medical Center";
            string resourceType = "Machine";
            string machineId = "EDGE";
            var startdate = new DateTime(2020, 3, 23);
            //var week_start = new DateTimeOffset(startdate, TimeZoneInfo.Local.GetUtcOffset(startdate));
            var enddate = startdate.AddDays(5);
            GetMachineAppointmentsRequest getMachineAppointmentsRequest = new GetMachineAppointmentsRequest
            {
                DepartmentName = new VMSType.String { Value = departmentName },
                HospitalName = new VMSType.String { Value = hospitalName },
                ResourceType = new VMSType.String { Value = resourceType },
                MachineId = new VMSType.String { Value = machineId },
                StartDateTime = new VMSType.String { Value = startdate.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                EndDateTime = new VMSType.String { Value = enddate.ToString("yyyy-MM-ddTHH:mm:sszzz") }
            };
            string request_appointments = $"{{\"__type\":\"GetMachineAppointmentsRequest:http://services.varian.com/AriaWebConnect/Link\",{JsonConvert.SerializeObject(getMachineAppointmentsRequest).TrimStart('{')}}}";
            string response_appointments = SendData(request_appointments, true, apiKey);
            GetMachineAppointmentsResponse getMachineAppointmentsResponse = JsonConvert.DeserializeObject<GetMachineAppointmentsResponse>(response_appointments);
            Dictionary<string, int> appointments = new Dictionary<string, int>();
            foreach (var appointment in getMachineAppointmentsResponse.MachineAppointments)
            {
                if (appointments.Keys.Contains(appointment.PatientId.Value))
                {
                    appointments[appointment.PatientId.Value]++;
                }
                else
                {
                    appointments.Add(appointment.PatientId.Value, 1);
                }
            }
            Console.WriteLine($"Appointments for the week of {startdate.ToString("MM-dd-yyyy")}\n on Machine {machineId}");
            foreach (var app in appointments)
            {
                Console.WriteLine($"{app.Key}: {app.Value}");
            }
            Console.ReadLine();
        }
        public static string SendData(string request, bool bIsJson, string apiKey)
        {
            var sMediaTYpe = bIsJson ? "application/json" :
           "application/xml";
            var sResponse = System.String.Empty;
            using (var c = new HttpClient(new
           HttpClientHandler()
            { UseDefaultCredentials = true }))
            {
                if (c.DefaultRequestHeaders.Contains("ApiKey"))
                {
                    c.DefaultRequestHeaders.Remove("ApiKey");
                }
                c.DefaultRequestHeaders.Add("ApiKey", apiKey);
                //in App.Config, change this to the Resource ID for your REST Service.
                var task =
               c.PostAsync(ConfigurationManager.AppSettings["GatewayRestUrl"],
                new StringContent(request, Encoding.UTF8,
               sMediaTYpe));
                Task.WaitAll(task);
                var responseTask =
               task.Result.Content.ReadAsStringAsync();
                Task.WaitAll(responseTask);
                sResponse = responseTask.Result;
            }
            return sResponse;
        }

    }
}
