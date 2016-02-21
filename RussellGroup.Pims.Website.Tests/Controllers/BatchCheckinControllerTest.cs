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
    public class BatchCheckinControllerTest
    {
        private Mock<ITransactionRepository> Repository;
        private BatchController Controller;

        [TestInitialize]
        public void Initialize()
        {
            Repository = new Mock<ITransactionRepository>(MockBehavior.Strict);

            var mockTransactionTypes = MockFactory.GetMockDbSet(TestDataFactory.GetTransactionTypes());
            var mockStatuses = MockFactory.GetMockDbSet(TestDataFactory.GetStatuses());
            var mockConditions = MockFactory.GetMockDbSet(TestDataFactory.GetConditions());
            var mockPlants = MockFactory.GetMockDbSet(TestDataFactory.GetPlants());
            var mockInventories = MockFactory.GetMockDbSet(new Inventory[0].AsQueryable());
            var mockJobs = MockFactory.GetMockDbSet(TestDataFactory.GetJobs(hasPlantHires: true));
            var mockPlantHire = MockFactory.GetMockDbSet(TestDataFactory.GetPlantHires());
            var mockInventoryHire = MockFactory.GetMockDbSet(new InventoryHire[0].AsQueryable());

            Repository.Setup(f => f.GetLastIssuedDocketAsync()).Returns(Task.FromResult(900001L));
            Repository.Setup(f => f.TransactionTypes).Returns(mockTransactionTypes.Object);
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

        #region Checkin

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkin_that_the_Checkin_view_is_returned()
        {
            // act
            var result = Controller.Checkin(null, null, null) as ViewResult;

            // assert
            Assert.AreEqual("Checkin", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkin_that_when_a_date_is_not_given_the_current_date_is_returned()
        {
            // act
            var result = Controller.Checkin(null, null, null) as ViewResult;

            // assert
            Assert.IsTrue(result.Model is BatchCheckin);

            var model = result.Model as BatchCheckin;

            Assert.AreEqual(DateTime.Now.Date, model.WhenEnded);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkin_that_the_parameters_passed_are_returned_in_the_view()
        {
            // act
            var result = Controller.Checkin("31/10/2017", "XX9999", true) as ViewResult;

            // assert
            Assert.IsTrue(result.Model is BatchCheckin);

            var model = result.Model as BatchCheckin;

            Assert.AreEqual(new DateTime(2017, 10, 31), model.WhenEnded);
            Assert.AreEqual("XX9999", model.Scans);
            Assert.AreEqual("Receipts have been created.", result.ViewBag.Message);
        }

        #endregion

        #region ConfirmCheckin

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_the_ConfirmCheckin_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;

            // assert
            Assert.AreEqual("ConfirmCheckin", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_there_are_no_scans()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = null;

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var model = result.GetBatchCheckinModel();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsFalse(model.CheckinTransactions.Any(), "The transactions are not empty.");
            Assert.AreEqual("Nothing was scanned to check in.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_populates_the_model_as_expected()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction1 = result.GetBatchCheckinTransaction();
            var plants1 = result.GetPlantsOfBatchCheckinTransaction();
            var transaction2 = result.GetBatchCheckinTransaction(1);
            var plants2 = result.GetPlantsOfBatchCheckinTransaction(1);

            // assert
            Assert.IsFalse(transaction1.Job.IsError, "The job is incorrectly in error.");
            Assert.IsFalse(transaction1.IsAutoDocket, "The auto docket is incorrectly true.");
            Assert.IsFalse(plants1[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants1[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0001", transaction1.Job.XJobId);
            Assert.AreEqual("100001", transaction1.ReturnDocket);
            Assert.AreEqual("03002", plants1[0].XPlantId);
            Assert.AreEqual("CMPSSR1", plants1[1].XPlantId);

            Assert.IsFalse(transaction2.Job.IsError, "The job is incorrectly in error.");
            Assert.IsFalse(transaction2.IsAutoDocket, "The auto docket is incorrectly true.");
            Assert.IsFalse(plants2[0].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0002", transaction2.Job.XJobId);
            Assert.AreEqual("100002", transaction2.ReturnDocket);
            Assert.AreEqual("07001", plants2[0].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_whitespace_of_the_scans_is_trimmed()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"{Environment.NewLine} DOCKET {Environment.NewLine} 100001 {Environment.NewLine} 99903002 {Environment.NewLine} 06003 {Environment.NewLine} DOCKET {Environment.NewLine} 100002 {Environment.NewLine} 12307001 {Environment.NewLine} ";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction1 = result.GetBatchCheckinTransaction();
            var plants1 = result.GetPlantsOfBatchCheckinTransaction();
            var transaction2 = result.GetBatchCheckinTransaction(1);
            var plants2 = result.GetPlantsOfBatchCheckinTransaction(1);

            // assert
            Assert.IsFalse(transaction1.Job.IsError, "The job is incorrectly in error.");
            Assert.IsFalse(transaction1.IsAutoDocket, "The auto docket is incorrectly true.");
            Assert.IsFalse(plants1[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants1[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0001", transaction1.Job.XJobId);
            Assert.AreEqual("100001", transaction1.ReturnDocket);
            Assert.AreEqual("03002", plants1[0].XPlantId);
            Assert.AreEqual("CMPSSR1", plants1[1].XPlantId);

            Assert.IsFalse(transaction2.Job.IsError, "The job is incorrectly in error.");
            Assert.IsFalse(transaction2.IsAutoDocket, "The auto docket is incorrectly true.");
            Assert.IsFalse(plants2[0].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0002", transaction2.Job.XJobId);
            Assert.AreEqual("100002", transaction2.ReturnDocket);
            Assert.AreEqual("07001", plants2[0].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_two_transactions_cannot_have_the_same_docket_number()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}100001{Environment.NewLine}99903002{Environment.NewLine}06003{Environment.NewLine}12307001";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction1 = result.GetBatchCheckinTransaction();
            var transaction2 = result.GetBatchCheckinTransaction(1);
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(transaction2.ReturnDocket, transaction1.ReturnDocket, "The docket numbers were expected to be the same.");
            Assert.AreEqual("The docket numbers cannot be the same between different jobs.", error);
            Assert.IsTrue(transaction1.Job.IsError, "The job is not in error.");
            Assert.IsTrue(transaction2.Job.IsError, "The job is not in error.");
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_two_transactions_cannot_have_the_same_auto_docket_number()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"AUTO DOCKET{Environment.NewLine}99903002{Environment.NewLine}06003{Environment.NewLine}12307001";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction1 = result.GetBatchCheckinTransaction();
            var transaction2 = result.GetBatchCheckinTransaction(1);
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(transaction2.ReturnDocket, transaction1.ReturnDocket, "The docket numbers were expected to be the same.");
            Assert.AreEqual("The docket numbers cannot be the same between different jobs.", error);
            Assert.IsTrue(transaction1.Job.IsError, "The job is not in error.");
            Assert.IsTrue(transaction2.Job.IsError, "The job is not in error.");
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_only_the_last_five_digits_of_a_plant_scan_are_used()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchCheckinTransaction();

            // assert
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("06003", plants[1].XPlantNewId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_a_plant_scan_does_not_exist()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}99999";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchCheckinTransaction();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 99999 could not be found.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_a_plant_scan_is_not_checked_out()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchCheckinTransaction();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 02002 is not checked out.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_a_plant_is_added_twice()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}03002{Environment.NewLine}03002";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var plants = result.GetPlantsOfBatchCheckinTransaction();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.IsTrue(plants[1].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 03002 has been added more than once.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_there_was_no_plant_scanned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction = result.GetBatchCheckinTransaction();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("123456", transaction.ReturnDocket);
            Assert.AreEqual("The was no plant scanned to check in.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_auto_docket_number_is_prefixed_with_DCL_and_increments_from_the_last_issued()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"AUTO DOCKET{Environment.NewLine}03002{Environment.NewLine}99906003";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction = result.GetBatchCheckinTransaction();
            var plants = result.GetPlantsOfBatchCheckinTransaction();

            // assert
            Assert.AreEqual("DCL900002", transaction.ReturnDocket);

            Assert.IsFalse(transaction.Job.IsError, "The job is incorrectly in error.");
            Assert.IsTrue(transaction.IsAutoDocket, "The auto docket is incorrectly false.");
            Assert.IsFalse(plants[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0001", transaction.Job.XJobId);
            Assert.AreEqual("03002", plants[0].XPlantId);
            Assert.AreEqual("CMPSSR1", plants[1].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_auto_docket_number_can_be_issued_twice()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"AUTO DOCKET{Environment.NewLine}99903002{Environment.NewLine}06003{Environment.NewLine}AUTO DOCKET{Environment.NewLine}12307001";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction1 = result.GetBatchCheckinTransaction();
            var transaction2 = result.GetBatchCheckinTransaction(1);

            // assert
            Assert.AreEqual("DCL900002", transaction1.ReturnDocket);
            Assert.IsTrue(transaction1.IsAutoDocket, "The auto docket is incorrectly false.");

            Assert.AreEqual("DCL900003", transaction2.ReturnDocket);
            Assert.IsTrue(transaction2.IsAutoDocket, "The auto docket is incorrectly false.");
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_an_error_is_found_when_a_plant_is_scanned_without_without_a_docket()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"03002{Environment.NewLine}DOCKET{Environment.NewLine}";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var transaction = result.GetBatchCheckinTransaction();
            var plants = result.GetPlantsOfBatchCheckinTransaction();
            var error = result.GetErrorMessage();

            // assert
            Assert.IsFalse(transaction.IsAutoDocket, "The auto docket is incorrectly true.");
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("There is no docket for the plant 03002.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_the_default_status_is_under_repair()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var model = result.Model as BatchCheckin;

            // assert
            Assert.AreEqual(Status.UnderRepair, model.StatusId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_the_status_can_be_changed()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}03002{Environment.NewLine}STOLEN";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var model = result.Model as BatchCheckin;

            // assert
            Assert.AreEqual(Status.Stolen, model.StatusId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmStatusUpdate_that_the_status_cannot_be_changed_to_checked_out()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}03002{Environment.NewLine}CHECKED OUT";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var model = result.Model as BatchCheckin;
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(Status.CheckedOut, model.StatusId);
            Assert.AreEqual("The status cannot be changed to checked out.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckin_that_a_status_can_only_be_set_once()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"STOLEN{Environment.NewLine}DOCKET{Environment.NewLine}123456{Environment.NewLine}03002{Environment.NewLine}STOLEN";

            // act
            var result = await Controller.ConfirmCheckin(batch) as ViewResult;
            var error = result.GetErrorMessage();

            // assert
            Assert.AreEqual(error, "The status can only be set once.");
        }

        #endregion

        #region CommitCheckin

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_a_bad_request_is_returned_when_called_with_no_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.CommitCheckin(batch, null) as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_a_bad_request_is_returned_when_called_with_an_unknown_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.CommitCheckin(batch, "unknown command") as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_successful_commit_a_redirect_to_Checkin_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            Repository.
                Setup(f => f.Checkin(
                    It.IsAny<Job>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .Returns(Task.FromResult(0));

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt receipt) =>
            {
                return Task.FromResult(receipt);
            });

            // act
            var result = await Controller.CommitCheckin(batch, "Checkin") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("Checkin", result.RouteValues["Action"]);
            Assert.AreEqual("31/10/2017", result.RouteValues["whenEnded"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_Retry_a_redirect_to_Checkin_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();

            // act
            var result = await Controller.CommitCheckin(batch, "Retry") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("Checkin", result.RouteValues["Action"]);
            Assert.AreEqual("31/10/2017", result.RouteValues["whenEnded"]);
            Assert.AreEqual(batch.Scans, result.RouteValues["scans"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_a_model_validation_failure_the_ConfirmCheckin_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"DC0001{Environment.NewLine}";

            // act
            var result = await Controller.CommitCheckin(batch, "Checkin") as ViewResult;

            // assert
            Assert.AreEqual("ConfirmCheckin", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_successful_commit_a_receipt_is_created()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckin();

            Repository.
                Setup(f => f.Checkin(
                    It.IsAny<Job>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .Returns(Task.FromResult(0));

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                r.Id = 9;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckin(batch, "Checkin") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.AreEqual(9, receipt.Id);
            Assert.AreEqual(TransactionType.Checkin, receipt.TransactionTypeId);
            Assert.AreEqual("Tester", receipt.UserName);
            Assert.AreEqual("application/pdf", receipt.Content.ContentType);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_successful_commit_the_auto_docket_numbers_are_prefixed_with_DCL()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckin();
            batch.Scans = $"AUTO DOCKET{Environment.NewLine}03002{Environment.NewLine}99906003";

            Repository.
                Setup(f => f.Checkin(
                    It.IsAny<Job>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .ReturnsAsync(1L);

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckin(batch, "Checkin") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.IsTrue(receipt.Docket.StartsWith("DCL"), "The docket is not prefixed with DCL.");
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckin_that_on_successful_commit_a_PDF_is_created()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckin();

            Repository.
                Setup(f => f.Checkin(
                    It.IsAny<Job>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .Returns(Task.FromResult(0));

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckin(batch, "Checkin") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.IsNotNull(receipt.Content.Hash, "A hash of the content was not set.");
            Assert.IsNotNull(receipt.Content.Data, "Data of the content was not set.");
        }

        #endregion
    }
}