using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Migrations
{
    public static class Extensions
    {
        public static int SaveChangesWithErrors(this DbContext context)
        {
            try
            {
                return context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder builder = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    builder.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());

                    foreach (var error in failure.ValidationErrors)
                    {
                        builder.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        builder.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    builder.ToString(), ex
                ); // add the original exception as the innerException
            }
        }
    }
}
