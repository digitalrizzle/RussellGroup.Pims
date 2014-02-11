using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using RussellGroup.Pims.Website.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RussellGroup.Pims.Website.Tests.Controllers
{
    [TestClass]
    public class JobControllerTest
    {
        private Mock<IRepository<Job>> repository;
        private JobController controller;

        #region Setup

        [TestInitialize]
        public void Initialize()
        {
            repository = new Mock<IRepository<Job>>();

            controller = new JobController(repository.Object);
            controller.SetFakeAuthenticatedControllerContext();
        }

        Job GetJob()
        {
            return GetJob(1, "Orlando Gee", "Jarrod Koonce");
        }

        Job GetJob(int id, string projectManager, string quantitySurveyor)
        {
            return new Job
            {
                JobId = id,
                XJobId = string.Format("X{0}", id),
                Description = string.Format("Test Job {0}", id),
                WhenStarted = DateTime.Now.AddDays(-28).Date,
                WhenEnded = DateTime.Now.Date,
                ProjectManager = projectManager,
                QuantitySurveyor = quantitySurveyor,
                Comment = "This is a comment."
            };
        }

        #endregion

        [TestMethod]
        public void Test_that_the_index_view_is_returned()
        {
            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void Test_that_the_index_data_is_returned()
        {
            // Arrange
            Job job1 = GetJob();
            Job job2 = GetJob(2, "Keith Harris", "Dean Seabolt");

            var jobs = new MemoryRepository<Job>();
            jobs.Add(job1).Wait();
            jobs.Add(job2).Wait();

            repository.Setup(f => f.GetAll()).Returns(jobs.GetAll());

            var parameters = new JqueryDataTableParameterModel()
            {
                iSortCol_0 = 1,
                sSortDir_0 = "asc",
                iDisplayLength = 10,
                iDisplayStart = 0,
                sSearch = string.Empty
            };

            // Act
            var result = controller.GetDataTableResult(parameters);
            var data = result.GetJqueryDataTableAaData();

            // Assert
            Assert.AreEqual(2, data.Count());

            Assert.AreEqual("1", data[0][0]);
            Assert.AreEqual("X1", data[0][1]);
            Assert.AreEqual("Test Job 1", data[0][2]);
            Assert.AreEqual(DateTime.Now.AddDays(-28).Date, DateTime.Parse(data[0][3]));
            Assert.AreEqual(DateTime.Now.Date, DateTime.Parse(data[0][4]));
            Assert.AreEqual("Orlando Gee", data[0][5]);

            Assert.AreEqual("2", data[1][0]);
            Assert.AreEqual("X2", data[1][1]);
            Assert.AreEqual("Test Job 2", data[1][2]);
            Assert.AreEqual(DateTime.Now.AddDays(-28).Date, DateTime.Parse(data[1][3]));
            Assert.AreEqual(DateTime.Now.Date, DateTime.Parse(data[1][4]));
            Assert.AreEqual("Keith Harris", data[1][5]);
        }

        [TestMethod]
        public void Test_that_the_details_view_is_returned()
        {
            // Arrange
            Job job = GetJob();

            var jobs = new MemoryRepository<Job>();
            jobs.Add(job).Wait();

            repository.Setup(f => f.Find(1)).Returns(jobs.Find(job.JobId));

            // Act
            var result = controller.Details(1).Result as ViewResult;

            // Assert
            var model = result.ViewData.Model as Job;

            Assert.AreEqual(job, model);
            Assert.AreEqual("Details", result.ViewName);
        }

        [TestMethod]
        public void Test_creating_a_job_and_that_it_is_persisted()
        {
            // Arrange
            Job job = GetJob();

            // Act
            var result = controller.Create(job).Result as RedirectToRouteResult;

            // Assert
            repository.Verify(f => f.Add(job), Times.Once);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Test_creating_a_job_with_an_invalid_model_and_check_it_is_not_persisted()
        {
            // Arrange
            Job job = new Job();
            controller.ModelState.AddModelError("key", "error");

            // Act
            var result = controller.Create(job).Result as ViewResult;

            // Assert
            repository.Verify(f => f.Add(job), Times.Never);

            var model = result.ViewData.Model as Job;

            Assert.AreEqual(job, model);
            Assert.AreEqual("Create", result.ViewName);
        }

        [TestMethod]
        public void Test_editing_a_job_and_that_it_is_persisted()
        {
            // Arrange
            Job job = GetJob();

            // Act
            var result = controller.Edit(job).Result as RedirectToRouteResult;

            // Assert
            repository.Verify(f => f.Update(job), Times.Once);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Test_editing_a_job_with_an_invalid_model_and_that_it_is_not_persisted()
        {
            // Arrange
            Job job = new Job();
            controller.ModelState.AddModelError("key", "error");

            // Act
            var result = controller.Edit(job).Result as ViewResult;

            // Assert
            repository.Verify(f => f.Update(job), Times.Never);

            var model = result.ViewData.Model as Job;

            Assert.AreEqual(job, model);
            Assert.AreEqual("Edit", result.ViewName);
        }

        [TestMethod]
        public void Test_deleting_a_job_and_that_it_is_removed()
        {
            // Arrange
            Job job = GetJob();

            var jobs = new MemoryRepository<Job>();
            jobs.Add(job).Wait();

            repository.Setup(f => f.Remove(1)).Returns(jobs.Remove(job.JobId));

            // Act
            var result = controller.DeleteConfirmed(job.JobId).Result as RedirectToRouteResult;

            // Assert
            repository.Verify(f => f.Remove(job.JobId), Times.Once);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
    }
}