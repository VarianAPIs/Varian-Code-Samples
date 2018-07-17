using System;
using System.Collections.Generic;
using System.Text;
using VMS.SF.Infrastructure.Contracts.Security;

namespace Extensions
{
    public static class ResponseExtensions
    {
        public static string ToFormattedString(this GetPrivilegesResponse response)
        {
            var result = new StringBuilder();
            result.AppendFormat("Privileges:{0}", Environment.NewLine);
            foreach (var privilege in response.Privileges)
            {
                result.AppendFormat("\tPrivilege:{0}", Environment.NewLine);
                result.AppendFormat("\t\tApplications:");
                result.AppendFormat("{0}{1}"
                    , string.Join(", ", privilege.Applications)
                    , Environment.NewLine);

                result.AppendFormat("\t\tGroupIds:");
                result.AppendFormat("{0}{1}"
                    , string.Join(", ", privilege.GroupIds)
                    , Environment.NewLine);

                result.AppendFormat("\t\tPrivileges: category:{0}, group:{1}, id:{2}, name:{3}, version:{4}{5}"
                    , privilege.Privilege.Category
                    , privilege.Privilege.Group
                    , privilege.Privilege.Id
                    , privilege.Privilege.Name
                    , privilege.Privilege.Version
                    , Environment.NewLine);


            }
            return result.ToString();
        }

    }
}
