using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IRepository<T>
    {
        Task<T> FindAsync(params object[] keyValues);
        IQueryable<T> GetAll();
        Task<int> AddAsync(T item);
        Task<int> UpdateAsync(T item);
        Task<int> RemoveAsync(params object[] keyValues);
    }
}
