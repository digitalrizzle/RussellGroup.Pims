using RussellGroup.Pims.DataAccess;
using RussellGroup.Pims.DataAccess.Migrations;
using System.Data.Entity;

namespace RussellGroup.Pims.Website.Tests
{
    public class UnitTestInitializer : DropCreateDatabaseIfModelChanges<PimsDbContext>
    {
        public override void InitializeDatabase(PimsDbContext context)
        {
            base.InitializeDatabase(context);

            context.Database.ExecuteSqlCommand("DELETE FROM dbo.Settings");
        }

        protected override void Seed(PimsDbContext context)
        {
            base.Seed(context);

            var factory = new SqlFactory(context, null, null);
            factory.GenerateSetUserNameContextStoredProcedure();

            //context.SaveChanges();
        }
    }
}
