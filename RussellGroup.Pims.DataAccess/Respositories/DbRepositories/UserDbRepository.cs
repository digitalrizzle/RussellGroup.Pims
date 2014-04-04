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

        public async Task Update(ApplicationUser user)
        {
            await Update(user, null);
        }

        public async Task Update(ApplicationUser user, string[] roles)
        {
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to update user.");
            }

            foreach (var role in await this.Roles.Select(f => f.Name).ToArrayAsync())
            {
                if (await userManager.IsInRoleAsync(user.Id, role))
                {
                    result = await userManager.RemoveFromRoleAsync(user.Id, role);

                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to remove user from role.");
                    }
                }
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

        public IQueryable<ApplicationRole> Roles
        {
            get
            {
                return roleManager.Roles;
            }
        }

        public void Dispose()
        {
            userManager.Dispose();
            roleManager.Dispose();

            base.Dispose();
        }
    }
}
