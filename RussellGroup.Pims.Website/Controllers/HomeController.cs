using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPlantRepository _repository;

        public HomeController(IPlantRepository _repository)
        {
            this._repository = _repository;
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
            var plants = _repository.GetAll();

            var unknown = plants.Count(f => f.Status.Id == Status.Unknown);
            var available = plants.Count(f => f.Status.Id == Status.Available);
            var unavailable = plants.Count(f => f.Status.Id == Status.Unavailable);
            var missing = plants.Count(f => f.Status.Id == Status.Missing);
            var stolen = plants.Count(f => f.Status.Id == Status.Stolen);
            var underRepair = plants.Count(f => f.Status.Id == Status.UnderRepair);
            var writtenOff = plants.Count(f => f.Status.Id == Status.WrittenOff);

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
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}