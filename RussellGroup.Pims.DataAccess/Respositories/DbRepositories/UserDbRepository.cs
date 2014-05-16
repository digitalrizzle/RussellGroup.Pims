using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class UserDbRepository : DbRepository<ApplicationUser>, IUserRepository
    {
        private bool _disposed;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserDbRepository()
        {
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Db) { AutoSaveChanges = false });
            _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager) { AllowOnlyAlphanumericUserNames = false };

            _roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(Db));
        }

        public new Task<int> AddAsync(ApplicationUser user)
        {
            throw new NotSupportedException();
        }

        public async Task<IdentityResult> AddAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return result;
            }

            if (await Db.SaveChangesAsync() == 0)
            {
                return IdentityResult.Failed("An unknown error occurred while saving the user.");
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    result = await _userManager.AddToRoleAsync(user.Id, role);
                }
            }

            if (await Db.SaveChangesAsync() == 0)
            {
                result = IdentityResult.Failed("An unknown error occurred while saving the user.");
            }

            return result;
        }

        public new Task<int> UpdateAsync(ApplicationUser user)
        {
            throw new NotSupportedException();
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);
            Db.Users.Attach(user);

            var storedUser = Db.Entry(user).GetDatabaseValues();
            var wasLockoutEnabled = (bool)storedUser["LockoutEnabled"];

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return result;
            }

            if (!wasLockoutEnabled && user.LockoutEnabled)
            {
                result = await _userManager.SetLockoutEndDateAsync(user.Id, DateTime.Now);

                if (!result.Succeeded)
                {
                    return result;
                }
            }

            if (roles != null)
            {
                var storedRoleIds = user.Roles.Select(f => f.RoleId);
                var storedRoles = GetAllRoles().Where(f => storedRoleIds.Contains(f.Id)).Select(f => f.Name);

                // remove roles
                var rolesToRemove = storedRoles.Except(roles).ToArray();

                foreach (var role in rolesToRemove)
                {
                    result = await _userManager.RemoveFromRoleAsync(user.Id, role);

                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }

                // add roles
                var rolesToAdd = roles.Except(storedRoles).ToArray();

                foreach (var role in rolesToAdd)
                {
                    result = await _userManager.AddToRoleAsync(user.Id, role);

                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
            }

            if (await Db.SaveChangesAsync() == 0)
            {
                result = IdentityResult.Failed("An unknown error occurred while saving the user.");
            }

            return result;
        }

        public new async Task<IdentityResult> RemoveAsync(params object[] keyValues)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);

            var user = await _userManager.FindByIdAsync(keyValues[0] as string);

            var result = await _userManager.DeleteAsync(user);

            if (await Db.SaveChangesAsync() == 0)
            {
                result = IdentityResult.Failed("An unknown error occurred while saving the user.");
            }

            return result;
        }

        public IQueryable<ApplicationRole> GetAllRoles()
        {
            return _roleManager.Roles;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _userManager.Dispose();
                _roleManager.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
