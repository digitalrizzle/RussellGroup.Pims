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
        public static readonly bool BYPASS_LDAP = bool.Parse(ConfigurationManager.AppSettings["BypassLdap"] ?? "false");

        private const int MAX_PAGE_SIZE = 1000 * 1024 * 1024;

        private readonly IUserRepository repository;

        public PrincipalContext Context { get; private set; }

        public ActiveDirectoryHelper(IUserRepository repository)
        {
            this.repository = repository;

            if (!BYPASS_LDAP)
            {
                var domain = ConfigurationManager.AppSettings["Domain"];
                var container = ConfigurationManager.AppSettings["Container"];
                //var storeUserName = ConfigurationManager.AppSettings["StoreUserName"];
                //var storePassword = ConfigurationManager.AppSettings["StorePassword"];

                //if (storeUserName != null && storePassword != null)
                //{
                //    Context = new PrincipalContext(ContextType.Domain, domain, container, storeUserName, storePassword);
                //}
                //else
                //{
                Context = new PrincipalContext(ContextType.Domain, domain, container);
                //}
            }
        }

        void IDisposable.Dispose()
        {
            repository.Dispose();

            if (Context != null)
            {
                Context.Dispose();
            }
        }

        private IEnumerable<Principal> Search(Principal principal)
        {
            using (PrincipalSearcher searcher = new PrincipalSearcher(principal))
            {
                ((DirectorySearcher)searcher.GetUnderlyingSearcher()).PageSize = MAX_PAGE_SIZE;
                return searcher.FindAll();
            }
        }

        private string RemoveDomain(string samAccountName)
        {
            return samAccountName.Contains("\\") ? samAccountName.Split('\\')[1] : samAccountName;
        }

        #region Groups

        public GroupPrincipal GetGroup(string groupSamAccountName)
        {
            return GroupPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, RemoveDomain(groupSamAccountName));
        }

        public IEnumerable<GroupPrincipal> GetAllGroups()
        {
            using (GroupPrincipal group = new GroupPrincipal(Context))
            {
                return Search(group).Cast<GroupPrincipal>();
            }
        }

        public IEnumerable<GroupPrincipal> GetGroupsInUser(string userSamAccountName)
        {
            using (UserPrincipal user = UserPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, RemoveDomain(userSamAccountName)))
            {
                if (user != null)
                {
                    return user.GetGroups().Cast<GroupPrincipal>();
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Users

        public UserPrincipal GetUser(string userSamAccountName)
        {
            return UserPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, RemoveDomain(userSamAccountName));
        }

        public IEnumerable<UserPrincipal> GetAllUsers()
        {
            using (GroupPrincipal group = GroupPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, "Domain Users"))
            {
                if (group != null)
                {
                    return group.GetMembers(false).Cast<UserPrincipal>();
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<UserPrincipal> GetUsersInGroup(string groupSamAccountName)
        {
            using (GroupPrincipal group = GroupPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, RemoveDomain(groupSamAccountName)))
            {
                if (group != null)
                {
                    return group.GetMembers(false).Cast<UserPrincipal>();
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        public ApplicationUser GetCurrentUser()
        {
            return repository.GetAll().SingleOrDefault(f => f.UserName.Equals(HttpContext.Current.User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            //return repository.GetAll().Single(f => f.UserName == @"RUSPDB\PDBDev");
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        // gets the current user, and if disabled then returns false
        // even if the user belongs to a group that is enabled
        public bool IsAuthorized(string[] roles)
        {
            ApplicationUser user = GetCurrentUser();

            return IsAuthorized(user, roles);
        }

        private bool IsAuthorized(ApplicationUser user, string[] roles)
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

        private bool IsUserInRole(ApplicationUser user, string role)
        {
            return IsUserInRole(user, new string[] { role });
        }

        private bool IsUserInRole(ApplicationUser user, string[] roles)
        {
            foreach (var role in roles)
            {
                if (user.Roles.Any(f => f.RoleId == repository.GetAllRoles().Single(r => r.Name == role).Id))
                {
                    return true;
                }
            }

            return false;
        }
    }
}