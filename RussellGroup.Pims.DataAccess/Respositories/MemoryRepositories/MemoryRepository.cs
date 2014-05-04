﻿using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    // http://www.codeproject.com/Articles/228865/Csharp-IDisposable-pattern-on-sub-classes
    public class MemoryRepository<T> : IRepository<T> where T : class
    {
        private bool _disposed;

        protected List<T> db = new List<T>();

        public virtual async Task<T> FindAsync(params object[] keyValues)
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

        public async Task<T> AddAsync(T item)
        {
            return await Task.Run<T>(() =>
            {
                db.Add(item);
                return item;
            });
        }

        public async Task UpdateAsync(T item)
        {
            await Task.Run(() =>
            {
                // do nothing
            });
        }

        public async Task RemoveAsync(params object[] keyValues)
        {
            await Task.Run(async () =>
            {
                var item = await FindAsync(keyValues);
                db.Remove(item);
            });
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                db = null;
            }

            _disposed = true;
        }
    }
}
