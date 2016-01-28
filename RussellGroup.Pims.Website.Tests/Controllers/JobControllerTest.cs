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
            repository = new Mock<IJobRepository>(MockBehavior.Strict);

            controller = new JobController(repository.Object);
            controller.SetFakeAuthenticatedControllerContext();
        }

        #endregion

        [TestMethod, TestCategory("Controllers")]
        public void Test_the_Index_view_is_returned()
        {
            // act
            var result = controller.Index() as ViewResult;

            // assert
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}