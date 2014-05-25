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
    // http://www.codeproject.com/Articles/228865/Csharp-IDisposable-pattern-on-sub-classes
    public class DbRepository<T> : IRepository<T> where T : class
    {
        private bool _disposed;

        protected readonly PimsDbContext Db = new PimsDbContext();

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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Db.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }
    }
}
