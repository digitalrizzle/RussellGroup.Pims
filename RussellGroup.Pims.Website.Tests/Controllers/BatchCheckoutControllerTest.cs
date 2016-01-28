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
    public class BatchCheckoutControllerTest
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
            var mockJobs = MockFactory.GetMockDbSet(TestDataFactory.GetJobs(hasPlantHires: false));
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

        #region Checkout

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkout_that_the_Checkout_view_is_returned()
        {
            // act
            var result = Controller.Checkout(null, null, null) as ViewResult;

            // assert
            Assert.AreEqual("Checkout", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkout_that_when_a_date_is_not_given_the_current_date_is_returned()
        {
            // act
            var result = Controller.Checkout(null, null, null) as ViewResult;

            // assert
            Assert.IsTrue(result.Model is BatchCheckout);

            var model = result.Model as BatchCheckout;

            Assert.AreEqual(DateTime.Now.Date, model.WhenStarted);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_Checkout_that_the_parameters_passed_are_returned_in_the_view()
        {
            // act
            var result = Controller.Checkout("31/12/2015", "XX9999", 9) as ViewResult;

            // assert
            Assert.IsTrue(result.Model is BatchCheckout);

            var model = result.Model as BatchCheckout;

            Assert.AreEqual(new DateTime(2015, 12, 31), model.WhenStarted);
            Assert.AreEqual("XX9999", model.Scans);
            Assert.AreEqual(9, model.ReceiptId);
        }

        #endregion

        #region ConfirmCheckout

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_the_ConfirmCheckout_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;

            // assert
            Assert.AreEqual("ConfirmCheckout", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_there_are_no_scans()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = null;

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsNull(transaction, "The transaction is incorrectly not null.");
            Assert.AreEqual("Nothing was scanned to checkout.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_populates_the_model_as_expected()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var plants = transaction.Plants.ToArray();

            // assert
            Assert.IsFalse(transaction.Job.IsError, "The job is incorrectly in error.");
            Assert.IsFalse(plants[0].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[1].IsError, "The plant is incorrectly in error.");
            Assert.IsFalse(plants[2].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("DC0001", transaction.Job.XJobId);
            Assert.AreEqual("01001", plants[0].XPlantId);
            Assert.AreEqual("02002", plants[1].XPlantId);
            Assert.AreEqual("04003", plants[2].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_a_job_scan_does_not_exist()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"AA0001{Environment.NewLine}01001{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("The job AA0001 could not be found.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_a_job_is_not_the_first_scan()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"01001{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("A job must be scanned first.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_if_the_same_job_is_added_twice()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}01001{Environment.NewLine}DC0001{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("The job DC0001 has been added more than once.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_if_no_plant_is_scanned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("The job DC0001 has no plant to checkout.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_only_the_last_five_digits_of_a_plant_scan_are_used()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var plants = result.GetPlantsOfFirstBatchCheckoutTransaction();

            // assert
            Assert.IsFalse(plants[2].IsError, "The plant is incorrectly in error.");
            Assert.AreEqual("04003", plants[2].XPlantId);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_a_plant_scan_does_not_exist()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}99999{Environment.NewLine}02002";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var plants = result.GetPlantsOfFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 99999 could not be found.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_a_plant_scan_is_not_available()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}03002";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var plants = result.GetPlantsOfFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 03002 is not available.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_a_plant_is_added_twice()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}01001{Environment.NewLine}01001";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var plants = result.GetPlantsOfFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(plants[0].IsError, "The plant is not in error.");
            Assert.IsTrue(plants[1].IsError, "The plant is not in error.");
            Assert.AreEqual("The plant 01001 has been added more than once.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_an_error_is_found_when_there_was_no_plant_scanned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}";

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();
            var error = result.GetFirstErrorMessage();

            // assert
            Assert.IsTrue(transaction.Job.IsError, "The job is not in error.");
            Assert.AreEqual("The job DC0001 has no plant to checkout.", error);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_ConfirmCheckout_that_that_a_docket_number_is_prefixed_with_DCL_and_increments_from_the_last_issued()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.ConfirmCheckout(batch) as ViewResult;
            var transaction = result.GetFirstBatchCheckoutTransaction();

            // assert
            Assert.IsFalse(transaction.Job.IsError, "The job is incorrectly in error.");
            Assert.IsTrue(transaction.Docket.Equals("DCL900002"));
        }

        #endregion

        #region CommitCheckout

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_a_bad_request_is_returned_when_called_with_no_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.CommitCheckout(batch, null) as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_a_bad_request_is_returned_when_called_with_an_unknown_command()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.CommitCheckout(batch, "unknown command") as HttpStatusCodeResult;

            // assert
            Assert.IsNotNull(result, "Not an HTTP status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_successful_commit_a_redirect_to_Checkout_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            Repository.
                Setup(f => f.Checkout(
                    It.IsAny<Job>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .ReturnsAsync(1L);

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt receipt) =>
            {
                receipt.Id = 7;
                return Task.FromResult(receipt);
            });

            // act
            var result = await Controller.CommitCheckout(batch, "Checkout") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("Checkout", result.RouteValues["Action"]);
            Assert.AreEqual("31/12/2015", result.RouteValues["whenStarted"]);
            Assert.AreEqual(7, result.RouteValues["receiptId"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_Retry_a_redirect_to_Checkout_occurs()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();

            // act
            var result = await Controller.CommitCheckout(batch, "Retry") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(result, "Not a redirect result");
            Assert.IsFalse(result.Permanent);
            Assert.AreEqual("Checkout", result.RouteValues["Action"]);
            Assert.AreEqual("31/12/2015", result.RouteValues["whenStarted"]);
            Assert.AreEqual(batch.Scans, result.RouteValues["scans"]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_a_model_validation_failure_the_ConfirmCheckout_view_is_returned()
        {
            // arrange
            var batch = TestDataFactory.GetBatchCheckout();
            batch.Scans = $"DC0001{Environment.NewLine}";

            // act
            var result = await Controller.CommitCheckout(batch, "Checkout") as ViewResult;

            // assert
            Assert.AreEqual("ConfirmCheckout", result.ViewName);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_successful_commit_a_receipt_is_created()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckout();

            Repository.
                Setup(f => f.Checkout(
                    It.IsAny<Job>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .ReturnsAsync(1L);

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                r.Id = 9;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckout(batch, "Checkout") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.AreEqual(9, receipt.Id);
            Assert.AreEqual(TransactionType.Checkout, receipt.TransactionTypeId);
            Assert.AreEqual("Tester", receipt.UserName);
            Assert.AreEqual("application/pdf", receipt.ContentType);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_successful_commit_the_docket_numbers_are_prefixed_with_DCL()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckout();

            Repository.
                Setup(f => f.Checkout(
                    It.IsAny<Job>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .ReturnsAsync(1L);

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckout(batch, "Checkout") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.IsTrue(receipt.Dockets.Split(',').All(f => f.StartsWith("DCL")), "The dockets are not prefixed with DCL.");
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_CommitCheckout_that_on_successful_commit_a_PDF_is_created()
        {
            // arrange
            Receipt receipt = null;
            var batch = TestDataFactory.GetBatchCheckout();

            Repository.
                Setup(f => f.Checkout(
                    It.IsAny<Job>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<KeyValuePair<int, int?>>>()))
                .ReturnsAsync(1L);

            Repository.Setup(f => f.StoreAsync(It.IsAny<Receipt>())).Returns((Receipt r) =>
            {
                receipt = r;
                return Task.FromResult(r);
            });

            // act
            var result = await Controller.CommitCheckout(batch, "Checkout") as RedirectToRouteResult;

            // assert
            Assert.IsNotNull(receipt, "A receipt was not created.");
            Assert.IsNotNull(receipt.Content.Hash, "A hash of the content was not set.");
            Assert.IsNotNull(receipt.Content.Data, "Data of the content was not set.");
        }

        #endregion
    }
}