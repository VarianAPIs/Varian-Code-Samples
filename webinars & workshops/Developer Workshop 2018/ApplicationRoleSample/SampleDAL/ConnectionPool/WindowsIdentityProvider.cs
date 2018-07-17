using System.Security.Principal;

namespace SampleDAL.ConnectionPool
{
    public interface IWindowsIdentityProvider
    {
        string UserSid { get; }
    }

    class WindowsIdentityProvider : IWindowsIdentityProvider
    {
        public string UserSid
        {
            get
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();

                string sid = null;
                if (windowsIdentity != null && windowsIdentity.User != null)
                {
                    sid = windowsIdentity.User.Value;
                }

                return sid;
            }
        }
    }
}
