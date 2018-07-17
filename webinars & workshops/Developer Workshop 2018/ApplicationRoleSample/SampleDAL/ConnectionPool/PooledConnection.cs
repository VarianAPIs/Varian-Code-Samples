using System;
using System.Data;
using System.Data.SqlClient;
using System.Security;
using Extensions;

namespace SampleDAL
{
    public class PooledConnection
    {
        #region Properties

        private SqlConnection _connection;
        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                    return null;
                else
                    return _connection;
            }
            set { _connection = value; }
        }

        private string _connectionString;
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set { _connectionString = value; }
        }

        public ConnectionState State
        {
            get { return _connection.State; }
        }

        public string WindowsUserSid { get; set; }

        #endregion Properties

        public PooledConnection(string connectionString, SqlConnection connection, string userSid)
        {
            _connectionString = connectionString;
            _connection = connection;
            WindowsUserSid = userSid;
        }

        public void Open()
        {
            if (_connection.State == ConnectionState.Open)
            {
                throw new Exception("This connection is already opened.");
            }

            _connection.Open();
        }

        public void Close()
        {
            if (_connection != null)
            {
                _connection.Dispose();

                _connection = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void SetAppRole(string appRole, SecureString appRolePassword)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_setapprole";

                var nameParam = cmd.CreateParameter();
                nameParam.ParameterName = "@rolename";
                nameParam.DbType = DbType.String;
                nameParam.Value = appRole;
                cmd.Parameters.Add(nameParam);

                var passwordParam = cmd.CreateParameter();
                passwordParam.ParameterName = "@password";
                passwordParam.DbType = DbType.String;
                passwordParam.Value = appRolePassword.GetNonSecureString();
                cmd.Parameters.Add(passwordParam);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
