using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    public class HireController : Controller
    {
        private PimsContext db = new PimsContext();

        public ActionResult Plant()
        {
            var plants = db.Plants.ToList();
            var transaction = new PlantHireTransaction()
            {
                Plants = plants,
                PlantHire = new List<PlantHire>()
            };

            return View(transaction);
        }

        //public async Task<ActionResult> Plant(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Job job = await db.Jobs.FindAsync(id);
        //    if (job == null)
        //    {
        //        return HttpNotFound();
        //    };

        //    var transaction = new PlantHireTransaction { Job = job };

        //    return View(transaction);
        //}

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

        private ActionResult View(PlantHireTransaction transaction)
        {
            var jobs = db.Jobs.OrderByDescending(f => f.WhenStarted);

            ViewBag.Jobs = new SelectList(jobs, "JobId", "Description", transaction.JobId);

            return base.View(transaction);
        }
    }
}