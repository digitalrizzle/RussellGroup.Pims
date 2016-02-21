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
    public class PlantControllerTest
    {
        private Mock<IPlantRepository> repository;

        private PlantController controller;

        #region Setup

        [TestInitialize]
        public void Initialize()
        {
            repository = new Mock<IPlantRepository>(MockBehavior.Strict);

            repository.SetupGet(f => f.Categories).Returns(TestDataFactory.GetCategories());
            repository.SetupGet(f => f.Conditions).Returns(TestDataFactory.GetConditions());
            repository.SetupGet(f => f.Statuses).Returns(TestDataFactory.GetStatuses());

            controller = new PlantController(repository.Object);
            controller.SetFakeAuthenticatedControllerContext();
        }

        #endregion

        [TestMethod, TestCategory("Controllers")]
        public void Test_ModelState_validation_for_required_fields()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();
            plant.XPlantId = null;
            plant.Description = null;

            var controller = new ModelStateTestController();

            // act
            var result = controller.TestTryValidateModel(plant);

            // assert
            var modelState = controller.ModelState;

            Assert.IsFalse(result);
            Assert.AreEqual(2, modelState.Keys.Count);

            Assert.IsTrue(modelState.Keys.Contains("XPlantId"));
            Assert.IsTrue(modelState["XPlantId"].Errors.Count == 1);
            Assert.AreEqual("The id field is required.", modelState["XPlantId"].Errors[0].ErrorMessage);

            Assert.IsTrue(modelState.Keys.Contains("Description"));
            Assert.IsTrue(modelState["Description"].Errors.Count == 1);
            Assert.AreEqual("The description field is required.", modelState["Description"].Errors[0].ErrorMessage);
        }

        [TestMethod, TestCategory("Controllers")]
        public void Test_ModelState_validation_for_regex_fields()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();
            plant.XPlantNewId = "invalid plant id";

            var controller = new ModelStateTestController();

            // act
            var result = controller.TestTryValidateModel(plant);

            // assert
            var modelState = controller.ModelState;

            Assert.IsFalse(result);
            Assert.AreEqual(1, modelState.Keys.Count);

            Assert.IsTrue(modelState.Keys.Contains("XPlantNewId"));
            Assert.IsTrue(modelState["XPlantNewId"].Errors.Count == 1);
            Assert.AreEqual("The new id field must be five characters.", modelState["XPlantNewId"].Errors[0].ErrorMessage);
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
            var plants = TestDataFactory.GetPlants().Take(3);
            var plantHire = TestDataFactory.GetPlantHires();

            repository.Setup(f => f.GetAll()).Returns(plants);
            repository.Setup(f => f.GetPlantHire(1)).Returns(plantHire.Where(f => f.PlantId == 1));
            repository.Setup(f => f.GetPlantHire(2)).Returns(plantHire.Where(f => f.PlantId == 2));
            repository.Setup(f => f.GetPlantHire(3)).Returns(plantHire.Where(f => f.PlantId == 3));

            var columns = new[]
            {
                new Column(string.Empty, "Id", false, false, string.Empty, false),
                new Column(string.Empty, "XPlantId", true, true, string.Empty, false),
                new Column(string.Empty, "XPlantNewId", true, true, string.Empty, false),
                new Column(string.Empty, "Description", true, true, string.Empty, false),
                new Column(string.Empty, "Category", true, true, string.Empty, false),
                new Column(string.Empty, "Hire", false, false, string.Empty, false),
                new Column(string.Empty, "IsDisused", false, false, string.Empty, false),
                new Column("Status", "Status.Name", true, true, string.Empty, false),
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

            Assert.AreEqual("01001", data[0][0]);
            Assert.AreEqual("01001", data[0][1]);
            Assert.AreEqual("Safety Harness", data[0][2]);
            Assert.AreEqual("Safety Equipment", data[0][3]);
            Assert.AreEqual(string.Empty, data[0][4]);
            Assert.AreEqual("No", data[0][5]);
            Assert.AreEqual("Available", data[0][6]);

            Assert.AreEqual("02002", data[1][0]);
            Assert.AreEqual("02002", data[1][1]);
            Assert.AreEqual("Saw", data[1][2]);
            Assert.AreEqual("Saws", data[1][3]);
            Assert.IsTrue(data[1][4].Contains("1"));
            Assert.AreEqual("No", data[1][5]);
            Assert.AreEqual("Available", data[1][6]);

            Assert.AreEqual("03002", data[2][0]);
            Assert.AreEqual("03002", data[2][1]);
            Assert.AreEqual("Saw", data[2][2]);
            Assert.AreEqual("Saws", data[2][3]);
            Assert.IsTrue(data[1][4].Contains("1"));
            Assert.AreEqual("No", data[2][5]);
            Assert.AreEqual("Checked out", data[2][6]);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_details_view_is_returned_for_a_valid_plant()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();

            repository.Setup(f => f.FindAsync(plant.Id)).Returns(Task.FromResult(plant));

            // act
            var result = await controller.Details(plant.Id) as ViewResult;

            // assert
            Assert.AreEqual("Details", result.ViewName);

            var model = result.Model as Plant;

            Assert.IsNotNull(plant);
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("01001", model.XPlantId);
            Assert.AreEqual("01001", model.XPlantNewId);
            Assert.AreEqual("Safety Harness", model.Description);
            Assert.AreEqual(new DateTime(2010, 7, 31), model.WhenPurchased);
            Assert.IsNull(model.WhenDisused);
            Assert.AreEqual(2.35m, model.Rate);
            Assert.AreEqual(1500.00m, model.Cost);
            Assert.AreEqual("HARNESS01001", model.Serial);
            Assert.AreEqual(false, model.IsElectrical);
            Assert.AreEqual(false, model.IsTool);
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
        public async Task Test_creating_a_valid_plant()
        {
            // arrange
            Plant model = null;
            var plant = TestDataFactory.GetPlants().First();

            repository
                .Setup(f => f.AddAsync(plant)).Returns(Task.FromResult(1))
                .Callback((Plant x) => model = x);

            // act
            controller.BindModel(plant, "Create");
            var result = await controller.Create(plant) as RedirectToRouteResult;

            // assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            Assert.IsNotNull(plant);
            //Assert.AreEqual(1, model.Id); // does not need to be checked on create
            Assert.AreEqual("01001", model.XPlantId);
            Assert.AreEqual("01001", model.XPlantNewId);
            Assert.AreEqual("Safety Harness", model.Description);
            Assert.AreEqual(new DateTime(2010, 7, 31), model.WhenPurchased);
            Assert.IsNull(model.WhenDisused);
            Assert.AreEqual(2.35m, model.Rate);
            Assert.AreEqual(1500.00m, model.Cost);
            Assert.AreEqual("HARNESS01001", model.Serial);
            Assert.AreEqual(false, model.IsElectrical);
            Assert.AreEqual(false, model.IsTool);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_edit_view_is_returned()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();

            repository.Setup(f => f.FindAsync(plant.Id)).Returns(Task.FromResult(plant));

            // act
            var result = await controller.Edit(plant.Id) as ViewResult;

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
        public async Task Test_for_edit_view_that_a_not_found_status_code_is_returned_for_non_existant_plant()
        {
            // arrange            
            repository.Setup(f => f.FindAsync(0)).Returns(Task.FromResult((Plant)null));

            // act
            var result = await controller.Edit(0) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_editing_a_valid_plant()
        {
            // arrange
            Plant model = null;
            var plant = TestDataFactory.GetPlants().First();

            repository.Setup(f => f.FindAsync(plant.Id)).Returns(Task.FromResult(plant));

            repository
                .Setup(f => f.UpdateAsync(plant)).Returns(Task.FromResult(1))
                .Callback((Plant x) => model = x);

            var collection = new FormCollection()
            {
                { "Id", "0" },  // this should not change
                { "XPlantId", "99999" },
                { "XPlantNewId", "00000" },
                { "Description", "Edited Safety Harness" },
                { "WhenPurchased", "31/7/2010" },
                { "WhenDisused","1/8/2010" },
                { "Rate", "2.35" },
                { "Cost", "1500.00" },
                { "Serial", "X123456789" },
                { "FixedAssetCode", "DC999999" },
                { "IsElectrical", "true" },
                { "IsTool", "true" }
            };

            controller.ValueProvider = collection.ToValueProvider();

            // act
            var result = await controller.Edit(1, collection) as RedirectToRouteResult;

            // assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("99999", model.XPlantId);
            Assert.AreEqual("00000", model.XPlantNewId);
            Assert.AreEqual("Edited Safety Harness", model.Description);
            Assert.AreEqual(new DateTime(2010, 7, 31), model.WhenPurchased);
            Assert.AreEqual(new DateTime(2010, 8, 1), model.WhenDisused);
            Assert.AreEqual(2.35m, model.Rate);
            Assert.AreEqual(1500.00m, model.Cost);
            Assert.AreEqual("X123456789", model.Serial);
            Assert.AreEqual("DC999999", model.FixedAssetCode);
            Assert.AreEqual(true, model.IsElectrical);
            Assert.AreEqual(true, model.IsTool);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_the_delete_view_is_returned()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();

            repository.Setup(f => f.FindAsync(plant.Id)).Returns(Task.FromResult(plant));

            // act
            var result = await controller.Delete(plant.Id) as ViewResult;

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
        public async Task Test_for_delete_view_that_a_not_found_status_code_is_returned_for_non_existant_plant()
        {
            // arrange            
            repository.Setup(f => f.FindAsync(0)).Returns(Task.FromResult((Plant)null));

            // act
            var result = await controller.Delete(0) as HttpStatusCodeResult;

            // assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod, TestCategory("Controllers")]
        public async Task Test_deleting_a_valid_plant()
        {
            // arrange
            var plant = TestDataFactory.GetPlants().First();

            repository.Setup(f => f.FindAsync(plant.Id)).Returns(Task.FromResult(plant));
            repository.Setup(f => f.RemoveAsync(plant)).Returns(Task.FromResult(1));

            // act
            var result = await controller.DeleteConfirmed(plant.Id) as RedirectToRouteResult;

            // assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);
        }
    }
}