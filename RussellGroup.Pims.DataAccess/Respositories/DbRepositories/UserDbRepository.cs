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
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Db));
            _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager) { AllowOnlyAlphanumericUserNames = false };

            _roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(Db));
        }

        public new Task<ApplicationUser> AddAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser> AddAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user.");
            }

            foreach (var role in roles)
            {
                result = await _userManager.AddToRoleAsync(user.Id, role);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to add user to role.");
                }
            }

            return user;
        }

        public new Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(ApplicationUser user, IEnumerable<string> roles, bool lockOut)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);

            var storedUser = await _userManager.FindByIdAsync(user.Id);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to update user.");
            }

            result = await _userManager.SetLockoutEnabledAsync(user.Id, lockOut);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to unlock or lockout user.");
            }

            if (lockOut)
            {
                result = await _userManager.SetLockoutEndDateAsync(user.Id, DateTime.Now);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to set lockout date for user.");
                }
            }

            var storedRoleIds = storedUser.Roles.Select(f => f.RoleId);
            var storedRoles = GetAllRoles().Where(f => storedRoleIds.Contains(f.Id)).Select(f => f.Name);

            // remove roles
            var rolesToRemove = storedRoles.Except(roles).ToArray();

            foreach (var role in rolesToRemove)
            {
                result = await _userManager.RemoveFromRoleAsync(user.Id, role);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to remove user from role.");
                }
            }

            // add roles
            var rolesToAdd = roles.Except(storedRoles).ToArray();

            foreach (var role in rolesToAdd)
            {
                result = await _userManager.AddToRoleAsync(user.Id, role);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to add user to role.");
                }
            }
        }

        public new async Task RemoveAsync(params object[] keyValues)
        {
            Db.SetContextUserName(HttpContext.Current.User.Identity.Name);

            var user = await _userManager.FindByIdAsync(keyValues[0] as string);
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to remove user.");
            }
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
