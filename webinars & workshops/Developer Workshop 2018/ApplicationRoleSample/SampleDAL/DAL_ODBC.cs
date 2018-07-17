using System;
using System.Data;
using System.Data.Odbc;
using Extensions;

namespace SampleDAL
{
    public class DAL_ODBC : IDAL
    {
        public static OdbcConnection _conn;
        private readonly ConnectionParameters _connectionParameters;
        private static DAL_ODBC _singleton;

        public DAL_ODBC(ConnectionParameters connectionParameters)
        {
            _connectionParameters = connectionParameters;
        }

        public static IDAL Instance(ConnectionParameters connectionParameters)
        {
            if (_singleton == null)
            {
                _singleton = new DAL_ODBC(connectionParameters);
            }

            return _singleton;
        }

        public Boolean IsAppRoleSet { get; private set; }

        public string Connect()
        {
            _conn = new OdbcConnection();
            _conn.ConnectionString = "DSN=" + _connectionParameters.DSNName;
            try
            {
                _conn.Open();
                return "Opened ODBC connection";
            }
            catch (Exception ex)
            {
                _conn.Close();
                return "Failed to connect to data source " + ex.Message;
            }
        }

        public string SetAppRole()
        {
            using (OdbcCommand cmd = _conn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_setapprole '" + _connectionParameters .AppRoleName+ "', '"+_connectionParameters.AppRolePassword.GetNonSecureString()+"'";

                cmd.ExecuteNonQuery();
            }
            IsAppRoleSet = true;

            return String.Format("Set AppRole {0}", _connectionParameters.AppRoleName);
        }

        public object ExecuteScalar(string sql)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                return cmd.ExecuteScalar().ToString();
            }
        }

        public string CloseConnections()
        {
            _conn.Close();
            IsAppRoleSet = false; // Connection is not pooled so the Application Role must be reset in this DAL

            return "Connection closed.";
        }
    }
}
