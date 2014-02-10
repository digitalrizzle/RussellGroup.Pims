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

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Roles = RoleType.Administrator)]
    public class UserController : Controller
    {
        private PimsContext db = new PimsContext();

        // GET: /User/
        public async Task<ActionResult> Index()
        {
            return View(await db.Users.Include("Roles").ToListAsync());
        }

        // GET: /User/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
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
                Roles = db.Roles.Where(r => r.RoleId == db.Roles.Max(f => f.RoleId)).ToArray(),
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
                Role role = db.Roles.Single(r => r.RoleId == user.RoleId);
                user.Roles = new List<Role>();
                user.Roles.Add(role);

                db.Users.Add(user);
                await db.SaveChangesAsync();
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
            User user = await db.Users.FindAsync(id);
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
                db.Entry(user).State = EntityState.Modified;

                await db.Entry(user).Collection(f => f.Roles).LoadAsync();
                Role role = db.Roles.Single(r => r.RoleId == user.RoleId);
                user.Roles.RemoveAll();
                user.Roles.Add(role);

                await db.SaveChangesAsync();

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
            User user = await db.Users.FindAsync(id);
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
            User user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(User user)
        {
            var roles = db.Roles.OrderBy(f => f.Name);
            var role = user != null && user.Roles != null && user.Roles.Count > 0 ? user.Roles.Min(f => f.RoleId) : db.Roles.Max(f => f.RoleId);
            user.RoleId = role;

            ViewBag.Roles = new SelectList(roles, "RoleId", "Name", role);

            return base.View(user);
        }
    }
}
