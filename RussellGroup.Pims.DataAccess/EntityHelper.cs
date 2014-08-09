using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Reflection;

namespace RussellGroup.Pims.DataAccess
{
    // http://michaelmairegger.wordpress.com/2013/03/30/find-primary-keys-from-entities-from-dbcontext/
    public sealed class EntityHelper
    {
        private static readonly Lazy<EntityHelper> LazyInstance = new Lazy<EntityHelper>(() => new EntityHelper());
        private readonly Dictionary<Type, string[]> _dictionary = new Dictionary<Type, string[]>();

        private EntityHelper() { }

        public static EntityHelper Instance
        {
            get { return LazyInstance.Value; }
        }

        public string[] GetKeyNames<T>(DbContext context) where T : class
        {
            Type type = typeof(T);

            // not applicable
            //// retreive the base type
            //while (t.BaseType != typeof(object))
            //    t = t.BaseType;

            string[] keys;

            _dictionary.TryGetValue(type, out keys);

            if (keys != null)
            {
                return keys;
            }

            ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;

            // create method CreateObjectSet with the generic parameter of the base-type
            MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes)
                                                     .MakeGenericMethod(type);

            dynamic objectSet = method.Invoke(objectContext, null);

            IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

            string[] keyNames = keyMembers.Select(f => (string)f.Name).ToArray();

            _dictionary.Add(type, keyNames);

            return keyNames;
        }

        public object[] GetKeyValues<T>(DbContext context, T entity) where T : class
        {
            var keyNames = GetKeyNames<T>(context);
            Type type = typeof(T);

            object[] keys = new object[keyNames.Length];

            for (int index = 0; index < keyNames.Length; index++)
            {
                keys[index] = type.GetProperty(keyNames[index]).GetValue(entity, null);
            }

            return keys;
        }
    }
}
