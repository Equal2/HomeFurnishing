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

namespace Jobs.Controllers
{
   [Authorize]
    public class DuplicateDocumentCheckController : System.Web.Mvc.Controller
    {

       IDuplicateDocumentCheckService _DuplicateCheckService;

       public DuplicateDocumentCheckController(IDuplicateDocumentCheckService ser)
       {
           _DuplicateCheckService = ser;
       }

       public JsonResult DuplicateCheckForCreate(string table, string docno, int doctypeId)
       {
           var temp = (_DuplicateCheckService.CheckForDocNoExists(table,docno,doctypeId));
           return Json(new { returnvalue = temp });
       }

       public JsonResult DuplicateCheckForEdit(string table, string docno, int doctypeId, int headerid)
       {
           var temp = (_DuplicateCheckService.CheckForDocNoExists(table, docno, doctypeId, headerid));
           return Json(new { returnvalue = temp });
       }
       
    }
}
