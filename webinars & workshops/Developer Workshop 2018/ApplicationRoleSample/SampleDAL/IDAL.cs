using System;

namespace SampleDAL
{
    public interface IDAL
    {
        Boolean IsAppRoleSet { get; }

        string Connect();
        string SetAppRole();
        object ExecuteScalar(string sql);
        string CloseConnections();
    }
}
