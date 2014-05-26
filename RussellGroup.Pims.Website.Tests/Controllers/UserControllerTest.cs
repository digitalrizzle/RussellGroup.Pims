﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using RussellGroup.Pims.Website.Controllers;
using RussellGroup.Pims.Website.Models;
using RussellGroup.Pims.Website.Tests;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NZPost.Votext3.Website.Tests.Controllers
{
    [TestClass]
    public class UserControllerTest
    {
        private Mock<IUserRepository> _mockRepository;
        private UserController _controller;

        #region Setup

        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            _controller = new UserController(_mockRepository.Object);
            _controller.SetFakeAuthenticatedControllerContext();
        }

        private IQueryable<ApplicationUser> GetAllUsers()
        {
            var allRoles = GetAllRoles();

            var users = new[]
            {
                GetUser("43b59d67-2096-4fd7-a711-01fb525ae03b", "BRETT-PC\\Brett", "brett.fisher@zoho.com", roles: allRoles),
                GetUser("ab547f90-7edd-4752-ad1c-48d3c26a400c", "constructors\\jassen", "", roles: allRoles),
                GetUser("9a50e2d1-3e6b-49f8-9763-52705a4abebe", "RUSPDB\\PDBDev", "", true, new DateTime(1999, 12, 31), roles: allRoles.Where(f => f.Name == Role.CanView))
            };

            return users.AsQueryable();
        }

        private ApplicationUser GetFirstUser()
        {
            return GetAllUsers().FirstOrDefault();
        }

        private UserRolesViewModel GetFirstUserUserRolesViewModel()
        {
            var user = GetFirstUser();

            var roles = GetAllRoles().ToList();
            roles.Single(f => f.Name == Role.CanEdit).IsChecked = true;

            var model = new UserRolesViewModel()
            {
                User = user,
                Roles = roles
            };

            return model;
        }

        private ApplicationUser GetUser(string id, string userName, string email, bool lockoutEnabled = false, DateTime? lockoutEndDateUtc = null, IEnumerable<ApplicationRole> roles = null)
        {
            if (lockoutEnabled && !lockoutEndDateUtc.HasValue)
            {
                throw new ArgumentNullException("If LockoutEnabled is enabled, then LockoutEndDateUtc must also be given.");
            }

            var user = new ApplicationUser
            {
                Id = id,
                UserName = userName,
                Email = email,
                LockoutEnabled = lockoutEnabled,
                LockoutEndDateUtc = lockoutEndDateUtc
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    var userRole = new IdentityUserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    };

                    user.Roles.Add(userRole);
                    role.Users.Add(userRole);
                }
            }

            return user;
        }

        private IQueryable<ApplicationRole> GetAllRoles()
        {
            var roles = new List<ApplicationRole>
            {
                GetRole("f5437545-d921-4f6b-88e4-04de8de58778", Role.CanEdit),
                GetRole("fdfc1500-f9e4-44ad-96f5-965e5dacc3cf", Role.CanEditCategories),
                GetRole("f2603874-4d32-4186-85c0-18030d84fbda", Role.CanEditUsers),
                GetRole("98a92488-06cd-4403-aaa7-c02d57ad9233", Role.CanView)
            };

            return roles.AsQueryable();
        }

        private ApplicationRole GetFirstRole()
        {
            return GetAllRoles().FirstOrDefault();
        }

        private static ApplicationRole GetRole(string id, string name, string description = null)
        {
            return new ApplicationRole()
            {
                Id = id,
                Name = name,
                Description = description
            };
        }

        #endregion

        [TestMethod]
        public void Test_that_the_index_view_is_returned()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void Test_that_the_index_data_is_returned()
        {
            // Arrange
            _mockRepository.Setup(f => f.GetAll()).Returns(GetAllUsers);

            var parameters = new JqueryDataTableParameterModel()
            {
                iSortCol_0 = 1,
                sSortDir_0 = "asc",
                iDisplayLength = 10,
                iDisplayStart = 0,
                sSearch = string.Empty
            };

            // Act
            var result = _controller.GetDataTableResult(parameters);
            var data = result.GetJqueryDataTableAaData();

            // Assert
            Assert.AreEqual(3, data.Count());

            Assert.AreEqual("43b59d67-2096-4fd7-a711-01fb525ae03b", data[0][0]);
            Assert.AreEqual("BRETT-PC\\Brett", data[0][1]);
            Assert.AreEqual("brett.fisher@zoho.com", data[0][2]);
            Assert.AreEqual("No", data[0][3]);

            Assert.AreEqual("ab547f90-7edd-4752-ad1c-48d3c26a400c", data[1][0]);
            Assert.AreEqual("constructors\\jassen", data[1][1]);
            Assert.AreEqual("", data[1][2]);
            Assert.AreEqual("No", data[1][3]);

            Assert.AreEqual("9a50e2d1-3e6b-49f8-9763-52705a4abebe", data[2][0]);
            Assert.AreEqual("RUSPDB\\PDBDev", data[2][1]);
            Assert.AreEqual("", data[2][2]);
            Assert.AreEqual("Yes", data[2][3]);
        }

        [TestMethod]
        public async Task Test_that_the_details_view_is_returned_for_a_valid_user()
        {
            // Arrange
            var user = GetFirstUser();

            _mockRepository.Setup(f => f.FindAsync(user.Id)).Returns(Task.FromResult(user));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Details(user.Id) as ViewResult;

            // Assert
            var model = result.Model as UserRolesViewModel;

            Assert.AreEqual(user.Id, model.User.Id);
            Assert.AreEqual(true, model.Roles.Any(f => f.Name == Role.CanEdit && f.IsChecked)); // all roles are returned so they're shown in the view, the IsChecked property indicates if the user belongs to that role
        }

        [TestMethod]
        public async Task Test_for_the_details_view_that_a_bad_request_status_code_is_returned_for_a_null_id()
        {
            // Arrange

            // Act
            var result = await _controller.Details(null) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public async Task Test_for_the_details_view_that_a_not_found_status_code_is_returned_for_a_non_existant_user()
        {
            // Arrange
            _mockRepository.Setup(f => f.FindAsync("this is a guid for a user that doesn't exist")).Returns(Task.FromResult((ApplicationUser)null));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Details("this is a guid for a user that doesn't exist") as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public void Test_that_the_create_view_is_returned()
        {
            // Arrange
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.AreEqual("Create", result.ViewName);

            _mockRepository.Verify(f => f.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
        }

        [TestMethod]
        public async Task Test_creating_a_valid_user()
        {
            // Arrange
            var model = GetFirstUserUserRolesViewModel();
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            _mockRepository.Setup(f => f.AddAsync(model.User, roleNames)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.Create(model) as RedirectToRouteResult;

            // Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            _mockRepository.Verify(f => f.AddAsync(model.User, roleNames), Times.Once);
            _mockRepository.Verify(f => f.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
            _mockRepository.Verify(f => f.RemoveAsync(It.IsAny<object[]>()), Times.Never);
        }

        [TestMethod]
        public async Task Test_creating_an_invalid_user_by_not_specifying_a_user_name()
        {
            // Arrange
            var model = GetFirstUserUserRolesViewModel();
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            _mockRepository.Setup(f => f.AddAsync(model.User, roleNames)).Returns(Task.FromResult(IdentityResult.Failed(new string[] { "Name cannot be null or empty." })));

            // Act
            var result = await _controller.Create(model) as ViewResult;
            var errors = result.ViewData.ModelState[string.Empty].Errors;

            // Assert
            Assert.AreEqual("Create", result.ViewName);
            Assert.AreEqual(errors[0].ErrorMessage, "Name cannot be null or empty.");

            _mockRepository.Verify(f => f.AddAsync(model.User, roleNames), Times.Once);
        }

        [TestMethod]
        public async Task Test_that_the_edit_view_is_returned()
        {
            // Arrange
            var user = GetFirstUser();

            _mockRepository.Setup(f => f.FindAsync(user.Id)).Returns(Task.FromResult(user));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Edit(user.Id) as ViewResult;

            // Assert
            Assert.AreEqual("Edit", result.ViewName);
        }

        [TestMethod]
        public async Task Test_for_the_edit_view_that_a_bad_request_status_code_is_returned_for_a_null_id()
        {
            // Arrange

            // Act
            var result = await _controller.Edit((string)null) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public async Task Test_for_the_edit_view_that_a_not_found_status_code_is_returned_for_a_non_existant_user()
        {
            // Arrange
            _mockRepository.Setup(f => f.FindAsync("this is a guid for a user that doesn't exist")).Returns(Task.FromResult((ApplicationUser)null));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Edit("this is a guid for a user that doesn't exist") as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public async Task Test_editing_a_valid_user()
        {
            // Arrange
            var model = GetFirstUserUserRolesViewModel();
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            _mockRepository.Setup(f => f.UpdateAsync(model.User, roleNames)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.Edit(model) as RedirectToRouteResult;

            // Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            _mockRepository.Verify(f => f.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
            _mockRepository.Verify(f => f.UpdateAsync(model.User, roleNames), Times.Once);
            _mockRepository.Verify(f => f.RemoveAsync(It.IsAny<object[]>()), Times.Never);
        }

        [TestMethod]
        public async Task Test_editing_an_invalid_user_by_not_specifying_a_user_name()
        {
            // Arrange
            var model = GetFirstUserUserRolesViewModel();
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            _mockRepository.Setup(f => f.UpdateAsync(model.User, roleNames)).Returns(Task.FromResult(IdentityResult.Failed(new string[] { "Name cannot be null or empty." })));

            // Act
            var result = await _controller.Edit(model) as ViewResult;
            var errors = result.ViewData.ModelState[string.Empty].Errors;

            // Assert
            Assert.AreEqual("Edit", result.ViewName);
            Assert.AreEqual(errors[0].ErrorMessage, "Name cannot be null or empty.");

            _mockRepository.Verify(f => f.UpdateAsync(model.User, roleNames), Times.Once);
        }

        [TestMethod]
        public async Task Test_that_the_delete_view_is_returned()
        {
            // Arrange
            var user = GetFirstUser();

            _mockRepository.Setup(f => f.FindAsync(user.Id)).Returns(Task.FromResult(user));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Delete(user.Id) as ViewResult;

            // Assert
            Assert.AreEqual("Delete", result.ViewName);
        }

        [TestMethod]
        public async Task Test_for_the_delete_view_that_a_bad_request_status_code_is_returned_for_a_null_id()
        {
            // Arrange

            // Act
            var result = await _controller.Delete((string)null) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public async Task Test_for_the_delete_view_that_a_not_found_status_code_is_returned_for_a_non_existant_user()
        {
            // Arrange
            _mockRepository.Setup(f => f.FindAsync("this is a guid for a user that doesn't exist")).Returns(Task.FromResult((ApplicationUser)null));
            _mockRepository.Setup(f => f.GetAllRoles()).Returns(GetAllRoles());

            // Act
            var result = await _controller.Delete("this is a guid for a user that doesn't exist") as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
        }

        [TestMethod]
        public async Task Test_deleting_a_valid_user()
        {
            // Arrange
            var user = GetFirstUser();

            _mockRepository.Setup(f => f.FindAsync(user.Id)).Returns(Task.FromResult(user));
            _mockRepository.Setup(f => f.RemoveAsync(user.Id)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.DeleteConfirmed(user.Id) as RedirectToRouteResult;

            // Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, result.RouteValues["controller"]);

            _mockRepository.Verify(f => f.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
            _mockRepository.Verify(f => f.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
            _mockRepository.Verify(f => f.RemoveAsync(user.Id), Times.Once);
        }
    }
}