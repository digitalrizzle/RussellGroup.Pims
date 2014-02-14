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

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Roles = RoleType.Administrator)]
    public class UserController : Controller
    {
        private readonly IUserRepository repository;

        public UserController(IUserRepository repository)
        {
            this.repository = repository;
        }

        // GET: /User/
        public async Task<ActionResult> Index()
        {
            return View(await repository.GetAll().ToListAsync());
        }

        // GET: /User/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await repository.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: /User/Create
        public ActionResult Create()
        {
            var user = new User
            {
                Roles = repository.Roles.Where(r => r.RoleId == repository.Roles.Max(f => f.RoleId)).ToArray(),
                IsEnabled = true
            };

            return View(user);
        }

        // POST: /User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserId,Name,RoleId,IsGroup,IsEnabled")] User user)
        {
            if (ModelState.IsValid)
            {
                await repository.Add(user);
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: /User/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await repository.Find(id);
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
        public async Task<ActionResult> Edit([Bind(Include="UserId,Name,RoleId,IsGroup,IsEnabled")] User user)
        {
            if (ModelState.IsValid)
            {
                await repository.Update(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: /User/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await repository.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await repository.Remove(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(User user)
        {
            var roles = repository.Roles.OrderBy(f => f.Name);
            var role = user != null && user.Roles != null && user.Roles.Count > 0 ? user.Roles.Min(f => f.RoleId) : repository.Roles.Max(f => f.RoleId);
            user.RoleId = role;

            ViewBag.Roles = new SelectList(roles, "RoleId", "Name", role);

            return base.View(user);
        }
    }
}
