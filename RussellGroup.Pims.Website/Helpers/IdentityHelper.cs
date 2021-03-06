﻿using RussellGroup.Pims.DataAccess.Models;
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
    public class IdentityHelper : IIdentityHelper
    {
        private readonly IUserRepository _repository;
        private readonly ApplicationUser _currentUser;
        private readonly IEnumerable<ApplicationRole> _roles;

        public PrincipalContext Context { get; private set; }

        public IdentityHelper(IUserRepository repository)
        {
            _repository = repository;

            _currentUser = _repository.GetAll().SingleOrDefault(f =>
                    f.UserName.Equals(HttpContext.Current.User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                );

            _roles = _repository.GetAllRoles().ToList();
        }

        public ApplicationUser GetCurrentUser()
        {
            return _currentUser;
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
                user.Roles.Any(f => f.RoleId == _roles.Single(r => r.Name == role).Id)
            );
        }
    }
}