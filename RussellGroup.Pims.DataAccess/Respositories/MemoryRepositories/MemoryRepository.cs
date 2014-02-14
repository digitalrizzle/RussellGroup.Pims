using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class MemoryRepository<T> : IRepository<T> where T : class
    {
        protected List<T> db = new List<T>();

        public virtual async Task<T> Find(params object[] keyValues)
        {
            return await Task.Run<T>(() =>
            {
                foreach (var entity in db)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var value in keyValues)
                            {
                                if (value.Equals(property.GetValue(entity)))
                                {
                                    return entity;
                                }
                            }
                        }
                    }
                }

                return (T)null;
            });
        }

        public IQueryable<T> GetAll()
        {
            return db.AsQueryable();
        }

        public async Task<T> Add(T item)
        {
            return await Task.Run<T>(() =>
            {
                db.Add(item);
                return item;
            });
        }

        public async Task<T> Update(T item)
        {
            return await Task.Run<T>(() =>
            {
                return item;
            });
        }

        public async Task Remove(params object[] keyValues)
        {
            await Task.Run(async () =>
            {
                var item = await Find(keyValues);
                db.Remove(item);
            });
        }

        public void Dispose() { }
    }
}
