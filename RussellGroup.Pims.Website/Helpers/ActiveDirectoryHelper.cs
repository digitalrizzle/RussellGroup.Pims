using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
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

        public ActiveDirectoryHelper(IUserRepository repository)
        {
            _repository = repository;
        }

        public ApplicationUser GetCurrentUser()
        {
            return _repository.GetAll().SingleOrDefault(f =>
                f.UserName.Equals(HttpContext.Current.User.Identity.Name, StringComparison.OrdinalIgnoreCase)
            );
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public bool IsAuthorized(string[] roles)
        {
            return IsAuthorized(GetCurrentUser(), roles);
        }

        public bool IsAuthorized(ApplicationUser user, string[] roles)
        {
            if (!IsAuthenticated()) return false;
            if (user == null || user.LockoutEnabled) return false;
             
            return IsUserInRole(user, roles);
        }

        private bool IsUserInRole(ApplicationUser user, string[] roles)
        {
            return roles.Any(role =>
                user.Roles.Any(f => f.RoleId == _repository.GetAllRoles().Single(r => r.Name == role).Id)
            );
        }
    }
}