namespace RussellGroup.Pims.DataAccess.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using RussellGroup.Pims.DataAccess.Models;
    using RussellGroup.Pims.DataAccess.Respositories;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class Configuration : DbMigrationsConfiguration<PimsContext>
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
            //factory.GenerateAuditTrigger("Settings", "Key");
            factory.GenerateAuditTrigger("Status", "StatusId");

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

            // add users and roles
            CreateRole(context, new string[] { ApplicationRole.CanEdit, ApplicationRole.CanEditUsers });

            CreateUser(context, new ApplicationUser { UserName = @"BRETT-PC\Brett" }, new string[] { ApplicationRole.CanEdit, ApplicationRole.CanEditUsers });
            CreateUser(context, new ApplicationUser { UserName = @"RUSPDB\PDBDev" }, new string[] { ApplicationRole.CanEdit });
            CreateUser(context, new ApplicationUser { UserName = @"constructors\jassen" }, new string[] { ApplicationRole.CanEdit });
            CreateUser(context, new ApplicationUser { UserName = @"constructors\allan.payne" }, new string[] { ApplicationRole.CanEdit });

            base.Seed(context);
        }

        private void CreateRole(PimsContext context, string[] roles)
        {
            var manager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(context));

            foreach (var role in roles)
            {
                if (!manager.RoleExists(role))
                {
                    var ir = manager.Create(new ApplicationRole() { Name = role });

                    if (!ir.Succeeded)
                    {
                        throw new Exception("Failed to create a role.");
                    }
                }
            }
        }

        public void CreateUser(PimsContext context, ApplicationUser user, string[] roles)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            manager.UserValidator = new UserValidator<ApplicationUser>(manager) { AllowOnlyAlphanumericUserNames = false };

            if (manager.FindByName(user.UserName) == null)
            {
                var result = manager.Create(user);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create user. " + result.Errors.First());
                }

                foreach (var role in roles)
                {
                    result = manager.AddToRole(user.Id, role);

                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to add user to role." + result.Errors.First());
                    }
                }
            }
        }
    }
}
