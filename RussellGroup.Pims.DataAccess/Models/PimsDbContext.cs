using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class PimsDbContext : IdentityDbContext<ApplicationUser>
    {
        public const string DefaultContextUserName = "System";

        public DbSet<Audit> Audits { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryHire> InventoryHires { get; set; }
        public DbSet<InventoryHireCheckin> InventoryHireCheckins { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantHire> PlantHires { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Status> Statuses { get; set; }

        public PimsDbContext() : base("PimsContext") { }

        public string UserName { get; private set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public void SetContextUserName(string userName)
        {
            UserName = userName;
        }

        public override int SaveChanges()
        {
            ExecuteSetContextUserNameCommand();

            var result = base.SaveChanges();

            return result;
        }

        public override Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (HttpContext.Current != null)
            {
                UserName = HttpContext.Current.User.Identity.Name;
            }

            ExecuteSetContextUserNameCommand();

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        private void ExecuteSetContextUserNameCommand()
        {
            this.Database.ExecuteSqlCommand("SetContextUserName @userName", new SqlParameter("userName", UserName));
        }
    }
}
