using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ErrorController : System.Web.Mvc.Controller
    {
          
        public ActionResult PermissionDenied()
        {
            return View("PermissionDenied");
        }


    }
}
