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

namespace Jobs.Areas.Rug.Controllers
{
    public class TempUserStoreController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

       
          IUnitOfWork _unitOfWork;
          public TempUserStoreController(IUnitOfWork unitOfWork)
          {
              _unitOfWork = unitOfWork;
          }
        // GET: /ProductMaster/
          public ActionResult Index()
          { 
              return View();
          }

        // GET: /ProductMaster/Create
          public ActionResult Create()
          {       
              return View();
          }

      

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
