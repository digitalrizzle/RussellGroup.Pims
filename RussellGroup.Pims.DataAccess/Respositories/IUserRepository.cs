using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> AddAsync(ApplicationUser user, IEnumerable<string> roles);
        Task UpdateAsync(ApplicationUser user, IEnumerable<string> roles, bool lockOut);
        IQueryable<ApplicationRole> GetAllRoles();
    }
}
