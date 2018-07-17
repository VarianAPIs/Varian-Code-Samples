using System;
using System.Data;
using System.Data.SqlClient;
using SampleDAL.ConnectionPool;

namespace SampleDAL
{
    public class DAL_ADOnet : IDAL
    {
        private static ConnectionParameters _connectionParameters;
        private PooledConnection _pooledConnection;
        private static DAL_ADOnet _singleton;

        private DAL_ADOnet(ConnectionParameters connectionParameters)
        {
            _connectionParameters = connectionParameters;
        }

        public static IDAL Instance(ConnectionParameters connectionParameters)
        {
            if (_singleton == null)
            {
                _singleton = new DAL_ADOnet(connectionParameters);
            }
            
            return _singleton;
        }

        public Boolean IsAppRoleSet { get; private set; }

        public string Connect()
        {
            try
            {
                _pooledConnection = CustomConnectionPool.Instance.GetConnection(GetConnectionString(_connectionParameters));

                if (_pooledConnection.State == ConnectionState.Open)
                    return "Connection retreived from pool";

                _pooledConnection.Open();
            }
            catch (Exception e)
            {
                return String.Format("Failed to connect. {0}", e.Message);
            }
            return "Opened pooled ADO.Net connection";
        }

        public string SetAppRole()
        {
            _pooledConnection.SetAppRole(_connectionParameters.AppRoleName, _connectionParameters.AppRolePassword);
            IsAppRoleSet = true;

            return String.Format("Set AppRole {0}", _connectionParameters.AppRoleName);
        }

        public object ExecuteScalar(string sql)
        {
            if (_pooledConnection == null)
                throw new Exception("Open the DB connection first, then set the application role.");

            using (var cmd = _pooledConnection.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                return cmd.ExecuteScalar();
            }
        }

        protected virtual string GetConnectionString(ConnectionParameters connectionParameters)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = string.Format("{0},{1}", connectionParameters.Server, connectionParameters.Port),
                InitialCatalog = connectionParameters.Database,
                AsynchronousProcessing = connectionParameters.AsynchronousProcessing,
                Pooling = false,
                IntegratedSecurity = true,
                MultipleActiveResultSets = connectionParameters.MultipleActiveResultSets,
                Encrypt = connectionParameters.UseSSL,
                TrustServerCertificate = false
            };

            if (connectionParameters.PacketSize != null)
            {
                builder.PacketSize = connectionParameters.PacketSize.Value;
            }

            return builder.ToString();
        }

        public string CloseConnections()
        {
            if (_pooledConnection != null)
            {
                CustomConnectionPool.Instance.ReturnPooledConnection(_pooledConnection);
                _pooledConnection = null;
                return "Connection returned to pool.";
            }

            return "Connection already closed.";
        }
    }
}
