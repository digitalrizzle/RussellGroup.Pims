using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Controllers;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Tests.Controllers
{
    [TestClass]
    public class BatchStatusControllerTest
    {
        private Mock<ITransactionRepository> Repository;
        private BatchController Controller;

        [TestInitialize]
        public void Initialize()
        {
            Repository = new Mock<ITransactionRepository>(MockBehavior.Strict);

            var mockStatuses = MockFactory.GetMockDbSet(TestDataFactory.GetStatuses());
            var mockConditions = MockFactory.GetMockDbSet(TestDataFactory.GetConditions());
            var mockPlants = MockFactory.GetMockDbSet(TestDataFactory.GetPlants());
            var mockInventories = MockFactory.GetMockDbSet(new Inventory[0].AsQueryable());
            var mockJobs = MockFactory.GetMockDbSet(TestDataFactory.GetJobs(hasPlantHires: true));
            var mockPlantHire = MockFactory.GetMockDbSet(TestDataFactory.GetPlantHires());
            var mockInventoryHire = MockFactory.GetMockDbSet(new InventoryHire[0].AsQueryable());

            Repository.Setup(f => f.GetLastIssuedDocketAsync()).Returns(Task.FromResult(900001L));
            Repository.Setup(f => f.Statuses).Returns(mockStatuses.Object);
            Repository.Setup(f => f.Conditions).Returns(mockConditions.Object);
            Repository.Setup(f => f.Plants).Returns(mockPlants.Object);
            Repository.Setup(f => f.Inventories).Returns(mockInventories.Object);
            Repository.Setup(f => f.Jobs).Returns(mockJobs.Object);
            Repository.Setup(f => f.PlantHires).Returns(mockPlantHire.Object);
            Repository.Setup(f => f.InventoryHires).Returns(mockInventoryHire.Object);

            Controller = new BatchController(Repository.Object);
            Controller.SetFakeAuthenticatedControllerContext();
        }

        #region StatusUpdate

        [TestMethod, TestCategory("Controllers")]
        public void Test_Status_that_the_Status_view_is_returned()
        {
            // act
            var result = Controller.StatusUpdate(null) as ViewResult;

            // assert
            Assert.AreEqual("Status", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_Status_that_the_parameters_passed_are_returned_in_the_view()
        {
            // act
            var batch = TestDataFactory.GetBatchStatus();
            var result = Controller.StatusUpdate(batch.Scans) as ViewResult;

            // assert
            Assert.IsTrue(result.Model is BatchStatus);

            var model = result.Model as BatchStatus;

            Assert.AreEqual($"WRITTEN OFF{Environment.NewLine}02002{Environment.NewLine}99904003", model.Scans);
        }

        #endregion

        #region ConfirmStatusUpdate

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_the_ConfirmStatus_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;

            // assert
            Assert.AreEqual("ConfirmStatus", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_an_error_is_found_when_there_are_no_scans()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = null;

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsFalse(model.Plants.Any(), "The plants are not empty.");
            Assert.AreEqual("Nothing was scanned to update the status.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_populates_the_model_as_expected()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var plants = result.GetPlantsOfBatchStatus();

            // assert
            Assert.AreEqual(Status.WrittenOff, model.StatusId);
            Assert.AreEqual(Status.WrittenOff, model.Status.Id);
            Assert.IsFalse(plants[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("02002", plants[0].XPlantId);
            Assert.AreEqual("04003", plants[1].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_whitespace_of_the_scans_is_trimmed()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $" {Environment.NewLine} WRITTEN OFF {Environment.NewLine} 02002 {Environment.NewLine} 99904003 {Environment.NewLine} ";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchStatus();

            // assert
            Assert.IsFalse(plants[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("02002", plants[0].XPlantId);
            Assert.AreEqual("04003", plants[1].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_only_the_last_five_digits_of_a_plant_scan_are_used()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"WRITTEN OFF{Environment.NewLine}123ABC!@#02002{Environment.NewLine}99904003";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchStatus();

            // assert
            Assert.IsFalse(plants[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("02002", plants[0].XPlantId);
            Assert.AreEqual("04003", plants[1].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_an_error_is_found_when_a_plant_scan_does_not_exist()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = "99999";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchStatus();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 99999 could not be found.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_an_error_is_found_when_a_plant_scan_is_checked_out()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = "03002";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchStatus();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 03002 is not available.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_an_error_is_found_when_a_plant_is_added_twice()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"02002{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchStatus();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.IsTrue(plants[1].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 02002 has been added more than once.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_an_error_is_found_when_there_was_no_plant_scanned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"STOLEN";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(Status.Stolen, model.StatusId);
            Assert.AreEqual(Status.Stolen, model.Status.Id);
            Assert.IsFalse(model.Plants.Any(), "The plants are not empty.");
            Assert.AreEqual("There was no plant scanned to update the status.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_a_status_must_be_scanned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"02002{Environment.NewLine}99904003";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual("There was no status scanned.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_the_status_can_be_changed()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"UNKNOWN{Environment.NewLine}02002{Environment.NewLine}99904003";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();

            // assert
            Assert.AreEqual(Status.Unknown, model.StatusId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_the_status_cannot_be_changed_to_available()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"AVAILABLE{Environment.NewLine}02002{Environment.NewLine}99904003";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(Status.Available, model.StatusId);
            Assert.AreEqual("The status cannot be changed to available.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_the_status_cannot_be_set_to_checked_out()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"CHECKED OUT{Environment.NewLine}02002{Environment.NewLine}99904003";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var model = result.GetBatchStatusModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(Status.CheckedOut, model.StatusId);
            Assert.AreEqual("The status cannot be changed to checked out.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_a_status_can_only_be_set_once()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"STOLEN{Environment.NewLine}02002{Environment.NewLine}99904003{Environment.NewLine}STOLEN";

            // act
            var result = await Controller.ConfirmStatusUpdate(batch) as ViewResult;
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(error, "The status can only be set once.");
        }

        #endregion

        #region CommitStatusUpdate

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitStatusUpdate_that_a_bad_request_is_returned_when_called_with_no_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            // act
            var result = await Controller.CommitStatusUpdate(batch, null) as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitStatusUpdate_a_bad_request_is_returned_when_called_with_an_unknown_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            // act
            var result = await Controller.CommitStatusUpdate(batch, "unknown command") as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitStatusUpdate_that_on_successful_commit_a_redirect_to_StatusUpdate_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            Repository.
                Setup(f => f.UpdateStatusAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(0));

            // act
            var result = await Controller.CommitStatusUpdate(batch, "Update Status") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("StatusUpdate", result.RouteValues["Action"]);
            Assert.IsNull(result.RouteValues["scans"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitStatusUpdate_that_on_Retry_a_redirect_to_ConfirmStatus_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();

            // act
            var result = await Controller.CommitStatusUpdate(batch, "Retry") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("StatusUpdate", result.RouteValues["Action"]);
            Assert.AreEqual(batch.Scans, result.RouteValues["scans"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitStatusUpdate_that_on_a_model_validation_failure_the_ConfirmStatus_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchStatus();
            batch.Scans = $"XXXXXX{Environment.NewLine}";

            // act
            var result = await Controller.CommitStatusUpdate(batch, "Update Status") as ViewResult;

            // assert
            Assert.AreEqual("ConfirmStatus", result.ViewName);
        }

        #endregion
    }
}