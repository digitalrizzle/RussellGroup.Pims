using Microsoft.VisualStudio.TestTools.UnitTesting;
using RussellGroup.Pims.DataAccess;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.Website.Tests.DbRepositories
{
    [TestClass]
    public class JobDbRepositoryTests
    {
        private PimsDbContext Context { get; set; }
        private IJobRepository Repository { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Context = new PimsDbContext(HttpContext.Current);
            Repository = new JobDbRepository(Context);

            Context.Database.Initialize(true);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_job_can_be_added()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            // act
            var result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            job = await Repository.GetAll().SingleOrDefaultAsync(f => f.XJobId.Equals("DC0001"));

            // assert
            Assert.IsNotNull(job);
            Assert.AreEqual("DC0001", job.XJobId);
            Assert.AreEqual("First Test Job", job.Description);
            Assert.AreEqual(new DateTime(2015, 12, 1), job.WhenStarted);
            Assert.AreEqual(new DateTime(2015, 12, 31), job.WhenEnded);
            Assert.AreEqual("Orlando Hubbard", job.ProjectManager);
            Assert.AreEqual("Jarrod Koonce", job.QuantitySurveyor);
            Assert.AreEqual("orlando.hubbard@unittest.com", job.NotificationEmail);
            Assert.AreEqual("This is a comment.", job.Comment);
        }

        [TestMethod, TestCategory("Repositories")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Test_that_a_job_with_an_XJobId_value_that_already_exists_cannot_be_added()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();
            var result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            // this is our duplicate job
            job = TestDataFactory.GetJobs().First();

            // act
            result = await Repository.AddAsync(job);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_job_can_be_updated()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();
            var result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            // update
            job.XJobId = "DC9999";
            job.Description = "Last Test Job";

            // act
            result = await Repository.UpdateAsync(job);
            Assert.IsTrue(result > 0);

            job = await Repository.GetAll().SingleOrDefaultAsync(f => f.XJobId.Equals("DC9999"));

            // assert
            Assert.IsNotNull(job);
            Assert.AreEqual("DC9999", job.XJobId);
            Assert.AreEqual("Last Test Job", job.Description);
            Assert.AreEqual(new DateTime(2015, 12, 1), job.WhenStarted);
            Assert.AreEqual(new DateTime(2015, 12, 31), job.WhenEnded);
            Assert.AreEqual("Orlando Hubbard", job.ProjectManager);
            Assert.AreEqual("Jarrod Koonce", job.QuantitySurveyor);
            Assert.AreEqual("orlando.hubbard@unittest.com", job.NotificationEmail);
            Assert.AreEqual("This is a comment.", job.Comment);
        }

        [TestMethod, TestCategory("Repositories")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Test_that_a_job_with_an_XJobId_value_that_already_exists_cannot_be_updated()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();
            var result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            job = TestDataFactory.GetJobs().Skip(1).First();
            result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            // attempt to update the second job with the XJobId of the first job
            job.XJobId = "DC0001";

            // act
            result = await Repository.UpdateAsync(job);
        }

        [TestMethod, TestCategory("Repositories")]
        public async Task Test_that_a_job_can_be_removed()
        {
            var job = TestDataFactory.GetJobs().First();

            var result = await Repository.AddAsync(job);
            Assert.IsTrue(result > 0);

            result = await Repository.RemoveAsync(job);
            Assert.IsTrue(result > 0);
        }
    }
}
