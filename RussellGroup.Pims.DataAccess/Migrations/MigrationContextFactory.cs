using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.Migrations
{
    public class MigrationContextFactory : IDbContextFactory<PimsDbContext>
    {
        public PimsDbContext Create()
        {
            var httpContext = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter()))
            {
                User = new GenericPrincipal(new GenericIdentity("System", "User"), null)
            };

            HttpContext.Current = httpContext;

            return new PimsDbContext(httpContext);
        }
    }
}
