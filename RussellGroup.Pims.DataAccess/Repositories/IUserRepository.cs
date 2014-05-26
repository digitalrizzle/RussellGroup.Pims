using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<IdentityResult> AddAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<IdentityResult> UpdateAsync(ApplicationUser user, IEnumerable<string> roles);
        new Task<IdentityResult> RemoveAsync(params object[] keyValues);
        IQueryable<ApplicationRole> GetAllRoles();
    }
}
