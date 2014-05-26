using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.DataAccess.ViewModels;
using RussellGroup.Pims.Website.Models;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanEditUsers)]
    public class UserController : Controller
    {
        private readonly IUserRepository _repository;

        public UserController(IUserRepository repository)
        {
            _repository = repository;
        }
        //
        // GET: /User/
        public ActionResult Index()
        {
            return base.View("Index");
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            var entries = _repository.GetAll();
            var sortColumnIndex = model.iSortCol_0;

            // ordering
            Func<ApplicationUser, string> ordering = (c =>
                    sortColumnIndex == 1 ? c.UserName :
                        sortColumnIndex == 2 ? string.Empty : c.LockoutEnabled.ToString());

            // sorting
            IEnumerable<ApplicationUser> ordered = model.sSortDir_0 == "asc" ?
                entries.OrderBy(ordering) :
                entries.OrderByDescending(ordering);

            // get the display values
            var displayData = ordered
                .Select(c => new[]
                {
                    c.Id,
                    c.UserName,
                    GetUserRolesViewModel(c).RoleNames,
                    c.LockoutEnabled.ToYesNo(),
                    this.CrudLinks(new { id = c.Id }, true)
                });

            // filter for sSearch
            var hint = model.sSearch;
            var searched = new List<string[]>();

            if (string.IsNullOrEmpty(hint))
            {
                searched.AddRange(displayData);
            }
            else
            {
                foreach (var row in displayData)
                {
                    // don't include in the search the id as it is hidden from the display
                    // don't include in the search the CRUD links either
                    for (int index = 1; index < row.Length - 1; index++)
                    {
                        if (!string.IsNullOrEmpty(row[index]) && row[index].IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            searched.Add(row);
                            break;
                        }
                    }
                }
            }

            // filter for the display
            var filtered = searched
                .Skip(searched.Count > model.iDisplayLength ? model.iDisplayStart : 0)
                .Take(searched.Count > model.iDisplayLength ? model.iDisplayLength : searched.Count);

            var result = new
            {
                model.sEcho,
                iTotalRecords = _repository.GetAll().Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /User/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var model = GetUserRolesViewModel(user);
            return View("Details", model);
        }

        //
        // GET: /User/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        //
        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserRolesViewModel model)
        {
            var user = model.User;
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            if (!roleNames.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one role must be selected.");
            }

            if (ModelState.IsValid)
            {
                var result = await _repository.AddAsync(user, roleNames);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            return View("Create", model);
        }

        //
        // GET: /User/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View("Edit", GetUserRolesViewModel(user));
        }

        //
        // POST: /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserRolesViewModel model)
        {
            var user = model.User;
            var roleNames = model.Roles.Where(f => f.IsChecked).Select(f => f.Name).ToArray();

            if (!roleNames.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one role must be selected.");
            }

            if (ModelState.IsValid)
            {
                var result = await _repository.UpdateAsync(user, roleNames);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            return View("Edit", model);
        }

        //
        // GET: /User/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View("Delete", GetUserRolesViewModel(user));
        }

        //
        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                await _repository.RemoveAsync(id);
                return RedirectToAction("Index");
            }

            return View("Delete", GetUserRolesViewModel(user));
        }

        private ActionResult View(string viewName, UserRolesViewModel model = null)
        {
            if (model == null)
            {
                model = GetUserRolesViewModel(new ApplicationUser());
            }

            return base.View(viewName, model);
        }

        private UserRolesViewModel GetUserRolesViewModel(ApplicationUser user)
        {
            var roles = _repository
                .GetAllRoles()
                .ToArray()
                .Select(r => new ApplicationRole
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsChecked = user != null ? user.Roles.Select(u => u.RoleId).Contains(r.Id) : false,
                    Description = r.Description
                })
                .ToList();

            var model = new UserRolesViewModel
            {
                User = user,
                Roles = roles
            };

            return model;
        }
    }
}
