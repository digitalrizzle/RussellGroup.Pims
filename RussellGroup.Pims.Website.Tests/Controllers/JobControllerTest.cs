using DataTables.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Controllers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        public void Test_ModelState_validation_for_required_fields()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();
            job.XJobId = null;
            job.Description = null;

            var controller = new ModelStateTestController();

            // act
            var result = controller.TestTryValidateModel(job);

            // assert
            var modelState = controller.ModelState;

            Assert.IsFalse(result);
            Assert.AreEqual(2, modelState.Keys.Count);

            Assert.IsTrue(modelState.Keys.Contains("XJobId"));
            Assert.IsTrue(modelState["XJobId"].Errors.Count == 1);
            Assert.AreEqual("The id field is required.", modelState["XJobId"].Errors[0].ErrorMessage);

            Assert.IsTrue(modelState.Keys.Contains("Description"));
            Assert.IsTrue(modelState["Description"].Errors.Count == 1);
            Assert.AreEqual("The description field is required.", modelState["Description"].Errors[0].ErrorMessage);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_ModelState_validation_for_regex_fields()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();
            job.XJobId = "invalid job id";
            job.NotificationEmail = "invalid notification email";

            var controller = new ModelStateTestController();

            // act
            var result = controller.TestTryValidateModel(job);

            // assert
            var modelState = controller.ModelState;

            Assert.IsFalse(result);
            Assert.AreEqual(2, modelState.Keys.Count);

            Assert.IsTrue(modelState.Keys.Contains("XJobId"));
            Assert.IsTrue(modelState["XJobId"].Errors.Count == 1);
            Assert.AreEqual("The job id must begin with two alphabetic characters and end with four digits.", modelState["XJobId"].Errors[0].ErrorMessage);

            Assert.IsTrue(modelState.Keys.Contains("NotificationEmail"));
            Assert.IsTrue(modelState["NotificationEmail"].Errors.Count == 1);
            Assert.AreEqual("The email address(es) are not valid.", modelState["NotificationEmail"].Errors[0].ErrorMessage);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_the_Index_view_is_returned()
        {
            // act
            var result = controller.Index() as ViewResult;

            // assert
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_the_index_data_is_returned()
        {
            // arrange
            var jobs = TestDataFactory.GetJobs();
            repository.Setup(f => f.GetAll()).Returns(jobs);
            //controller.SetFakeAuthenticatedControllerContext(TestDataFactory.UserName, "News", "Index", new { id = 1 });

            var columns = new[]
            {
                new Column(string.Empty, "Id", false, false, string.Empty, false),
                new Column(string.Empty, "XJobId", true, true, string.Empty, false),
                new Column(string.Empty, "Description", true, true, string.Empty, false),
                new Column(string.Empty, "WhenStarted", true, true, string.Empty, false),
                new Column(string.Empty, "WhenEnded", true, true, string.Empty, false),
                new Column(string.Empty, "ProjectManager", true, true, string.Empty, false),
                new Column(string.Empty, "CrudLinks", false, false, string.Empty, false)
            };

            columns[1].SetColumnOrdering(1, "desc");

            var parameters = new DefaultDataTablesRequest()
            {
                Draw = 1,
                Start = 0,
                Length = 10,
                Search = null,
                Columns = new ColumnCollection(columns)
            };

            // act
            var result = controller.GetDataTableResult(parameters);
            var data = result.GetJqueryDataTableData();

            // assert
            Assert.AreEqual(3, data.Count());

            Assert.AreEqual("1", data[0][0]);
            Assert.AreEqual("DC0001", data[0][1]);
            Assert.AreEqual("First Test Job", data[0][2]);
            Assert.AreEqual("1/12/2015", data[0][3]);
            Assert.AreEqual("31/12/2015", data[0][4]);
            Assert.AreEqual("Orlando Hubbard", data[0][5]);

            Assert.AreEqual("2", data[1][0]);
            Assert.AreEqual("DC0002", data[1][1]);
            Assert.AreEqual("Second Test Job", data[1][2]);
            Assert.AreEqual("1/03/2016", data[1][3]);
            Assert.AreEqual(string.Empty, data[1][4]);
            Assert.AreEqual("Peter Parker", data[1][5]);

            Assert.AreEqual("3", data[2][0]);
            Assert.AreEqual("XX9999", data[2][1]);
            Assert.AreEqual("Random Test Job", data[2][2]);
            Assert.AreEqual("1/01/2031", data[2][3]);
            Assert.AreEqual("31/12/2031", data[2][4]);
            Assert.AreEqual("Carlee Walquist", data[2][5]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_details_view_is_returned_for_a_valid_job()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.FindAsync(job.Id)).Returns(Task.FromResult(job));

            // act
            var result = await controller.Details(job.Id) as ViewResult;

            // assert
            Assert.AreEqual("Details", result.ViewName);

            var model = result.Model as Job;

            Assert.IsNotNull(job);
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("DC0001", model.XJobId);
            Assert.AreEqual("First Test Job", model.Description);
            Assert.AreEqual(new DateTime(2015, 12, 1), model.WhenStarted);
            Assert.AreEqual(new DateTime(2015, 12, 31), model.WhenEnded);
            Assert.AreEqual("Orlando Hubbard", model.ProjectManager);
            Assert.AreEqual("Jarrod Koonce", model.QuantitySurveyor);
            Assert.AreEqual("orlando.hubbard@unittest.com", model.NotificationEmail);
            Assert.AreEqual("This is a comment.", model.Comment);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_details_view_returns_a_bad_request_status_code_for_a_null_id()
        {
            // arrange                        

            // act
            var result = await controller.Details(null) as HttpStatusCodeResult;

            // assert            
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_the_create_view_is_returned()
        {
            // arrange

            // act
            var result = controller.Create() as ViewResult;

            // assert
            Assert.AreEqual("Create", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_creating_a_valid_job()
        {
            // arrange
            Job model = null;
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.AddAsync(job)).Returns(Task.FromResult(1)).Callback((Job x) => model = x);
            repository.Setup(f => f.GetAll()).Returns(new Job[0].AsQueryable());

            // act
            controller.BindModel(job, "Create");
            var result = await controller.Create(job) as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "The result is not a RedirectToRouteResult.");
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            Assert.IsNotNull(job);
            //Assert.AreEqual(1, model.Id); // does not need to be checked on create
            Assert.AreEqual("DC0001", model.XJobId);
            Assert.AreEqual("First Test Job", model.Description);
            Assert.AreEqual(new DateTime(2015, 12, 1), model.WhenStarted);
            Assert.AreEqual(new DateTime(2015, 12, 31), model.WhenEnded);
            Assert.AreEqual("Orlando Hubbard", model.ProjectManager);
            Assert.AreEqual("Jarrod Koonce", model.QuantitySurveyor);
            Assert.AreEqual("orlando.hubbard@unittest.com", model.NotificationEmail);
            Assert.AreEqual("This is a comment.", model.Comment);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_a_job_with_an_XJobId_that_already_exists_cannot_be_created()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.AddAsync(job)).Returns(Task.FromResult(1));
            repository.Setup(f => f.GetAll()).Returns(TestDataFactory.GetJobs());

            // act
            controller.BindModel(job, "Create");
            var result = await controller.Create(job) as ViewResult;

            // assert
            Assert.IsNotNull(result, "The result is not a ViewResult.");
            Assert.AreEqual("The id already exists.", result.GetErrorMessage());
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_edit_view_is_returned()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.FindAsync(job.Id)).Returns(Task.FromResult(job));

            // act
            var result = await controller.Edit(job.Id) as ViewResult;

            // assert
            Assert.AreEqual("Edit", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_for_edit_view_that_a_bad_request_status_code_is_returned_for_null_id()
        {
            // arrange            

            // act
            var result = await controller.Edit(null) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_for_edit_view_that_a_not_found_status_code_is_returned_for_non_existant_job()
        {
            // arrange            
            repository.Setup(f => f.FindAsync(0)).Returns(Task.FromResult((Job)null));

            // act
            var result = await controller.Edit(0) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_editing_a_valid_job()
        {
            // arrange
            Job model = null;
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.FindAsync(job.Id)).Returns(Task.FromResult(job));
            repository.Setup(f => f.GetAll()).Returns(TestDataFactory.GetJobs());

            repository
                .Setup(f => f.UpdateAsync(job)).Returns(Task.FromResult(1))
                .Callback((Job x) => model = x);

            var collection = new FormCollection()
            {
                { "Id", "0" },  // this should not change
                { "XJobId", "DC1001" },
                { "Description", "Edited First Test Job" },
                { "WhenStarted", "16/9/2015" },
                { "WhenEnded","1/10/2016" },
                { "ProjectManager", "Dominic Morrison" },
                { "QuantitySurveyor", "Belinda Hunt" },
                { "NotificationEmail", "dominic.morrison@unittest.com,jarrodkoonce@unittest.com" },
                { "Comment", "This is an edited comment." }
            };

            controller.ValueProvider = collection.ToValueProvider();

            // act
            var result = await controller.Edit(1, "Save", collection) as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "The result is not a RedirectToRouteResult.");
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("DC1001", model.XJobId);
            Assert.AreEqual("Edited First Test Job", model.Description);
            Assert.AreEqual(new DateTime(2015, 9, 16), model.WhenStarted);
            Assert.AreEqual(new DateTime(2016, 10, 1), model.WhenEnded);
            Assert.AreEqual("Dominic Morrison", model.ProjectManager);
            Assert.AreEqual("Belinda Hunt", model.QuantitySurveyor);
            Assert.AreEqual("dominic.morrison@unittest.com,jarrodkoonce@unittest.com", model.NotificationEmail);
            Assert.AreEqual("This is an edited comment.", model.Comment);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_a_job_with_an_XJobId_that_already_exists_cannot_be_edited()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.UpdateAsync(job)).Returns(Task.FromResult(1));
            repository.Setup(f => f.FindAsync(1)).Returns(Task.FromResult(job));
            repository.Setup(f => f.GetAll()).Returns(TestDataFactory.GetJobs());

            var collection = new FormCollection()
            {
                { "Id", "0" },  // this should not change
                { "XJobId", "DC0002" }, // this is our duplicate
                { "Description", "Edited First Test Job" },
                { "WhenStarted", "16/9/2015" },
                { "WhenEnded","1/10/2016" },
                { "ProjectManager", "Dominic Morrison" },
                { "QuantitySurveyor", "Belinda Hunt" },
                { "NotificationEmail", "dominic.morrison@unittest.com,jarrodkoonce@unittest.com" },
                { "Comment", "This is an edited comment." }
            };

            controller.ValueProvider = collection.ToValueProvider();

            // act
            controller.BindModel(job, "Edit");
            var result = await controller.Edit(1, "Save", collection) as ViewResult;

            // assert
            Assert.IsNotNull(result, "The result is not a ViewResult.");
            Assert.AreEqual("The id already exists.", result.GetErrorMessage());
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_delete_view_is_returned()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.FindAsync(job.Id)).Returns(Task.FromResult(job));

            // act
            var result = await controller.Delete(job.Id) as ViewResult;

            // assert
            Assert.AreEqual("Delete", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_for_delete_view_that_a_bad_request_status_code_is_returned_for_null_id()
        {
            // arrange            

            // act
            var result = await controller.Delete(null) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_for_delete_view_that_a_not_found_status_code_is_returned_for_non_existant_job()
        {
            // arrange            
            repository.Setup(f => f.FindAsync(0)).Returns(Task.FromResult((Job)null));

            // act
            var result = await controller.Delete(0) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_deleting_a_valid_job()
        {
            // arrange
            var job = TestDataFactory.GetJobs().First();

            repository.Setup(f => f.FindAsync(job.Id)).Returns(Task.FromResult(job));
            repository.Setup(f => f.RemoveAsync(job)).Returns(Task.FromResult(1));

            // act
            var result = await controller.DeleteConfirmed(job.Id) as RedirectToRouteResult;

            // assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);
        }
    }
}