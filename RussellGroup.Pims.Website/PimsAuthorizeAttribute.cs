using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RussellGroup.Pims.DataAccess.Models;

namespace RussellGroup.Pims.Website
{
    // this code has been adapted from:
    // http://schotime.net/blog/index.php/2009/02/17/custom-authorization-with-aspnet-mvc

    public class PimsAuthorizeAttribute : AuthorizeAttribute
    {
        public new RoleType Roles;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            return httpContext.User.Identity.IsInRole(Roles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            filterContext.Result = new RedirectResult("~/Home/Unauthorized");
        }
    }
}
