using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    // http://www.codeproject.com/Articles/228865/Csharp-IDisposable-pattern-on-sub-classes
    public interface IRepository<T> : IDisposable
    {
        Task<T> Find(params object[] keyValues);

        IQueryable<T> GetAll();

        Task<T> Add(T item);
        Task Update(T item);
        Task Remove(params object[] keyValues);
    }
}
