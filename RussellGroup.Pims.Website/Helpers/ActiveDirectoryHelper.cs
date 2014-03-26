using RussellGroup.Pims.DataAccess.Models;
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

        private PimsContext db = new PimsContext();

        public PrincipalContext Context { get; private set; }

        public ActiveDirectoryHelper()
        {
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
            db.Dispose();

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

        public IQueryable<User> GetGroups()
        {
            return db.Users.Where(f => f.IsEnabled && f.IsGroup);
        }

        public User GetCurrentUser()
        {
            return db.Users.SingleOrDefault(f => !f.IsGroup && f.Name.Equals(HttpContext.Current.User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        }

        public User GetMostPrivilegedGroup()
        {
            User mostPrivilegedGroup = null;

            foreach (var group in GetGroups())
            {
                if (HttpContext.Current.User.IsInRole(group.Name))
                {
                    // get the highest role privilege that this user has
                    int roleId = group.Roles.Min(r => r.RoleId);

                    if (mostPrivilegedGroup == null || roleId < mostPrivilegedGroup.RoleId)
                    {
                        mostPrivilegedGroup = group;
                    }
                }
            }

            return mostPrivilegedGroup;
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        // gets the current user, and if disabled then returns false
        // even if the user belongs to a group that is enabled
        public bool IsAuthorized(RoleType roleType)
        {
            User user = GetCurrentUser() ?? GetMostPrivilegedGroup();

            return IsAuthorized(user, roleType);
        }

        private bool IsAuthorized(User user, RoleType roleType)
        {
            if (IsAuthenticated())
            {
                if (user != null && user.IsEnabled)
                {
                    return user.Roles.Any(f => (f.RoleId & (int)roleType) == f.RoleId);
                }
            }

            return false;
        }
    }
}