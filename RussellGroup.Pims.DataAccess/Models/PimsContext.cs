using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class PimsContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Audit> Audits { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryHire> InventoryHires { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantHire> PlantHires { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Status> Statuses { get; set; }

        public PimsContext() : base("PimsContext") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var task = this.SaveChangesAsync();
            task.Wait();

            return task.Result;
        }

        public override async Task<int> SaveChangesAsync()
        {
            return await this.SaveChangesAsync(CancellationToken.None);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            DbConnection connection = base.Database.Connection;
            if (connection.State == ConnectionState.Closed) connection.Open();

            if (HttpContext.Current != null)
            {
                DbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SetContextUserName";
                command.Parameters.Add(new SqlParameter("userName", HttpContext.Current.User.Identity.Name));
                command.ExecuteNonQuery();
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            connection.Close();

            return result;
        }
    }
}
