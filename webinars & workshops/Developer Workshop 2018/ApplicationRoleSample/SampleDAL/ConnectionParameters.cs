using System.Security;

namespace SampleDAL
{
    public struct ConnectionParameters
    {
        public string Server;
        public string Database;
        public int Port;
        public string AppRoleName;
        public SecureString AppRolePassword;
        public bool AsynchronousProcessing;
        public bool MultipleActiveResultSets;
        public int? PacketSize;
        public bool UseSSL;
        public string DSNName;
    }
}