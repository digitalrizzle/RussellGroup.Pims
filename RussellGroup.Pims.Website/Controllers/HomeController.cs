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

        public HomeController(IPlantRepository repository)
        {
            _repository = repository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Help()
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

            var unknown = plants.Count(f => f.StatusId == Status.Unknown);
            var available = plants.Count(f => f.StatusId == Status.Available);
            var checkedout = plants.Count(f => f.StatusId == Status.CheckedOut);
            var stolen = plants.Count(f => f.StatusId == Status.Stolen);
            var underRepair = plants.Count(f => f.StatusId == Status.UnderRepair);
            var writtenOff = plants.Count(f => f.StatusId == Status.WrittenOff);

            var data = new[]
            {
                new { key = "available", value = available },
                new { key = "checked out", value = checkedout },
                new { key = "unknown", value = unknown },
                new { key = "stolen", value = stolen },
                new { key = "under repair", value = underRepair },
                new { key = "written off", value = writtenOff },
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetConditionData()
        {
            var plants = _repository.GetAll();

            var unknown = plants.Count(f => f.ConditionId == Condition.Unknown);
            var poor = plants.Count(f => f.ConditionId == Condition.Poor);
            var fair = plants.Count(f => f.ConditionId == Condition.Fair);
            var good = plants.Count(f => f.ConditionId == Condition.Good);
            var excellent = plants.Count(f => f.ConditionId == Condition.Excellent);

            var data = new[]
            {
                new { key = "excellent", value = excellent },
                new { key = "good", value = good },
                new { key = "fair", value = fair },
                new { key = "poor", value = poor },
                new { key = "unknown", value = unknown },
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}