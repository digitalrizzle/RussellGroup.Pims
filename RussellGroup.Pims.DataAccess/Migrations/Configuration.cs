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
            factory.GenerateAuditTrigger("Contacts", "ContactId");
            factory.GenerateAuditTrigger("Inventories", "InventoryId");
            factory.GenerateAuditTrigger("InventoryHires", "InventoryHireId");
            factory.GenerateAuditTrigger("Jobs", "JobId");
            factory.GenerateAuditTrigger("Plants", "PlantId");
            factory.GenerateAuditTrigger("PlantHires", "PlantHireId");
            factory.GenerateAuditTrigger("Roles", "RoleId");
            factory.GenerateAuditTrigger("Settings", "Key");
            factory.GenerateAuditTrigger("Users", "UserId");

            factory.GenerateAuditTrigger("UserRoles", "User_UserId", "Role_RoleId");

            // seed
            var settings = new List<Setting>
            {
                new Setting { Key = "IsAuditingEnabled", Value = "1" }
            };

            settings.ForEach(setting => context.Settings.AddOrUpdate(setting));
            context.SaveChanges();

            var users = new List<User>
            {
                new User { UserId = 1, Name = "Brett", IsGroup = false, IsEnabled = true }
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
