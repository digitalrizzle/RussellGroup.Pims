using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess
{
    public class PimsDbContext : IdentityDbContext<ApplicationUser>
    {
        internal const string SystemUserName = "system";

        public DbSet<Audit> Audits { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryHire> InventoryHires { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantHire> PlantHires { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Status> Statuses { get; set; }

        public PimsDbContext(HttpContext context) : base("PimsContext")
        {
            HttpContext = context;
        }

        public HttpContext HttpContext { get; private set; }

        public string UserName
        {
            get
            {
                string userName = null;

                try
                {
                    userName = HttpContext.User.Identity.Name;
                }
                catch
                {
                    // do nothing
                }

                if (string.IsNullOrEmpty(userName))
                {
                    //throw new InvalidOperationException("The UserName could not be obtained.");
                    System.Diagnostics.Debug.WriteLine("The UserName could not be obtained.");
                    userName = "system";
                }

                return userName;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public override int SaveChanges()
        {
            DbConnection connection = base.Database.Connection;
            ExecuteSetContextUserNameCommand(connection);

            var result = base.SaveChanges();

            return result;
        }

        public override async Task<int> SaveChangesAsync()
        {
            return await SaveChangesAsync(CancellationToken.None);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            DbConnection connection = base.Database.Connection;
            ExecuteSetContextUserNameCommand(connection);

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        private void ExecuteSetContextUserNameCommand(DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            DbCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "SetContextUserName";
            command.Parameters.Add(new SqlParameter("userName", UserName));
            command.ExecuteNonQuery();
        }
    }
}
