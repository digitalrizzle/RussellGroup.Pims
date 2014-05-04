using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Helpers
{
    public class ActiveDirectoryHelper : IActiveDirectoryHelper
    {
        private readonly IUserRepository _repository;

        public PrincipalContext Context { get; private set; }

        public ActiveDirectoryHelper(IUserRepository _repository)
        {
            this._repository = _repository;
        }

        void IDisposable.Dispose()
        {
            _repository.Dispose();

            if (Context != null)
            {
                Context.Dispose();
            }
        }

        public ApplicationUser GetCurrentUser()
        {
            return _repository.GetAll().SingleOrDefault(f => f.UserName.Equals(HttpContext.Current.User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        // gets the current user, and if disabled then returns false
        // even if the user belongs to a group that is enabled
        public bool IsAuthorized(string roles)
        {
            ApplicationUser user = GetCurrentUser();

            return IsAuthorized(user, roles);
        }

        private bool IsAuthorized(ApplicationUser user, string roles)
        {
            if (IsAuthenticated())
            {
                if (user != null && !user.LockoutEnabled)
                {
                    if (IsUserInRole(user, roles))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsUserInRole(ApplicationUser user, string roles)
        {
            foreach (var role in roles.Split(','))
            {
                if (user.Roles.Any(f => f.RoleId == _repository.GetAllRoles().Single(r => r.Name == role).Id))
                {
                    return true;
                }
            }

            return false;
        }
    }
}