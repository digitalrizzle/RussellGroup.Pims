using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.Website.Helpers;
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
            using (ActiveDirectoryHelper helper = new ActiveDirectoryHelper())
            {
                return helper.GetUser(identity.Name).GivenName;
            }
#endif
        }
    }
}