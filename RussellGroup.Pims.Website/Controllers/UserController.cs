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
using RussellGroup.Pims.DataAccess.Respositories;
using RussellGroup.Pims.DataAccess.ViewModels;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanEditUsers)]
    public class UserController : Controller
    {
        private readonly IUserRepository _repository;

        public UserController(IUserRepository _repository)
        {
            this._repository = _repository;
        }

        // GET: /User/
        public async Task<ActionResult> Index()
        {
            var roles = await _repository.GetAllRoles().ToArrayAsync();
            var users = await _repository.GetAll().ToArrayAsync();

            var model = users.Select(ur => new UserRoles()
            {
                User = ur,
                Roles = roles.Where(r => ur.Roles.Select(u => u.RoleId).Contains(r.Id)).ToArray()
            });

            return View(model);
        }

        // GET: /User/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: /User/Create
        public ActionResult Create()
        {
            var user = new ApplicationUser();
            return View(user);
        }

        // POST: /User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FormCollection collection)
        {
            var roleIds = collection.GetGuids("role-id-field").Select(f => f.ToString());
            var roles = await _repository.GetAllRoles().Where(f => roleIds.Contains(f.Id)).Select(f => f.Name).ToArrayAsync();

            var user = new ApplicationUser { UserName = collection["User.UserName"], LockoutEnabled = false };

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(user, roles);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: /User/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(FormCollection collection)
        {
            var roleIds = collection.GetGuids("role-id-field").Select(f => f.ToString());
            var roles = await _repository.GetAllRoles().Where(f => roleIds.Contains(f.Id)).Select(f => f.Name).ToArrayAsync();
            var isLockoutEnabled = bool.Parse(collection["User.LockoutEnabled"].Split(',')[0]);

            ApplicationUser user = await _repository.FindAsync(collection["User.Id"]);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(user, roles, isLockoutEnabled);
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: /User/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _repository.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            await _repository.RemoveAsync(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(ApplicationUser user)
        {
            var model = new UserRoles()
            {
                User = user,
                Roles = _repository.GetAllRoles()
                    .ToArray()
                    .Select(f => new ApplicationRole
                    {
                        Id = f.Id,
                        Name = f.Name,
                        IsChecked = user.Roles.Select(u => u.RoleId).Contains(f.Id),
                        Description = f.Description
                    })
                    .ToArray()
            };

            return base.View(model);
        }
    }
}
