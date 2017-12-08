using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using System.Data.SqlClient;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductUidHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        IProductUidHeaderService _ProductUidHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public ProductUidHeaderController(IProductUidHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductUidHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /ProductUidHeader/

        public ActionResult Index()
        {
            IEnumerable<ProductUidHeaderIndexViewModel> p = _ProductUidHeaderService.GetProductUidHeaderIndexList();
            return View(p);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ProductUidHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ProductUidHeaderService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print(int id)
        {
            ProductUidHeader header = _ProductUidHeaderService.Find(id);

            String GenHeaderId = header.GenDocTypeId.ToString () + '-' + header.GenDocNo;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + GenHeaderId);
        }


        [HttpGet]
        public ActionResult PrintSeries()
        {
            string ProductUidHeaderIdStr = "";
            string PendingToPrintProductUidHeaderIdsSessionVarName = "ProductUid_PendingToPrintProductUidHeaderIds" + User.Identity.Name.ToString().ToString();
            if (System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName] == null)
            {
                ProductUidHeaderIdStr = "";
            }
            else
            {
                ProductUidHeaderIdStr = System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName].ToString();
            }
            
            DisposeSessionPrintVariable();
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + ProductUidHeaderIdStr);
        }


        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductUid);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private void PrepareViewBag(ProductUidHeaderIndexViewModel s)
        {

        }

        // GET: /ProductUidHeader/Create

        public ActionResult Create()
        {
            ProductUidHeaderIndexViewModel p = new ProductUidHeaderIndexViewModel();
            p.GenDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductUid).DocumentTypeId;
            p.GenDocDate = DateTime.Now;
            p.GenDocNo = new ProductUidHeaderService(_unitOfWork).FGetNewDocNo("GenDocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProductUidHeaders", p.GenDocTypeId, p.GenDocDate);
            ViewBag.Mode = "Add";




            string PendingToPrintProductUidHeaderIdsSessionVarName = "ProductUid_PendingToPrintProductUidHeaderIds" + User.Identity.Name.ToString().ToString();
            if (System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName] != null)
            {
                p.PrintProductUidHeaderIds = System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName].ToString();
            }
            

            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(ProductUidHeaderIndexViewModel svm)
        {
            string GodownName = "";
            string ProcessName = "";
            ViewBag.Mode = "Add";

            if (svm.Qty == 0 )
            {
                ModelState.AddModelError("", "Please enter qty.");
                PrepareViewBag(svm);
                return View("Create", svm);
            }

            if (svm.GenPersonId == 0 || svm.GenPersonId == null)
            {
                ModelState.AddModelError("", "Please enter Person.");
                PrepareViewBag(svm);
                return View("Create", svm);
            }

            if (svm.ProductId == 0 || svm.ProductId == null)
            {
                ModelState.AddModelError("", "Please enter Product.");
                PrepareViewBag(svm);
                return View("Create", svm);
            }

            if (ModelState.IsValid)
            {
                if (svm.ProductUidHeaderId == 0)
                {
                    if (svm.GodownId != null && svm.GodownId != 0)
                    {
                        GodownName = new GodownService(_unitOfWork).Find((int)svm.GodownId).GodownName;
                    }

                    if (svm.ProcessId != null && svm.ProcessId != 0)
                    {
                        ProcessName = new ProcessService(_unitOfWork).Find((int)svm.ProcessId).ProcessName;
                    }

                    
                    

                    ProductUidHeader ProdUidHeader = new ProductUidHeader();
                    //ProdUidHeader.GenDocNo = svm.GenDocNo;
                    ProdUidHeader.ProductId = svm.ProductId;
                    ProdUidHeader.GenDocTypeId = svm.GenDocTypeId;
                    ProdUidHeader.GenDocDate = svm.GenDocDate;
                    ProdUidHeader.GenPersonId = svm.GenPersonId;
                    ProdUidHeader.GenDocNo = new ProductUidHeaderService(_unitOfWork).FGetNewDocNo("GenDocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProductUidHeaders", svm.GenDocTypeId, svm.GenDocDate);
                    ProdUidHeader.CreatedDate = DateTime.Now;
                    ProdUidHeader.ModifiedDate = DateTime.Now;
                    ProdUidHeader.CreatedBy = User.Identity.Name;
                    ProdUidHeader.ModifiedBy = User.Identity.Name;

                    ProdUidHeader.GenRemark = "Godown : " + GodownName + ", Process : " + ProcessName;
                    

                    ProdUidHeader.ObjectState = Model.ObjectState.Added;
                    context.ProductUidHeader.Add(ProdUidHeader);

                    List<string> uids = new List<string>();

                    using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                    {
                        sqlConnection.Open();

                        int TypeId = (int) svm.GenDocTypeId;

                        SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FGenerateBarcodeList ( " + TypeId + ", " + svm.Qty + ")", sqlConnection);

                        SqlDataReader Reader = (Totalf.ExecuteReader());
                        while (Reader.Read())
                        {
                            uids.Add((string)Reader.GetValue(0));
                        }
                    }



                    foreach (string UidItem in uids)
                    {
                        ProductUid ProdUid = new ProductUid();

                        ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                        ProdUid.ProductUidName = UidItem;
                        ProdUid.ProductId = ProdUidHeader.ProductId;
                        ProdUid.IsActive = true;
                        ProdUid.CreatedBy = User.Identity.Name;
                        ProdUid.CreatedDate = DateTime.Now;
                        ProdUid.ModifiedBy = User.Identity.Name;
                        ProdUid.ModifiedDate = DateTime.Now;
                        //ProdUid.GenLineId = item.JobOrderLineId;
                        //ProdUid.GenDocId = pd.JobOrderHeaderId;
                        ProdUid.GenDocNo = ProdUidHeader.GenDocNo;
                        ProdUid.GenDocTypeId = ProdUidHeader.GenDocTypeId;
                        ProdUid.GenDocDate = ProdUidHeader.GenDocDate;
                        ProdUid.GenPersonId = ProdUidHeader.GenPersonId;
                        ProdUid.CurrenctProcessId = svm.ProcessId;
                        ProdUid.CurrenctGodownId = svm.GodownId;
                        ProdUid.Status = ProductUidStatusConstants.Generated;
                        ProdUid.LastTransactionDocNo = ProdUidHeader.GenDocNo;
                        ProdUid.LastTransactionDocTypeId = ProdUidHeader.GenDocTypeId;
                        ProdUid.LastTransactionDocDate = ProdUidHeader.GenDocDate;
                        ProdUid.LastTransactionPersonId = ProdUidHeader.GenPersonId;
                        //ProdUid.LastTransactionLineId = item.JobOrderLineId;
                        //ProdUid.ProductUIDId = count;
                        ProdUid.ObjectState = Model.ObjectState.Added;
                        //new ProductUidService(_unitOfWork).Create(ProdUid);
                        context.ProductUid.Add(ProdUid);


                    }


                    try
                    {
                        context.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);                       
                        return View("Create", svm);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductUid).DocumentTypeId,
                        DocId = ProdUidHeader.ProductUidHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    ReAssignSessionVariables(ProdUidHeader.ProductUidHeaderId);
                    //return RedirectToAction("Edit", new { id = ProdUidHeader.ProductUidHeaderId }).Success("Data saved Successfully");
                    return RedirectToAction("Create").Success("Data saved Successfully");
                }
            }
            PrepareViewBag(svm);            
            return View("Create", svm);
        }


        // GET: /ProductUidHeader/Edit/5
        public ActionResult Edit(int id, string PrevAction, string PrevController)
        {
            if (PrevAction != null)
            {
                ViewBag.Redirect = PrevAction;
            }

            ProductUidHeaderIndexViewModel svm = _ProductUidHeaderService.GetProductUidHeaderIndexViewModel(id);
            PrepareViewBag(svm);
            if (svm == null)
            {
                return HttpNotFound();
            }
            return View("Create", svm);
        }



        // GET: /PurchaseOrderHeader/Delete/5

        public ActionResult Delete(int id, string PrevAction, string PrevController)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductUidHeaderIndexViewModel ProductUidHeader = _ProductUidHeaderService.GetProductUidHeaderIndexViewModel(id);
            if (ProductUidHeader == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                //string temp = (Request["Redirect"].ToString());
                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                var ProductUidHeader = _ProductUidHeaderService.GetProductUidHeaderIndexViewModel(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ProductUidHeader,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var ProductUid = new ProductUidService(_unitOfWork).GetProductUidList (vm.id);

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in ProductUid)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });
                    new ProductUidService(_unitOfWork).Delete(item.ProductUIDId);
                }

                // Now delete the Purhcase Order Header
                new ProductUidHeaderService(_unitOfWork).Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                //Commit the DB
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductUid).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));               

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }

        public JsonResult GetPendingToPrint()
        {
            int PendingToPrint = 0;

            string PendingToPrintSessionVarName = "ProductUid_PendingToPrint" + User.Identity.Name.ToString().ToString();
            if (System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName] != null)
            {
                PendingToPrint = Convert.ToInt32(System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName]);
            }
            else
            {
                PendingToPrint = 0;
            }

            return Json(PendingToPrint);
        }

        public JsonResult GetPendingToPrintProductUidHeaderIds()
        {
            string PendingToPrintProductUidHeaderIds = "";

            string PendingToPrintProductUidHeaderIdsSessionVarName = "ProductUid_PendingToPrintProductUidHeaderIds" + User.Identity.Name.ToString().ToString();
            if (System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName] != null)
            {
                PendingToPrintProductUidHeaderIds = Convert.ToString(System.Web.HttpContext.Current.Session[PendingToPrintProductUidHeaderIdsSessionVarName]);
            }
            else
            {
                PendingToPrintProductUidHeaderIds = "";
            }

            return Json(PendingToPrintProductUidHeaderIds);
        }

        public void ReAssignSessionVariables(int ProductUidHeaderId)
        {
            string PendingToPrintSessionVarName = "ProductUid_PendingToPrint" + User.Identity.Name.ToString();
            if (System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName] == null)
            {
                System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName] = 1;
            }
            else
            {
                System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName] = (int)System.Web.HttpContext.Current.Session[PendingToPrintSessionVarName] + 1;
            }

            string ProductUidHeaderIdSeriesSessionVarName = "ProductUid_PendingToPrintProductUidHeaderIds" + User.Identity.Name.ToString();


            //String GenHeaderId = header.GenDocTypeId.ToString() + '-' + header.GenDocNo;
            ProductUidHeader ProductUidHeader = new ProductUidHeaderService(_unitOfWork).Find(ProductUidHeaderId);

            if (System.Web.HttpContext.Current.Session[ProductUidHeaderIdSeriesSessionVarName] == null)
            {
                System.Web.HttpContext.Current.Session[ProductUidHeaderIdSeriesSessionVarName] = ProductUidHeader.GenDocTypeId.ToString() + '-' + ProductUidHeader.GenDocNo;
            }
            else
            {
                System.Web.HttpContext.Current.Session[ProductUidHeaderIdSeriesSessionVarName] = System.Web.HttpContext.Current.Session[ProductUidHeaderIdSeriesSessionVarName].ToString() + "," + ProductUidHeader.GenDocTypeId.ToString() + '-' + ProductUidHeader.GenDocNo;
            }


        }

        public void DisposeSessionPrintVariable()
        {
            string PendingToPrintSessionVarName = "ProductUid_PendingToPrint" + User.Identity.Name.ToString();
            string ProductUidHeaderIdSeriesSessionVarName = "ProductUid_PendingToPrintProductUidHeaderIds" + User.Identity.Name.ToString();

            System.Web.HttpContext.Current.Session.Remove(PendingToPrintSessionVarName);
            System.Web.HttpContext.Current.Session.Remove(ProductUidHeaderIdSeriesSessionVarName);
        }

        public JsonResult GetLastValues()
        {
            List<ProductUidLastValues> ProductUidLastValuesJson = new List<ProductUidLastValues>();

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductUid).DocumentTypeId;
            string CreatedBy = User.Identity.Name.ToString();

            ProductUidLastValues temp = (from H in context.ProductUidHeader
                        join L in context.ProductUid on H.ProductUidHeaderId equals L.ProductUidHeaderId into ProductUidTable
                        from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                        where H.GenDocTypeId == DocTypeId && H.CreatedBy == CreatedBy
                        orderby H.ProductUidHeaderId descending
                               select new ProductUidLastValues
                        {
                            PersonId = H.GenPersonId,
                            PersonName = H.GenPerson.Name,
                            GodownId = ProductUidTab.CurrenctGodownId,
                            GodownName = ProductUidTab.CurrenctGodown.GodownName,
                            ProcessId = ProductUidTab.CurrenctProcessId,
                            ProcessName = ProductUidTab.CurrenctProcess.ProcessName,
                        }).Take(1).FirstOrDefault();

            ProductUidLastValuesJson.Add(temp);

            return Json(ProductUidLastValuesJson);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    public class ProductUidLastValues
    {
        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
    }

}



