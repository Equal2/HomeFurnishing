using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Xml.Linq;
using CustomEventArgs;
using DocumentEvents;
using JobOrderDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;


namespace Jobs.Controllers
{
    [Authorize]
    public class SmsController : System.Web.Mvc.Controller
    {
        IExceptionHandlingService _exception;

        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public SmsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult SendSms()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendSms(SmsViewModel Svm)
        {
            string SmsAPI = "http://my.msgwow.com/api/sendhttp.php?authkey=163094A1yq0cNVbFr85953f3cf&mobiles=" + Svm.MobileNoList + "&message="+ Svm.Message +"&sender=Kanpur&route=4";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(SmsAPI);
            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
            string responseString = respStreamReader.ReadToEnd();
            respStreamReader.Close();
            myResp.Close();

            return RedirectToAction("SendSms").Success("Sms send successfully.");
        }

        public ActionResult GetPerson(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = GetPersonList(searchTerm, filter);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public IQueryable<ComboBoxResult> GetPersonList(string term, int? filter)
        {
            var list = (from Le in db.Persons
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : Le.Name.ToLower().Contains(term.ToLower()))
                        && Le.Mobile != null
                        orderby Le.Name
                        select new ComboBoxResult
                        {
                            id = Le.Mobile,
                            text = Le.Name
                        });
            return list;
        }

        public JsonResult SetPerson(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Person> prod = from p in db.Persons
                                                       where p.PersonID == temp
                                                       select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().Mobile.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

    }


}