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
            // generate the audit triggers
            var factory = new SqlFactory(context);

            factory.GenerateAuditTrigger("Categories", "CategoryId");
            factory.GenerateAuditTrigger("Inventories", "InventoryId");
            factory.GenerateAuditTrigger("InventoryHires", "InventoryHireId");
            factory.GenerateAuditTrigger("Jobs", "JobId");
            factory.GenerateAuditTrigger("Plants", "PlantId");
            factory.GenerateAuditTrigger("PlantHires", "PlantHireId");
            factory.GenerateAuditTrigger("Roles", "RoleId");
            //factory.GenerateAuditTrigger("Settings", "Key");
            factory.GenerateAuditTrigger("Status", "StatusId");
            factory.GenerateAuditTrigger("Users", "UserId");

            factory.GenerateAuditTrigger("UserRoles", "User_UserId", "Role_RoleId");

            // seed
            var settings = new List<Setting>
            {
                new Setting { Key = "IsAuditingEnabled", Value = "TRUE" }
            };

            settings.ForEach(setting => context.Settings.AddOrUpdate(setting));
            context.SaveChanges();

            var statuses = new List<Status>
            {
                new Status { StatusId = 1, Name = "Unknown" },
                new Status { StatusId = 2, Name = "Available" },
                new Status { StatusId = 3, Name = "Unavailable" },
                new Status { StatusId = 4, Name = "Missing" },
                new Status { StatusId = 5, Name = "Stolen" },
                new Status { StatusId = 6, Name = "Under repair" },
                new Status { StatusId = 7, Name = "Written off" }
            };

            statuses.ForEach(status => context.Statuses.AddOrUpdate(status));
            context.SaveChanges();

            var users = new List<User>
            {
                new User { UserId = 1, Name = @"BRETT-PC\Brett", IsGroup = false, IsEnabled = true },
                new User { UserId = 2, Name = @"RUSPDB\PDBDev", IsGroup = false, IsEnabled = true },
                new User { UserId = 3, Name = @"constructors\jassen", IsGroup = false, IsEnabled = true },
                new User { UserId = 4, Name = @"constructors\allan.payne", IsGroup = false, IsEnabled = true },
            };

            users.ForEach(user => context.Users.AddOrUpdate(user));
            context.SaveChanges();

            var roles = new List<Role>
            {
                new Role { RoleId = 1, Name = "Administrator", Users = users },
                new Role { RoleId = 32768, Name = "Temp" }
            };

            roles.ForEach(role => context.Roles.AddOrUpdate(role));
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
