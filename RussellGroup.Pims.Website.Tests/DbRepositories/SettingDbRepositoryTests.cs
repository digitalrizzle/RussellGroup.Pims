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

        [TestMethod]
        public async Task Test_that_a_setting_can_be_added()
        {
            // arrange
            var setting = TestDataFactory.GetSetting();

            // act
            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            setting = await Repository.FindAsync("Hello");

            // assert
            Assert.AreEqual("Hello", setting.Key);
            Assert.AreEqual("Hello, setting!", setting.Value);
        }

        [TestMethod]
        public async Task Test_that_a_setting_can_be_updated()
        {
            var setting = TestDataFactory.GetSetting();
            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            // update
            setting.Value = "Hello again.";
            result = await Repository.UpdateAsync(setting);
            Assert.IsTrue(result > 0);

            setting = await Repository.FindAsync("Hello");

            Assert.AreEqual("Hello", setting.Key);
            Assert.AreEqual("Hello again.", setting.Value);
        }

        [TestMethod]
        public async Task Test_that_a_setting_can_be_removed()
        {
            var setting = TestDataFactory.GetSetting();

            var result = await Repository.AddAsync(setting);
            Assert.IsTrue(result > 0);

            result = await Repository.RemoveAsync(setting);
            Assert.IsTrue(result > 0);
        }
    }
}
