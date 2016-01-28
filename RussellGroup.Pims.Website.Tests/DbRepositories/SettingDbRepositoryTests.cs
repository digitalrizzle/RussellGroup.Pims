using Microsoft.VisualStudio.TestTools.UnitTesting;
using RussellGroup.Pims.DataAccess;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.Website.Tests.DbRepositories
{
    [TestClass]
    public class SettingDbRepositoryTests
    {
        private PimsDbContext Context { get; set; }
        private IRepository<Setting> Repository { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Context = new PimsDbContext(HttpContext.Current);
            Repository = new DbRepository<Setting>(Context);

            Context.Database.Initialize(true);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_setting_can_be_added()
        {
            // arrange
            var setting = TestDataFactory.GetSettings().First();

            // act
            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            setting = await Repository.FindAsync("LastIssuedDocket");

            // assert
            Assert.AreEqual("LastIssuedDocket", setting.Key);
            Assert.AreEqual("900001", setting.Value);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_setting_can_be_updated()
        {
            var setting = TestDataFactory.GetSettings().First();
            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            // update
            setting.Value = "900002";
            result = await Repository.UpdateAsync(setting);
            Assert.IsTrue(result > 0);

            setting = await Repository.FindAsync("LastIssuedDocket");

            Assert.AreEqual("LastIssuedDocket", setting.Key);
            Assert.AreEqual("900002", setting.Value);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_setting_can_be_removed()
        {
            var setting = TestDataFactory.GetSettings().First();

            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            result = await Repository.RemoveAsync(setting);
            Assert.IsTrue(result > 0);
        }
    }
}
