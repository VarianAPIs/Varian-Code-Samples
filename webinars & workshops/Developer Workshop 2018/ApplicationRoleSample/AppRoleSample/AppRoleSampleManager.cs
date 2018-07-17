using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Common;
using Helpers;
using SampleDAL;

namespace AppRoleSample
{
    class AppRoleSampleManager
    {
        private AppRoleHelper _AppRoleHelper;

        public static SecureString CachedAppRolePassword { get; private set; }
        public static IDAL DAL { get; set; }
        public bool IsAppRoleRetrieved { get; private set; }
        public bool IsAppRoleSet { get; private set; }

        private bool _connected;
        public bool Connected
        {
            get
            {
                return _connected;
            }
            private set
            {
                _connected = value;
                if (value == false) IsAppRoleSet = false;
            }
        }

        private SharedFrameworkReader _sharedFrameworkReader;

        public AppRoleSampleManager(AppRoleHelper appRoleHelper)
        {
            _AppRoleHelper = appRoleHelper;
            IsAppRoleRetrieved = false;
            IsAppRoleSet = false;

            _sharedFrameworkReader = new SharedFrameworkReader(appRoleHelper.GatewayTokenUri);
        }

        public bool GetAppRolePassword(string accessToken, out string status)
        {
            status = ""; 
            
            try
            {
                CachedAppRolePassword = _AppRoleHelper.GetAppRolePassword(ConnectivitySettings.AppRole, accessToken);
                status = "Retrieved password for application role: " + ConnectivitySettings.AppRole;
                return IsAppRoleRetrieved = true;
            }
            catch (Exception e)
            {
                FormatException(ref status, e);
                return false;
            }
        }

        public bool ConnectODBC(out string status)
        {
            CloseDBConnection(out status);
            
            try
            {
                ConnectionParameters connectionParameters = new ConnectionParameters();
                connectionParameters.AppRoleName = ConnectivitySettings.AppRole;
                connectionParameters.AppRolePassword = CachedAppRolePassword;
                connectionParameters.DSNName = ConnectivitySettings.DSNName;

                DAL = DAL_ODBC.Instance(connectionParameters);
                status = DAL.Connect();
                return Connected = true;
            }
            catch (Exception e)
            {
                FormatException(ref status, e);
                return false;
            }
        }

        public bool ConnectADONet(string accessToken, out string status)
        {
            CloseDBConnection(out status);

            try
            {
                ConnectionParameters connectionParameters = new ConnectionParameters();
                connectionParameters.AppRoleName = ConnectivitySettings.AppRole;
                connectionParameters.AppRolePassword = CachedAppRolePassword;
                connectionParameters.UseSSL = true;

                var dataSource = _sharedFrameworkReader.GetDataSource(accessToken);
                connectionParameters.Server = dataSource.Database.Hostname;
                connectionParameters.Database = dataSource.Database.Databasename;
                connectionParameters.Port = Int32.Parse(dataSource.Database.Portnumber);

                DAL = DAL_ADOnet.Instance(connectionParameters);
                status = DAL.Connect();
                return Connected = true;
            }
            catch (Exception e)
            {
                FormatException(ref status, e);
                return Connected = false;
            }
        }

        public void CloseDBConnection(out string status)
        {
            status = "closing connection...";
            Connected = false;
            if (DAL != null)
                status = DAL.CloseConnections();

        }

        public bool SetAppRolePassword(out string status)
        {
            status = "";

            try
            {
                if (DAL.IsAppRoleSet)
                {
                    status = "AppRole already set for pooled connection.";
                    return true;
                }
                    
                status = DAL.SetAppRole();
            }
            catch (Exception e)
            {
                FormatException(ref status, e);
                return false;
            }

            return true;
        }

        public string Query(string query)
        {
            string status = "";
            if (String.IsNullOrEmpty(query))
                query = "SELECT count(1) FROM ss_code_table";

            try
            {
                object resultSet = DAL.ExecuteScalar(query);
                status = String.Format("QUERY: {0}\nRESULT: {1}", query, resultSet.ToString());
            }
            catch (Exception e)
            {
                FormatException(ref status, e);
            }

            return status;
        }


        private  void FormatException(ref string status, Exception e)
        {
            status = "Unhandled exception: " + e.Message +
                     "\nInner exception: " + (e.InnerException == null ? "null" : e.InnerException.Message);
        }


    }
}
