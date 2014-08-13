using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    // http://www.codeproject.com/Articles/228865/Csharp-IDisposable-pattern-on-sub-classes
    public class DbRepository<T> : IRepository<T> where T : class
    {
        protected PimsDbContext Db { get; private set; }

        public DbRepository(PimsDbContext context)
        {
            Db = context;
        }

        protected string UserName
        {
            get { return HttpContext.Current.User.Identity.Name; }
        }

        public async Task<T> FindAsync(params object[] keyValues)
        {
            return await Db.Set<T>().FindAsync(keyValues);
        }

        public IQueryable<T> GetAll()
        {
            return Db.Set<T>();
        }

        public virtual async Task<int> AddAsync(T item)
        {
            Db.Set<T>().Add(item);
            return await Db.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateAsync(T item)
        {
            Db.Entry(item).State = EntityState.Modified;
            return await Db.SaveChangesAsync();
        }

        public virtual async Task<int> RemoveAsync(params object[] keyValues)
        {
            var item = await FindAsync(keyValues);
            Db.Set<T>().Remove(item);
            return await Db.SaveChangesAsync();
        }
    }
}
