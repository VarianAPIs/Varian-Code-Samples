using Common.SFSettings;
using Helpers;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
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
    /** some possible settings
             * datasource
             * environmentsettingskey
             * globalconfig
             * languages
             * radiationtherapy
             * sharedsettings
             * windowsintegartednauthenticationsetting
             * */
    class Program
    {
        private static string _accessToken;
        private static string _refreshToken;

        private static string _authority;
        private static string _clientIdentifier;
        private static string _clientSecret;
        private static string _scope;
        private static string _gatewayTokenUri;

        private static SFSettingsReader _settingsReader;

        static void Main(string[] args)
        {
            _authority = ConfigurationManager.AppSettings["Authority"];
            _clientIdentifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            _clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            _scope = ConfigurationManager.AppSettings["Scope"];
            _gatewayTokenUri = ConfigurationManager.AppSettings["GatewayTokenUri"];

            _settingsReader = new SFSettingsReader(_gatewayTokenUri);
            //authenticate
            RequestTokenAsync().GetAwaiter().GetResult();
            //read doseunit
            var doseUnits = _getDoseUnits();
            //get list of hospitals
            var hospitals = _getHospitals();
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

        private static string _getDoseUnits()
        {
            var sharedSettings = _settingsReader.GetSharedSettings("sharedsettings", _accessToken);
            return sharedSettings.DoseUnits;
        }


        private static IEnumerable<string> _getHospitals()
        {
            var appRole = _settingsReader.GetAppRole("DevWorkshop", _accessToken);

            return null;
        }
        
    }
}
