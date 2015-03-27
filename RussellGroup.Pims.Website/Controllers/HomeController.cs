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
        [PimsAuthorize(Role.CanView, Role.CanEdit, Role.CanEditCategories, Role.CanEditUsers)]
        public ActionResult Index()
        {
            return View();
        }

        [PimsAuthorize(Role.CanView, Role.CanEdit, Role.CanEditCategories, Role.CanEditUsers)]
        public ActionResult Help()
        {
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}