using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Model.ViewModels;
using Service;
using Data.Infrastructure;
using Core.Common;
using System.Xml.Linq;
using Model.ViewModel;



namespace Jobs.Controllers
{
    [Authorize]
    public class ProductRateGroupWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductRateGroupWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _exception = exec;
            _unitOfWork = unitOfWork;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult RateListHeaderIndex(int MenuId)
        {
            var RateListHeader = new RateListHeaderService(_unitOfWork).GetProductRateGroupHeader(MenuId);
            return View(RateListHeader);
        }

        public ActionResult ProductRateGroup(int Id)
        {
            ViewBag.RateListHeaderId = Id;
            var Header = db.RateListProductRateGroup.Where(m => m.RateListHeaderId == Id).Select(m => m.ProductRateGroupId).ToList();

            var ProductRateGroups = db.ProductRateGroup.Where(m => Header.Contains(m.ProductRateGroupId)).Select(m => new ComboBoxList { PropFirst = m.ProductRateGroupName, Id = m.ProductRateGroupId }).ToList();

            ViewBag.List = ProductRateGroups;
            ViewBag.Name = "Product Rate Group -" + db.RateListHeader.Find(Id).RateListName;
            ViewBag.WizardType = "Pending";
            ViewBag.SOD = "Product";
            return View();
        }


        private static int TOTAL_ROWS = 0;



        public JsonResult AjaxGetJsonProductData(int draw, int start, int length, FilterArgs Fvm)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            // note: we only sort one column at a time
            if (Request.Form["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
                SortColName = Request.Form["columns[" + sortColumn + "][data]"];
            }
            if (Request.Form["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            DataTableProductData dataTableData = new DataTableProductData();
            dataTableData.draw = draw;
            int recordsFiltered = 0;
            dataTableData.data = FilterProductData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, Fvm);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public class DataTableProductData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<ProductRateIndex> data { get; set; }
        }



        // here we simulate SQL search, sorting and paging operations
        private List<ProductRateIndex> FilterProductData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, FilterArgs Fvm)
        {
            bool Pending = false;
            bool Sample = false;

            if (string.IsNullOrEmpty(Fvm.WizardType) || Fvm.WizardType == "Pending")
                Pending = true;






            List<ProductRateIndex> list = new List<ProductRateIndex>();
            IQueryable<ProductRateIndex> _data = (from Pp in db.ProductProcess
                                                  join Rh in db.RateListHeader on Pp.ProcessId equals Rh.ProcessId into RateListHeaderTable
                                                  from RateListHeaderTab in RateListHeaderTable.DefaultIfEmpty()
                                                  join P in db.Product on Pp.ProductId equals P.ProductId into ProductTable
                                                  from ProductTab in ProductTable.DefaultIfEmpty()
                                                  join D1 in db.Dimension1 on Pp.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                                                  from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                                                  join D2 in db.Dimension2 on Pp.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                                                  from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                                                  where RateListHeaderTab.RateListHeaderId == Fvm.RateListHeaderId && (Pending ? Pp.ProductRateGroupId == null : 1 == 1)
                                                  && RateListHeaderTab.DivisionId == ProductTab.DivisionId && ProductTab.IsActive == true
                                                  group new { Pp, ProductTab, Dimension1Tab, Dimension2Tab } by new { ProductTab.ProductId, Dimension1Tab.Dimension1Id, Dimension2Tab.Dimension2Id } into g
                                                  select new ProductRateIndex
                                                  {
                                                      ProductId = g.Key.ProductId,
                                                      ProductName = g.Max(m => m.ProductTab.ProductName),
                                                      Dimension1Id = g.Key.Dimension1Id,
                                                      Dimension1Name = g.Max(m => m.Dimension1Tab.Dimension1Name),
                                                      Dimension2Id = g.Key.Dimension2Id,
                                                      Dimension2Name = g.Max(m => m.Dimension2Tab.Dimension2Name),
                                                      ImageFileName = g.Max(m => m.ProductTab.ImageFileName),
                                                      ImageFolderName = g.Max(m => m.ProductTab.ImageFolderName),
                                                      ProductRateGroupId = g.Max(m => m.Pp.ProductRateGroupId),
                                                  });


            recordTotal = _data.Count();

            if (string.IsNullOrEmpty(search))
            {

            }
            else
            {

                // simulate search
                _data = from m in _data
                        where (m.ProductName).ToLower().Contains(search.ToLower()) || (m.Dimension1Name).ToLower().Contains(search.ToLower())
                        || (m.Dimension2Name).ToLower().Contains(search.ToLower()) 
                        select m;

            }

            _data = _data.OrderBy(m => m.ProductName);


            recordFiltered = _data.Count();

            // get just one page of data
            list = _data.Select(m => new ProductRateIndex
            {
                ProductId = m.ProductId,
                ProductName = m.ProductName,
                Dimension1Id = m.Dimension1Id,
                Dimension1Name = m.Dimension1Name,
                Dimension2Id = m.Dimension2Id,
                Dimension2Name = m.Dimension2Name,
                ImageFileName = m.ImageFileName,
                ImageFolderName = m.ImageFolderName,
                ProductRateGroupId = m.ProductRateGroupId,
            })
            .Skip(start).Take((start == 0) ? 90 : length).ToList();

            return list;
        }



        public ActionResult UpdateProductRate(int ProductId, int? Dimension1Id, int? Dimension2Id, int ProductRateGroupId, int RateListHeaderId)
        {
            XElement Modifications;
            bool Flag = new ProductProcessService(_unitOfWork).UpdateProductRateGroupForProduct(ProductId, Dimension1Id, Dimension2Id, ProductRateGroupId, RateListHeaderId, User.Identity.Name, out Modifications);

            if (Flag)
            {
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                   {
                       DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                       ActivityType = (int)ActivityTypeContants.Modified,
                       xEModifications = Modifications,
                   }));
            }

            return Json(new { Success = Flag });

            //return View();
        }

        public ActionResult Filters(FilterArgs Fvm)
        {
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = "Pending", Value = "Pending" });
            temp.Add(new SelectListItem { Text = "All", Value = "All" });

            ViewBag.WizType = new SelectList(temp, "Value", "Text", Fvm.WizardType);

            List<SelectListItem> tempSOD = new List<SelectListItem>();
            tempSOD.Add(new SelectListItem { Text = "All", Value = "All" });

            ViewBag.IncSample = new SelectList(tempSOD, "Value", "Text", Fvm.SOD);

            return PartialView("_Filters", Fvm);
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

    //public class FilterArgs
    //{
    //    public string WizardType { get; set; }
    //    public string SOD { get; set; }
    //    public int RateListHeaderId { get; set; }
    //    public string DisContinued { get; set; }
    //}

    public class ProductRateIndex
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFolderName { get; set; }
        public int? ProductRateGroupId { get; set; }
    }
}
