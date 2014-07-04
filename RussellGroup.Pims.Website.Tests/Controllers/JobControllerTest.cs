using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
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
        private Mock<IJobRepository> repository;
        private JobController controller;

        #region Setup

        [TestInitialize]
        public void Initialize()
        {
            repository = new Mock<IJobRepository>();

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
                Id = id,
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
    }
}