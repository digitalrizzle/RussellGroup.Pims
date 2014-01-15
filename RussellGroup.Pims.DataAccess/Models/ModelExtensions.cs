using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public static class ModelExtensions
    {
        public static void RemoveAll<T>(this ICollection<T> collection)
        {
            if (collection != null)
            {
                while (collection.Count > 0)
                {
                    collection.Remove(collection.First());
                }
            }
        }
    }
}
