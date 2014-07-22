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
using DataTables.Mvc;

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

        // https://github.com/ALMMa/datatables.mvc
        public JsonResult GetDataTableResult([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var hint = model.Search != null ? model.Search.Value : string.Empty;
            var sortColumn = model.Columns.GetSortedColumns().First();

            var all = _repository.GetAll();

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.UserName.Contains(hint) ||
                    f.Email.Contains(hint) ||
                    (f.LockoutEnabled ? "Yes" : "No").Contains(hint)
                );

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<ApplicationUser, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

            // sorting
            var sorted = sortColumn.SortDirection == Column.OrderDirection.Ascendant
                ? filtered.OrderBy(ordering)
                : filtered.OrderByDescending(ordering);

            // display
            var roles = _repository.GetAllRoles().ToList();

            var paged = sorted
                .Skip(model.Start)
                .Take(model.Length)
                .ToList()
                .Select(c => new
                {
                    c.Id,
                    c.UserName,
                    Role = c.Roles != null ? string.Join(", ", roles.Where(r => c.Roles.Select(i => i.RoleId).Contains(r.Id)).Select(r => r.Name)) : string.Empty,
                    LockoutEnabled = c.LockoutEnabled.ToYesNo(),
                    CrudLinks = this.CrudLinks(new { id = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
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
