using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPlantRepository repository;

        public HomeController(IPlantRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

        public JsonResult GetPlantData()
        {
            var plants = repository.GetAll();

            var unknown = plants.Count(f => f.Status.StatusId == Status.Unknown);
            var available = plants.Count(f => f.Status.StatusId == Status.Available);
            var unavailable = plants.Count(f => f.Status.StatusId == Status.Unavailable);
            var missing = plants.Count(f => f.Status.StatusId == Status.Missing);
            var stolen = plants.Count(f => f.Status.StatusId == Status.Stolen);
            var underRepair = plants.Count(f => f.Status.StatusId == Status.UnderRepair);
            var writtenOff = plants.Count(f => f.Status.StatusId == Status.WrittenOff);

            var data = new[]
            {
                new { key = "available", value = available },
                new { key = "unavailable", value = unavailable },
                new { key = "missing", value = missing },
                new { key = "unknown", value = unknown },
                new { key = "stolen", value = stolen },
                new { key = "under repair", value = underRepair },
                new { key = "written off", value = writtenOff },
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}