using System.Security;

namespace Helpers
{
    public static class ConnectivitySettings
    {
        public static string ApiKey { get; set; }
        public static string GatewayRestUri { get; set; }
        public static string ServiceUser { get; set; }
        public static string AccessToken { get; set; }
        public static SecureString ServicePassword { get; set; }
    }
}
