using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    // http://www.codeproject.com/Articles/228865/Csharp-IDisposable-pattern-on-sub-classes
    public interface IRepository<T> : IDisposable
    {
        Task<T> FindAsync(params object[] keyValues);
        IQueryable<T> GetAll();
        Task<int> AddAsync(T item);
        Task<int> UpdateAsync(T item);
        Task<int> RemoveAsync(params object[] keyValues);
    }
}
