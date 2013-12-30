namespace RussellGroup.Pims.DataAccess.Migrations
{
    using RussellGroup.Pims.DataAccess.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<RussellGroup.Pims.DataAccess.Models.PimsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(PimsContext context)
        {
            var users = new List<User>
            {
                new User { UserId = 1, Name = "Brett", isGroup = false, isEnabled = true }
            };

            users.ForEach(user => context.Users.AddOrUpdate(user));
            context.SaveChanges();

            var roles = new List<Role>
            {
                new Role { RoleId = 1, Name = "Administrator", Users = users },
                new Role { RoleId = 32768, Name = "Temp" },
            };

            roles.ForEach(role => context.Roles.AddOrUpdate(role));
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
