using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class UserDbRepository : DbRepository<User>, IUserRepository
    {
        public new async Task<User> Add(User user)
        {
            Role role = db.Roles.Single(r => r.RoleId == user.RoleId);
            user.Roles = new List<Role>();
            user.Roles.Add(role);

            var result = db.Users.Add(user);
            await db.SaveChangesAsync();
            return result;
        }

        public new async Task<User> Update(User user)
        {
            db.Entry(user).State = EntityState.Modified;

            await db.Entry(user).Collection(f => f.Roles).LoadAsync();
            Role role = db.Roles.Single(r => r.RoleId == user.RoleId);
            user.Roles.RemoveAll();
            user.Roles.Add(role);

            await db.SaveChangesAsync();
            return user;
        }

        public async Task Remove(params object[] keyValues)
        {
            User user = await db.Users.FindAsync(keyValues);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }

        public IQueryable<Role> Roles
        {
            get { return db.Roles; }
        }
    }
}
