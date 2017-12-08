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
using Data.Infrastructure;
using Service;
using AutoMapper;
using Presentation.ViewModels;
using Presentation;
using System.Configuration;

namespace Jobs.Controllers
{
   [Authorize]
    public class GetNewDocNoController : System.Web.Mvc.Controller
    {
       IUnitOfWork _unitOfWork;

       public GetNewDocNoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


       public JsonResult GetNewDocNo(string table, int doctype,DateTime date,int division,int site)
       {
           var temp = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + "." + table, doctype, date, division, site);
           return Json(new { returnvalue = temp },JsonRequestBehavior.AllowGet);
       }

       
    }
}
