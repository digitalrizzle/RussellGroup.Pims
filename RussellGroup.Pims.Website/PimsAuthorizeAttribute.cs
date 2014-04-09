using Ninject;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website
{
    public class AuthorizationFilter : AuthorizeAttribute
    {
        private readonly IActiveDirectoryHelper helper;

        public AuthorizationFilter(IActiveDirectoryHelper helper)
        {
            this.helper = helper;
        }

        [Inject]
        public new string[] Roles { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!helper.IsAuthorized(Roles))
            {
                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }

            base.OnAuthorization(filterContext);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PimsAuthorizeAttribute : Attribute
    {
        public string[] Roles;
    }
}
