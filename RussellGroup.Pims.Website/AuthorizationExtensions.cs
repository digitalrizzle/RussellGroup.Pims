#define LOCAL

using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace RussellGroup.Pims.Website
{
    public static class AuthorizationExtensions
    {
        public static string GetUserGivenName(this IIdentity identity)
        {
#if LOCAL
            return "LOCAL";
#else
            using (LDAPHelper helper = new LDAPHelper())
            //using (ActiveDirectoryHelper helper = new ActiveDirectoryHelper())
            {
                return helper.GetUser(identity.Name.Contains("\\") ? identity.Name.Split('\\')[1] : identity.Name).GivenName;
            }
#endif
        }

        public static bool IsInRole(this IIdentity identity, RoleType roles)
        {
            if (identity == null)
            {
                return false;
            }

            if (!identity.IsAuthenticated)
            {
                return false;
            }

#if LOCAL
            return true;
#else

            if (roles.HasFlag(RoleType.Administrator))
            {
                using (LDAPHelper helper = new LDAPHelper())
                //using (ActiveDirectoryHelper helper = new ActiveDirectoryHelper())
                {
                    var groupName = ConfigurationManager.AppSettings["AdministratorActiveDirectoryGroupName"];

                    var isAuthorized = helper
                        .GetGroupsInUser(identity.Name.Contains("\\") ? identity.Name.Split('\\')[1] : identity.Name)
                        .Any(f => f.Equals(groupName));

                    return isAuthorized;
                }
            }

            return true;
#endif
        }

    }
}