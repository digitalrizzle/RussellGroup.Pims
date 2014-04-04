using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class DbRepository<T> : IRepository<T> where T : class
    {
        protected PimsContext db = new PimsContext();

        public async Task<T> Find(params object[] keyValues)
        {
            return await db.Set<T>().FindAsync(keyValues);
        }

        public IQueryable<T> GetAll()
        {
            return db.Set<T>();
        }

        public async Task<T> Add(T item)
        {
            var result = db.Set<T>().Add(item);
            await db.SaveChangesAsync();
            return result;
        }

        public async Task Update(T item)
        {
            db.Entry(item).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task Remove(params object[] keyValues)
        {
            var item = await Find(keyValues);
            var result = db.Set<T>().Remove(item);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
