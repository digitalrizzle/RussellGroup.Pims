using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Migrations
{
    internal sealed partial class Configuration : DbMigrationsConfiguration<PimsDbContext>
    {
        private void SeedUsers(PimsDbContext context)
        {
            //CreateUser(context, new ApplicationUser { UserName = @"DOMAIN\user name" }, new string[] { Role.CanView, Role.CanEdit, Role.CanEditCategories, Role.CanEditUsers });
            CreateUser(context, new ApplicationUser { UserName = Environment.UserDomainName + "\\" + Environment.UserName }, new string[] { Role.CanView, Role.CanEdit, Role.CanEditCategories, Role.CanEditUsers });

            // include other users here to seed
        }
    }
}
