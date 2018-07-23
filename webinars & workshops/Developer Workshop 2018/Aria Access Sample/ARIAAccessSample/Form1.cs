using Hl7.Fhir.Serialization;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.AWC.Link.MU3.WebService.Contracts;
using VMS.AWC.Link.WebService.Contracts;

namespace DevWorkshop2018AriaAccess
{
    public partial class Form1 : Form
    {
        private string _redirectUri;
        private string _authority;
        private string _clientIdentifier;
        private string _scope;
        private string _gatewayTokenUri;
        private string _fhirServerUrl;

        static string _accessToken;
        static string _identityToken;
        static string _refreshToken;

        public Form1()
        {
            InitializeComponent();
            _redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            _authority = ConfigurationManager.AppSettings["Authority"];
            _clientIdentifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            _scope = ConfigurationManager.AppSettings["Scope"];
            _gatewayTokenUri = ConfigurationManager.AppSettings["GatewayTokenUri"];
            _fhirServerUrl = ConfigurationManager.AppSettings["FhirServerUrl"];

            Load += Authenticate;
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSearchPatient_Click(object sender, EventArgs e)
        {
            String strLastName = txtLastName.Text.ToString().TrimEnd();
            String strFirstName = txtFirstName.Text.ToString().TrimEnd();
            //String token = _accessToken;
            //String url = "https://ARIAV15TBox:55051/Gateway/Service.svc/token/Process";

            txtPatientResponse.Text = processSearchPatientRequest(strLastName, strFirstName, _accessToken, _gatewayTokenUri);

        }

        private string processSearchPatientRequest(string strLastName, string strFirstName, String token, String url)
        {
            var request = new PatientSelectionRequest()
            {
                BirthDateFrom = null,
                BirthDateTo = null,
                FirstName = null,
                LastName = null,
                ExactSearch = null,
                Sex = null
            };
            if (String.IsNullOrEmpty(strFirstName) == false)
                request.FirstName = new VMS.AWC.Common.Contracts.String { Value = strFirstName };
            if (String.IsNullOrEmpty(strLastName) == false)
                request.LastName = new VMS.AWC.Common.Contracts.String { Value = strLastName };

            var response = ARIAAccessHelper.SendRequestData(request, url, token);

            return response;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void lblMachineId_Click(object sender, EventArgs e)
        {

        }

        private void txtCreatePatientFilePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSearchAppointment_Click(object sender, EventArgs e)
        {
            
            String hospital = txtHospital.Text.ToString().Trim();
            String department = txtDepartment.Text.ToString().Trim();
            String machineId = txtMachineId.Text.ToString().Trim();

            txtApptResponse.Text = processSearchAppointmentRequest(machineId, hospital,  department, _accessToken, _gatewayTokenUri);
        }

        private string processSearchAppointmentRequest(string machineId, string hospitalName, string departmentname, string token, string url)
        {
            var request = new GetMachineAppointmentsRequest()
            {
                MachineId = new VMS.AWC.Common.Contracts.String() { Value = machineId},
                HospitalName = new VMS.AWC.Common.Contracts.String() { Value = hospitalName },
                DepartmentName = new VMS.AWC.Common.Contracts.String() { Value = departmentname}
            };
            

            var response = ARIAAccessHelper.SendRequestData(request, url, token);

            return response;
        }

        private void btnCreateAppointment_Click(object sender, EventArgs e)
        {
            string filePath = txtCreateApptFilePath.Text.ToString().TrimEnd();
            string request = File.ReadAllText(filePath);
            
            
            var response = ARIAAccessHelper.SendRequestData(request, _gatewayTokenUri, _accessToken);

            txtCreateApptResp.Text = response;


        }

        private void btnCreatePatient_Click(object sender, EventArgs e)
        {
            string filePath = txtCreatePatientFilePath.Text.ToString().TrimEnd();
            string request = File.ReadAllText(filePath);
            

            var response = ARIAAccessHelper.SendRequestData(request, _gatewayTokenUri, _accessToken);

            txtCreatePatientResp.Text = response;
        }

        private void txtCreateApptFilePath_TextChanged(object sender, EventArgs e)
        {

        }

        private async void Authenticate(object sender, EventArgs e)
        {
            var browser = new SystemBrowser();
            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = ConfigurationManager.AppSettings["Authority"],
                ClientId = _clientIdentifier,
                ClientSecret = "secret",
                Scope = "openid profile offline_access " + _scope,
                RedirectUri = redirectUri,
                Browser = browser,
                //FilterClaims = false,
                Policy = new Policy
                {
                    Discovery = new DiscoveryPolicy
                    {
                        ValidateEndpoints = false,
                        ValidateIssuerName = false
                    }
                }
            };


            var oidcClient = new OidcClient(options);
            var loginRequest = new LoginRequest ();

            var result = await oidcClient.LoginAsync(loginRequest);
            if (result.IsError)
            {
                _accessToken = null;
                _identityToken = null;
                _refreshToken = null;

                txtIdentityToken.Text = "Error";
                txtAccessToken.Text = "Error";
            }
            else
            {
                _accessToken = result.AccessToken;
                _identityToken = result.IdentityToken;
                _refreshToken = result.RefreshToken;
            }

            txtIdentityToken.Text = JWTTokenHelper.ReadToken(_identityToken);
            txtAccessToken.Text = JWTTokenHelper.ReadToken(_accessToken);
        }

        private async void RefreshTokens(object sender, EventArgs e)
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
                txtIdentityToken.Text = "Error";
                txtAccessToken.Text = "Error";
            }
            else
            {
                _accessToken = result.AccessToken;
                _refreshToken = result.RefreshToken;
                txtIdentityToken.Text = JWTTokenHelper.ReadToken(_identityToken);
                txtAccessToken.Text = JWTTokenHelper.ReadToken(_accessToken);
            }
        }

        private void btnfhirSearchPatient_Click(object sender, EventArgs e)
        {
            String strLastName = txtLastName.Text.ToString().TrimEnd();
            String strFirstName = txtFirstName.Text.ToString().TrimEnd();

            Hl7.Fhir.Rest.FhirClient client = new Hl7.Fhir.Rest.FhirClient(_fhirServerUrl);
            client.OnBeforeRequest += Client_OnBeforeRequest;
            client.PreferredFormat = Hl7.Fhir.Rest.ResourceFormat.Json;

            List<string> paramsList = new List<string>();

            if (string.IsNullOrEmpty(strLastName) == false)
                paramsList.Add("family=" + strLastName);

            if (string.IsNullOrEmpty(strFirstName) == false)
                paramsList.Add("given=" + strFirstName);


            var bundle = client.Search<Hl7.Fhir.Model.Patient>(paramsList.ToArray());

            txtPatientResponse.Text = new FhirJsonSerializer().SerializeToString(bundle);
        }

        private void Client_OnBeforeRequest(object sender, Hl7.Fhir.Rest.BeforeRequestEventArgs e)
        {
            e.RawRequest.Headers.Add("Authorization", "Bearer " + _accessToken);
        }
    }
}
