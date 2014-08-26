namespace RussellGroup.Pims.DataAccess.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using RussellGroup.Pims.DataAccess.Models;
    using RussellGroup.Pims.DataAccess.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class Configuration : DbMigrationsConfiguration<PimsDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(PimsDbContext context)
        {
            context.SetContextUserName(PimsDbContext.DefaultContextUserName);

            // generate the audit triggers
            var factory = new SqlFactory(context, "Audit", "Setting");

            factory.GenerateSetUserNameContextStoredProcedure();

            factory.GenerateAuditTrigger("Categories");
            factory.GenerateAuditTrigger("Inventories");
            factory.GenerateAuditTrigger("InventoryHires");
            factory.GenerateAuditTrigger("Jobs");
            factory.GenerateAuditTrigger("Plants");
            factory.GenerateAuditTrigger("PlantHires");
            factory.GenerateAuditTrigger(tableName: "Settings", primaryKeyName1: "Key");
            factory.GenerateAuditTrigger("Status");

            factory.GenerateAuditTrigger(tableName: "AspNetUsers", primaryKeyName1: "Id");
            factory.GenerateAuditTrigger(tableName: "AspNetRoles", primaryKeyName1: "Id");
            factory.GenerateAuditTrigger("AspNetUserRoles", "UserId", "RoleId");
            factory.GenerateAuditTrigger("AspNetUserClaims", "Id", "UserId");
            factory.GenerateAuditTrigger("AspNetUserLogins", "ProviderKey", "UserId");

            // seed
            var statuses = new List<Status>
            {
                new Status { Id = Status.Unknown, Name = "Unknown" },
                new Status { Id = Status.Available, Name = "Available" },
                new Status { Id = Status.Unavailable, Name = "Unavailable" },
                new Status { Id = Status.Missing, Name = "Missing" },
                new Status { Id = Status.Stolen, Name = "Stolen" },
                new Status { Id = Status.UnderRepair, Name = "Under repair" },
                new Status { Id = Status.WrittenOff, Name = "Written off" }
            };

            statuses.ForEach(status => context.Statuses.AddOrUpdate(status));
            context.SaveChanges();

            var conditions = new List<Condition>
            {
                new Condition { Id = Condition.Unknown, Name = "Unknown" },
                new Condition { Id = Condition.Poor, Name = "Poor" },
                new Condition { Id = Condition.Fair, Name = "Fair" },
                new Condition { Id = Condition.Good, Name = "Good" },
                new Condition { Id = Condition.Excellent, Name = "Excellent" },
            };

            conditions.ForEach(condition => context.Conditions.AddOrUpdate(condition));
            context.SaveChanges();

            // add users and roles
            CreateRole(context, new ApplicationRole[]
            {
                new ApplicationRole { Name = Role.CanView, Precedence = 1, Description = "View jobs, plant, inventory and hire; cannot edit" },
                new ApplicationRole { Name = Role.CanEdit, Precedence = 2, Description = "Manage jobs, plant, inventory and hire" },
                new ApplicationRole { Name = Role.CanEditCategories, Precedence = 3, Description = "Manage categories" },
                new ApplicationRole { Name = Role.CanEditUsers, Precedence = 4, Description = "Manage users" }
            });

            context.SaveChanges();

            // auditing
            var settings = new List<Setting>
            {
                new Setting { Key = "IsAuditingEnabled", Value = "TRUE" }
            };

            settings.ForEach(setting => context.Settings.AddOrUpdate(setting));
            context.SaveChanges();

            base.Seed(context);
        }

        private void CreateRole(PimsDbContext context, ApplicationRole[] roles)
        {
            var manager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(context));

            foreach (var role in roles)
            {
                if (!manager.RoleExists(role.Name))
                {
                    var ir = manager.Create(role);

                    if (!ir.Succeeded)
                    {
                        throw new Exception("Failed to create a role.");
                    }
                }
            }
        }

        public void CreateUser(PimsDbContext context, ApplicationUser user, string[] roles)
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
