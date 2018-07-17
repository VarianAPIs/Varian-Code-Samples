using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SampleDAL.ConnectionPool
{
    public sealed class CustomConnectionPool : IDisposable
    {
        private readonly IWindowsIdentityProvider _windowsIdentityProvider;

        private readonly List<PooledConnection> _pool = new List<PooledConnection>();

        public CustomConnectionPool(IWindowsIdentityProvider identityProvider)
        {
            _windowsIdentityProvider = identityProvider;
        }

        static CustomConnectionPool()
        {
            Instance = new CustomConnectionPool(new WindowsIdentityProvider());
        }

        public static CustomConnectionPool Instance { get; private set; }

        // Get a connection from the pool. If one does not exist, create it.
        public PooledConnection GetConnection(string connectionString)
        {
            PooledConnection connection = GetConnectionFromPool(connectionString);
            if (connection == null)
            {
                // Create a new connection, since one was not found in the pool. After use it will be returned to the pool
                connection = CreatePooledConnection(connectionString);
            }

            return connection;
        }

        // Take a connection from the pool
        private PooledConnection GetConnectionFromPool(string connectionString)
        {
            lock (_pool)
            {
                var pooledConnection = _pool.FirstOrDefault(e =>
                    e.ConnectionString == connectionString &&
                    e.WindowsUserSid == _windowsIdentityProvider.UserSid);

                if (pooledConnection != null)
                {
                    _pool.Remove(pooledConnection);
                }

                return pooledConnection;
            }
        }

        // Create a new pooled connection
        public PooledConnection CreatePooledConnection(string connectionString)
        {
            lock (_pool)
            {
                string userSid = _windowsIdentityProvider.UserSid;

                var connection = new SqlConnection(connectionString);

                return NewPooledConnection(connectionString, connection, userSid);
            }
        }

        // Return a connection to the pool for later use
        internal void ReturnPooledConnection(PooledConnection pooledConnection)
        {
            lock (_pool)
            {
                AddConnectionToPool(pooledConnection);
            }
        }

        private void AddConnectionToPool(PooledConnection pooledConnection)
        {
            if (_pool.All(e => e.Connection != pooledConnection.Connection) && pooledConnection.Connection.State == ConnectionState.Open)
            {
                _pool.Add(pooledConnection);
            }
        }

        private PooledConnection NewPooledConnection(string connectionString, SqlConnection connection, string userSid)
        {
            return new PooledConnection(connectionString, connection, userSid);
        }

        private void CloseConnection(PooledConnection connection)
        {
            try
            {
                connection.Dispose();
            }
            catch { }
        }

        public void Dispose()
        {
            foreach (PooledConnection p in _pool)
                CloseConnection(p);
        }
    }
}
