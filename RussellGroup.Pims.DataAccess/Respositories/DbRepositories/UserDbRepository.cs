using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class UserDbRepository : DbRepository<ApplicationUser>, IUserRepository
    {
        protected readonly UserManager<ApplicationUser> userManager;
        protected readonly RoleManager<ApplicationRole> roleManager;

        public UserDbRepository()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager) { AllowOnlyAlphanumericUserNames = false };

            roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(db));
        }

        public async Task<ApplicationUser> Add(ApplicationUser user)
        {
            return await Add(user, null);
        }

        public async Task<ApplicationUser> Add(ApplicationUser user, string[] roles)
        {
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user.");
            }

            foreach (var role in roles)
            {
                result = await userManager.AddToRoleAsync(user.Id, role);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to add user to role.");
                }
            }

            await db.SaveChangesAsync();

            return user;
        }

        public Task Update(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task Update(ApplicationUser user, string[] roles)
        {
            var result = userManager.Update(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to update user.");
            }

            foreach (var userRole in user.Roles.ToArray())
            {
                var role = GetAllRoles().Single(f => f.Id == userRole.RoleId);
                result = userManager.RemoveFromRole(user.Id, role.Name);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to remove user from role.");
                }
            }

            foreach (var role in roles)
            {
                if (!userManager.IsInRole(user.Id, role))
                {
                    result = userManager.AddToRole(user.Id, role);

                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to add user to role.");
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task Remove(params object[] keyValues)
        {
            var user = await userManager.FindByIdAsync(keyValues[0] as string);
            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to remove user.");
            }

            await db.SaveChangesAsync();
        }

        public IQueryable<ApplicationRole> GetAllRoles()
        {
            return roleManager.Roles;
        }

        public IQueryable<ApplicationRole> GetRoles(ApplicationUser user)
        {
            return GetAllRoles().Where(f => ((ApplicationRole)user.Roles).Id == f.Id);
        }

        public IQueryable<ApplicationRole> GetRoles(IEnumerable<string> roleIds)
        {
            return GetAllRoles().Where(r => roleIds.Contains(r.Id));
        }

        public new void Dispose()
        {
            userManager.Dispose();
            roleManager.Dispose();

            base.Dispose();
        }
    }
}
