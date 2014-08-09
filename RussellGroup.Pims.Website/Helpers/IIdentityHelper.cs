using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Helpers
{
    public interface IIdentityHelper
    {
        ApplicationUser GetCurrentUser();

        bool IsAuthenticated();
        bool IsAuthorized(string[] roles);
        bool IsAuthorized(ApplicationUser user, string[] roles);
    }
}