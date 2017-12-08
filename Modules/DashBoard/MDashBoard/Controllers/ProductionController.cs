using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.ViewModel;
using System.Data.SqlClient;
using MDashBoard.Models;
using System.Collections;
using System.Configuration;
using System.Data;
using Data;


namespace MDashBoard.Controllers
{
    [Authorize]
    public class ProductionController : System.Web.Mvc.Controller
    {
        //
        // GET: /Production/
        //working Web.spRep_MD_DashBoard
        #region Global
        private ApplicationDbContext DB = new ApplicationDbContext();
        int SiteId = (int?)System.Web.HttpContext.Current.Session["SiteId"] ?? 17;
        int DivisionId = (int?)System.Web.HttpContext.Current.Session["DivisionId"] ?? 6;
        SubProductionDataset DS = new SubProductionDataset();
        #endregion

        #region Function
        public Dataset Production(int? Id)
        {
            return null;
        }

        #endregion

        public ActionResult Index()
        {
            // return View(Production(Id));   
            return View(Month());
        }

        /* public JsonResult JIndex(int? Id)
         {
             ViewBag.Id = Id;
             if (Id != null)
             {
                 return Json(Production(Id),JsonRequestBehavior.AllowGet);
                 // showdata = Production(Id);
             }
             else
             {
                 return null;
             }


         }*/

        #region Production
        /* public ActionResult odalPage()
        {
            return PartialView("_Create", Production());
        }

       
        public Dataset Production()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            Dataset DS = new Dataset();
            IEnumerable<Production> Month = DB.Database.SqlQuery<Production>("Select Head,Value from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Month' Order By Short desc").ToList();
            IEnumerable<Production> JobWorker = DB.Database.SqlQuery<Production>("Select Head,Value from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='JobWorker'").ToList();
            IEnumerable<Production> Construction = DB.Database.SqlQuery<Production>("Select Head,Value from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Construction'").ToList();
            DS.Month = Month.ToList();
            DS.JobWorker = JobWorker.ToList();
            DS.Construction = Construction.ToList();            
            return DS;
        }*/

        public Dataset MonthProduction()
        {
            Dataset DS = new Dataset();
            IEnumerable<Production> Month = DB.Database.SqlQuery<Production>("Select Head,Value from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Month'").ToList();
            DS.Month = Month.ToList();
            return DS;
        }
        #endregion

        #region DyeingOrder

        public ActionResult DyeingModel()
        {
            return PartialView("_DyeingModel", DyeingReceive());
        }


        public SubProductionDataset DyeingReceive()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select Jobworker,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Group By Jobworker").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult DyeingModelMJ(string MValue, string JValue, string Type)
        {
            ViewBag.Title = "";
            if (Type == "Jobworker")
            {
                ViewBag.Title = JValue.ToString();
            }
            if (Type == "Month")
            {
                ViewBag.Title = MValue.ToString();
            }
            return PartialView("_DyeingModelMonthJobworker", DyeingReceive(MValue, JValue, Type));
        }
        public SubProductionDataset DyeingReceive(string MValue, string JValue, string Type)
        {
            if (Type == "Jobworker")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + JValue + "'" + " group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where   Jobworker=" + "'" + JValue + "'" + " and  SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Month = Month.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();

            }
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select Jobworker,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + MValue + "'" + " Group By Jobworker").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where   Month=" + "'" + MValue + "'" + " and SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Jobworker = Jobworker.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }



        public ActionResult DyeingModelName(string MValue, string JValue, string Title, string Type)
        {
            ViewBag.Title = Title.ToString();
            string Link = null;
            if (Type == "Jobworker")
            {
                Link = "/Production/DyeingModelMJ?MValue=&JValue=" + JValue.ToString() + "&Type=Jobworker";
            }
            else if (Type == "Month")
            {
                Link = "/Production/DyeingModelMJ?MValue=" + MValue.ToString() + "&JValue=&Type=Month";
            }
            ViewBag.Type = Link.ToString();
            return PartialView("_DyeingModelName", DyeingJobworker(MValue, JValue));
        }
        public SubProductionDataset DyeingJobworker(string MValue, string JValue)
        {
            string Condition = "";
            if (JValue != "" && JValue != null)
            {
                Condition = Condition + " AND JobWorker=" + "'" + JValue + "'";
            }
            if (MValue != "" && MValue != null)
            {
                Condition = Condition + " AND Month=" + "'" + MValue + "'";
            }
            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select Name,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition + "  Group By Name").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition).ToList();
            DS.Name = Name.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        #endregion

        #region Packing
        public ActionResult PackingModel()
        {
            return PartialView("_PackingModel", Packing());
        }
        public SubProductionDataset Packing()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select Name,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Group By Name order By Name Asc").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,MaxDate)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDate)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Name = Name.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }


        public ActionResult PackingModelMJ(string MValue, string JValue, string Type)
        {
            ViewBag.Title = "";
            if (Type == "Jobworker")
            {
                ViewBag.Title = JValue.ToString();
            }
            if (Type == "Month")
            {
                ViewBag.Title = MValue.ToString();
            }
            return PartialView("_PackingModelMonthJobworker", PackingReceive(MValue, JValue, Type));
        }
        public SubProductionDataset PackingReceive(string MValue, string JValue, string Type)
        {
            if (Type == "Jobworker")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Name=" + "'" + JValue + "'" + " group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,MaxDate)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where   Name=" + "'" + JValue + "'" + " and  SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Month = Month.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();

            }
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Week = DB.Database.SqlQuery<SubProduction>("Select Month,Week,sum(isnull(Area,0)) AS Area,format(Min(convert(DATE,MinDate)),'dd/MMM/yyyy')+'-'+format(max(convert(DATE,MaxDate)),'dd/MMM/yyyy') AS Name from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + MValue + "'" + "GROUP BY Week,Month ORDER BY Min(convert(DATE,MinDate)),max(convert(DATE,MaxDate))").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,MaxDate)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where   Month=" + "'" + MValue + "'" + " and SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Week = Week.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }


        public ActionResult PackingModelDay(string MValue, string WValue, string Title)
        {
            ViewBag.Title = Title.ToString();
            string Link = null;
            Link = "/Production/PackingModelMJ?MValue=" + MValue.ToString() + "&JValue=&Type=Month";
            ViewBag.Type = Link.ToString();
            return PartialView("_PackingModelDateWise", PackingDate(MValue, WValue));
        }
        public SubProductionDataset PackingDate(string MValue, string WValue)
        {
            string Condition = "";
            if (WValue != "" && WValue != null)
            {
                Condition = Condition + " AND Week=" + "'" + WValue + "'";
            }
            if (MValue != "" && MValue != null)
            {
                Condition = Condition + " AND Month=" + "'" + MValue + "'";
            }

            IEnumerable<SubProduction> Day = DB.Database.SqlQuery<SubProduction>("Select format(min(convert(DATE,MinDate)),'dd/MMM/yyyy') as DATE,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition + "  Group By convert(DATE,MinDATE)").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,maxDate)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition).ToList();
            DS.Day = Day.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }
        #endregion

        #region Purchase

        public ActionResult PurchaseModel()
        {
            return PartialView("_PurchaseModel", Purchase());
        }
        public SubProductionDataset Purchase()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            //IEnumerable<SubProduction> SupplierName = DB.Database.SqlQuery<SubProduction>("Select SupplierName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By SupplierName").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(Area) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By ProductCategoryName").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            //DS.SupplierName = SupplierName.ToList();
            DS.Construction = Construction.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult PurchaseMCWise(string MValue, string CValue, string Type)
        {
            ViewBag.Title = "";
            if (Type == "Construction")
            {
                ViewBag.Title = CValue.ToString();
            }
            if (Type == "Month")
            {
                ViewBag.Title = MValue.ToString();
            }
            return PartialView("_PurcaseMonthConstruction", PurchaseReceive(MValue, CValue, Type));
        }
        public SubProductionDataset PurchaseReceive(string MValue, string CValue, string Type)
        {
            if (Type == "Construction")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + CValue + "'" + " group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where   ProductCategoryName=" + "'" + CValue + "'" + " and  SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Month = Month.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();

            }
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + MValue + "'" + " Group By ProductCategoryName").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where   Month=" + "'" + MValue + "'" + " and SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
                DS.Construction = Construction.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }

        public ActionResult PurchaseSupplierName(string MValue, string CValue, string Title, string Type)
        {

            ViewBag.Title = Title.ToString();
            string Link = null;
            if (Type == "Construction")
            {
                Link = "/Production/PurchaseMCWise?MValue=&CValue=" + CValue.ToString() + "&Type=Construction";
            }
            else if (Type == "Month")
            {

                Link = "/Production/PurchaseMCWise?MValue=" + MValue + "&CValue=&Type=Month";
            }
            ViewBag.Type = Link.ToString();
            return PartialView("_PurchaseSupplierName", SupplierName(MValue, CValue));
        }
        public SubProductionDataset SupplierName(string MValue, string CValue)
        {
            string Condition = "";
            if (CValue != "" && CValue != null)
            {
                Condition = Condition + " AND ProductCategoryName=" + "'" + CValue + "'";
            }
            if (MValue != "" && MValue != null)
            {
                Condition = Condition + " AND Month=" + "'" + MValue + "'";
            }
            IEnumerable<SubProduction> SupplierName = DB.Database.SqlQuery<SubProduction>("Select SupplierName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition + "  Group By SupplierName").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " " + Condition).ToList();
            DS.SupplierName = SupplierName.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }
        #endregion

        #region Finishing
        public ActionResult FinishingModel()
        {
            return PartialView("_FinishingModel", Finishing());
        }
        public SubProductionDataset Finishing()
        {
            IEnumerable<SubProduction> ProcessName = DB.Database.SqlQuery<SubProduction>("select ProcessName ,sum(isnull(Area,0)) as Area,sum(CASE WHEN convert(DATE,DATE)=convert(DATE,getdate()) THEN isnull(Area,0) ELSE 0 END) AS TodayArea from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Group By ProcessName order by max(ProcessSr)").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,Date)),'dd/MMM/yyyy') as MaxDate,format(Min(convert(DATE,Date)),'dd/MMM/yyyy') as MinDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " ").ToList();
            DS.ProcessName = ProcessName.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult FinishingMonthModel(string Process)
        {
            ViewBag.Title = Process.ToString();
            return PartialView("_FinishingMonthModel", finishingMonth(Process));
        }

        public SubProductionDataset finishingMonth(string Process)
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProcessName=" + "'" + Process + "'" + "  Group By Month order by Max(Month1) desc").ToList();
            IEnumerable<SubProduction> MinMaxdate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,Date)),'dd/MMM/yyyy') as MaxDate,format(Min(convert(DATE,Date)),'dd/MMM/yyyy') as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProcessName=" + "'" + Process + "'" + " Group By Month").ToList();
            DS.Month = Month.ToList();
            DS.MinMaxDate = MinMaxdate.ToList();
            return DS;
        }

        public ActionResult FinishingDate(string Process, string Month, string Title)
        {
            ViewBag.Title = Title.ToString();
            string Link = null;
            Link = "/Production/FinishingMonthModel?Process=" + Process.ToString();
            ViewBag.Type = Link.ToString();
            return PartialView("_FinishingModelDateWise", FinishingMonthProcess(Process, Month));
        }
        public SubProductionDataset FinishingMonthProcess(string Process, string Month)
        {

            IEnumerable<SubProduction> Date = DB.Database.SqlQuery<SubProduction>("Select format(convert(DATE,Date),'dd/MMM/yyyy') as Date,sum(isnull(Area,0)) as Area  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProcessName=" + "'" + Process + "'" + "and  Month=" + "'" + Month + "'" + "  Group By convert(DATE,Date) order by convert(DATE,Date) desc").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,Date)),'dd/MMM/yyyy') as MaxDate,format(Min(convert(DATE,Date)),'dd/MMM/yyyy') as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Finishing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProcessName=" + "'" + Process + "'" + "and  Month=" + "'" + Month + "'" + "  ").ToList();
            DS.Day = Date.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        #endregion


        #region MonthData
        public MonthDtatSet MonthPageLoad()
        {
            IEnumerable<SubProduction> TopMonthProduction = DB.Database.SqlQuery<SubProduction>("Select top 5 Area,Name from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_TopProductionMember Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Month' order by Area Desc").ToList();
            IEnumerable<SubProduction> TopYearProduction = DB.Database.SqlQuery<SubProduction>("Select top 5 Area,Name from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_TopProductionMember Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Year' Order By Area Desc").ToList();
            MonthDtatSet DS = new MonthDtatSet();
            DS.TopMonthProduction = TopMonthProduction.ToList();
            DS.TopYearProduction = TopYearProduction.ToList();
            return DS;
        }


        public MonthDtatSet Month()
        {


            string Date = DateTime.Now.ToString("MMM -yy");
            string CurrentDate = DateTime.Now.ToString("dd/MMM/yy");
            MonthDtatSet DS = new MonthDtatSet();
            //var onth = DB.Database.SqlQuery<float>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").FirstOrDefault();
            //var onth2 = DB.Database.SqlQuery<decimal>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").FirstOrDefault();
            IEnumerable<SubProduction> ProductionMonth = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").ToList();
            IEnumerable<SubProduction> ProductionToday = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and DATE=" + "'" + CurrentDate + "'" + " group By DATE").ToList();

            IEnumerable<SubProduction> DyeingReceiveMonth = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").ToList();
            IEnumerable<SubProduction> DyeingReceiveToday = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and DATE=" + "'" + CurrentDate + "'" + " group By DATE").ToList();

            IEnumerable<SubProduction> PurchaseReceiveMonth = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").ToList();
            IEnumerable<SubProduction> PurchaseReceiveToday = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_PurchaseProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and DATE=" + "'" + CurrentDate + "'" + " group By DATE").ToList();

            IEnumerable<SubProduction> PackingMonth = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Date + "'" + " group By Month").ToList();
            IEnumerable<SubProduction> PackingToday = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_Packing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and MaxDate=" + "'" + CurrentDate + "'" + " group By MaxDate").ToList();

            //IEnumerable<Packing> PackingMonth = DB.Database.SqlQuery<Packing>("Select Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDPacking Where SiteId=" + SiteId + " and  Head=" + "'" + Date + "'" + " and DivisionId=" + DivisionId + " and Type='Month'").ToList();
            //IEnumerable<Purchase> PurchaseMonth = DB.Database.SqlQuery<Purchase>("Select Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDPurchase Where SiteId=" + SiteId + " and Head=" + "'" + Date + "'" + " and  DivisionId=" + DivisionId + " and Type='Month'").ToList();

            IEnumerable<SubProduction> TopMonthProduction = DB.Database.SqlQuery<SubProduction>("Select top 5 Area,Name from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_TopProductionMember Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Month' order by Area Desc").ToList();
            IEnumerable<SubProduction> TopYearProduction = DB.Database.SqlQuery<SubProduction>("Select top 5 Area,Name from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_TopProductionMember Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Type='Year' Order By Area Desc").ToList();

            IEnumerable<SubProduction> ChartProduction = DB.Database.SqlQuery<SubProduction>("Select Month,CurrentArea,PreviousArea,round(((isnull(CurrentArea,0)-isnull(PreviousArea,0))/((isnull(CurrentArea,0)+isnull(PreviousArea,0))/2))*100,2) as Percentage  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDProduction_Chart Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Order By MonthInt").ToList();

            IEnumerable<SubProduction> OrderToBeIssue = DB.Database.SqlQuery<SubProduction>("Select round(sum(AreaTobeIssue),0) as AreaTobeIssue,sum(TodayArea) as TodayArea from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> OnLoom = DB.Database.SqlQuery<SubProduction>("Select Area,Month,color from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Order by Month1 desc").ToList();
            IEnumerable<SubProduction> DyeingBal = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> DyeingPlan = DB.Database.SqlQuery<SubProduction>("Select sum(isnull(DyeingPlanQty,0)) as DyeingPlan,NULL as DyeingOrder from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> MaterialReqBalIssue = DB.Database.SqlQuery<SubProduction>("Select sum(BalanceQty) as BalanceQty,NULL as IssueQty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();

            #region TodayIssueReceive
            IEnumerable<SubProduction> WeavingOrder = DB.Database.SqlQuery<SubProduction>("Select sum(Qty) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_dashBoard_WeaingOrder Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> WeavingReceive = DB.Database.SqlQuery<SubProduction>("Select sum(Qty) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_dashBoard_WeaingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> DyeingReceive = DB.Database.SqlQuery<SubProduction>("Select sum(Qty) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_dashBoard_DyeingReceive Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> DyeingOrder = DB.Database.SqlQuery<SubProduction>("Select sum(Qty) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_dashBoard_DyeingOrder Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            IEnumerable<SubProduction> WeavingMaterialIssue = DB.Database.SqlQuery<SubProduction>("Select sum(Qty) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MD_dashBoard_MaterialIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            #endregion




            DS.ProductionMonth = ProductionMonth.ToList();
            DS.ProductionToday = ProductionToday.ToList();

            DS.DyeingReceiveMonth = DyeingReceiveMonth.ToList();
            DS.DyeingReceiveToday = DyeingReceiveToday.ToList();

            DS.PurchaseReceiveMonth = PurchaseReceiveMonth.ToList();
            DS.PurchaseReceiveToday = PurchaseReceiveToday.ToList();

            DS.PackingMonth = PackingMonth.ToList();
            DS.PackingToday = PackingToday.ToList();

            DS.TopMonthProduction = TopMonthProduction.ToList();
            DS.TopYearProduction = TopYearProduction.ToList();

            DS.ChartProduction = ChartProduction.ToList();

            DS.OrderToBeIssue = OrderToBeIssue.ToList();
            DS.OnLoom = OnLoom.ToList();
            DS.DyeingBal = DyeingBal.ToList();
            DS.DyeingPlan = DyeingPlan.ToList();
            DS.MaterialReqBalIssue = MaterialReqBalIssue.ToList();
            #region TodayIssueReceive
            DS.WeavingOrder =WeavingOrder.ToList();
            DS.WeavingReceive =WeavingReceive.ToList();
            DS.DyeingReceive =DyeingReceive.ToList();
            DS.DyeingOrder =DyeingOrder.ToList();
            DS.WeavingMaterialIssue = WeavingMaterialIssue.ToList();
            #endregion

            return DS;
        }
        #endregion


        #region SubProduction

        public ActionResult odalPage()
        {
            return PartialView("_Create", ShowMainProduction());
        }
        public SubProductionDataset ShowMainProduction()
        {

            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By Jobworker").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(Area) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By ProductCategoryName").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.Construction = Construction.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult SubProductionMonth(string Month)
        {
            ViewBag.Month = Month.ToString();
            return PartialView("_SubProductionMonthModel", ShowMonth(Month));
        }

        public SubProductionDataset ShowMonth(string Month)
        {
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Month + "'" + " group By Jobworker").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(Area) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and Month=" + "'" + Month + "'" + " group By ProductCategoryName").ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.Construction = Construction.ToList();
            DS.MinMaxDate.AddRange(MinMaxDate(" AND Month=" + "'" + Month + "'"));
            return DS;
        }

        public ActionResult SubProductionJobWorker(string JobWorker)
        {
            ViewBag.Jobworker = JobWorker.ToString();
            return PartialView("_SubProductionJobWorkerModel", ShowJobWorker(JobWorker));
        }
        public SubProductionDataset ShowJobWorker(string JobWorker)
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and JobWorker=" + "'" + JobWorker + "'" + " group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and JobWorker=" + "'" + JobWorker + "'" + " group By ProductCategoryName").ToList();
            DS.Month = Month.ToList();
            DS.MinMaxDate.AddRange(MinMaxDate(" AND JobWorker=" + "'" + JobWorker + "'"));
            DS.Construction = Construction.ToList();
            return DS;
        }



        public ActionResult SubProductionConstruction(string Construction)
        {
            ViewBag.Construction = Construction.ToString();
            return PartialView("_SubProductionConstructionModel", ShowConstruction(Construction));
        }
        public SubProductionDataset ShowConstruction(string Construction)
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and ProductCategoryName=" + "'" + Construction + "'" + " group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and ProductCategoryName=" + "'" + Construction + "'" + " group By Jobworker").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.MinMaxDate.AddRange(MinMaxDate(" AND ProductCategoryName=" + "'" + Construction + "'"));
            return DS;
        }




        public ActionResult SubProductionName(string Jobworker, string Month, string Construction, string Title, string Type)
        {
            ViewBag.Title = Title.ToString();


            string Link = null;
            if (Type == "Construction")
            {
                Link = "/Production/SubProductionConstruction?Construction=" + Construction.ToString();
            }
            else if (Type == "JobWorker")
            {
                Link = "/Production/SubProductionJobWorker?JobWorker=" + Jobworker.ToString();
            }
            else if (Type == "Month")
            {
                Link = "/Production/SubProductionMonth?Month=" + Month.ToString();
            }
            ViewBag.Type = Link.ToString();
            return PartialView("_SubProductionNameModel", NameProduction(Jobworker, Month, Construction));
        }


        public SubProductionDataset NameProduction(string Jobworker, string Month, string Construction)
        {
            string Condition = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Condition = Condition + " AND JobWorker=" + "'" + Jobworker + "'";
            }
            if (Month != "" && Month != null)
            {
                Condition = Condition + " AND Month=" + "'" + Month + "'";
            }
            if (Construction != "" && Construction != null)
            {
                Condition = Condition + " and ProductCategoryName=" + "'" + Construction + "'";
            }



            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterMonth = new SqlParameter("@Month", Month);
            SqlParameter SqlParameterJobWorker = new SqlParameter("@Jobworker", Jobworker);
            SqlParameter SqlParameterCategoryName = new SqlParameter("@ProductCategoryName", Construction);
            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spRep_SubProduction @SiteId,@DivisionId,@ProductCategoryName,@Month,@Jobworker", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterCategoryName, SqlParameterMonth, SqlParameterJobWorker).ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,DATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString()).ToList();
            DS.Name = Name.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }


        public List<SubProduction> MinMaxDate(string Condition)
        {
            return DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,MinDATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".SubProduction Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString()).ToList();

        }
        #endregion


        #region OrderTobeissue
        public ActionResult OrderTobeissue()
        {
            ViewBag.Color = "bg-yellow-active";
            return PartialView("_OrderToBeissueMain", ShowOrdertoBeissue());
        }
        public SubProductionDataset ShowOrdertoBeissue()
        {

            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Name as Jobworker,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and AreaTobeIssue >0  group By Name").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By ProductCategoryName").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.Construction = Construction.ToList();
            return DS;
        }

        public ActionResult SubOrderTobeIssue(string Type, string Value)
        {
            ViewBag.Title = Value.ToString();
            ViewBag.Color = "bg-yellow-active";
            return PartialView("_SubOrderTobeIssue", ShowOrdertoBeissueCondition(Type, Value));
        }
        public SubProductionDataset ShowOrdertoBeissueCondition(string Type, string Value)
        {
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Name as Jobworker,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " and AreaTobeIssue >0  group By Name").ToList();
                IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "and  Month=" + "'" + Value + "'" + "  group By ProductCategoryName").ToList();
                DS.Jobworker = Jobworker.ToList();
                DS.Construction = Construction.ToList();
            }
            else if (Type == "Construction")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Name as Jobworker,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + Value + "'" + " and AreaTobeIssue >0  group By Name").ToList();
                DS.Month = Month.ToList();
                DS.Jobworker = Jobworker.ToList();
            }
            else if (Type == "Name")
            {
                IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "and  Name=" + "'" + Value + "'" + "  group By ProductCategoryName").ToList();
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(AreaTobeIssue,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OrderTobeIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Name=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                DS.Construction = Construction.ToList();
                DS.Month = Month.ToList();
            }
            return DS;
        }

        #endregion

        #region OnloomBalance

        public ActionResult OnLoomBalance()
        {
            ViewBag.Color = "bg-green-active";
            return PartialView("_OnLoomBalance", ShowOnloomBalance());
        }
        public SubProductionDataset ShowOnloomBalance()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Jobworker").ToList();
            IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By ProductCategoryName").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.Construction = Construction.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult SonloomBalance(string Type, string Value)
        {
            ViewBag.Title = Value.ToString();
            ViewBag.Color = "bg-green-active";
            return PartialView("_SubonLoomBalance", SubOnloomBalance(Type, Value));
        }
        public SubProductionDataset SubOnloomBalance(string Type, string Value)
        {
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + "  group By Jobworker").ToList();
                IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + "  group By ProductCategoryName").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " ").ToList();
                DS.Jobworker = Jobworker.ToList();
                DS.Construction = Construction.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            else if (Type == "Construction")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + Value + "'" + "   group By Jobworker").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  ProductCategoryName=" + "'" + Value + "'" + "").ToList();
                DS.Month = Month.ToList();
                DS.Jobworker = Jobworker.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            else if (Type == "Name")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> Construction = DB.Database.SqlQuery<SubProduction>("Select ProductCategoryName,sum(isnull(Area,0)) as Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  and  Jobworker=" + "'" + Value + "'" + "  group By ProductCategoryName").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "").ToList();
                DS.Month = Month.ToList();
                DS.Construction = Construction.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }

        public ActionResult OnLoomName(string Type, string Jobworker, string Month, string Construction)
        {
            string Title = "";
            string Link = "";           
            if (Jobworker != "" && Jobworker != null)
            {
                Title =  Title + " >> " + Jobworker.ToString();
            }
            if (Month != "" && Month != null)
            {
                Title =Title + " >> " + Month.ToString();
            }
            if (Construction != "" && Construction != null)
            {
                Title =  Title + " >> " + Construction.ToString();
            }

            if (Type == "Month")
            {
                Link = "/Production/SonloomBalance?Type=" + Type.ToString() + "&Value=" + Month.ToString();
            }
            else if (Type == "Name")
            {
                Link = "/Production/SonloomBalance?Type=" + Type.ToString() + "&Value=" + Jobworker.ToString();
            }
            else if (Type == "Construction")
            {
                Link = "/Production/SonloomBalance?Type=" + Type.ToString() + "&Value=" + Construction.ToString();
            }
            ViewBag.Title = "On Loom Balance " + Title.ToString();
            ViewBag.Type = Link.ToString();
            ViewBag.Color = "bg-green-active";
            return PartialView("_OnLoomName", NameOnLoom(Jobworker, Month, Construction));
        }
        public SubProductionDataset NameOnLoom(string Jobworker, string Month, string Construction)
        {
            string Condition = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Condition = Condition + " AND Jobworker=" + "'" + Jobworker + "'";
            }
            if (Month != "" && Month != null)
            {
                Condition = Condition + " AND Month=" + "'" + Month + "'";
            }
            if (Construction != "" && Construction != null)
            {
                Condition = Condition + " and ProductCategoryName=" + "'" + Construction + "'";
            }
            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select Name,sum(isnull(Area,0)) as Area  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + " Group By Name ").ToList();

            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,DATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString()).ToList();
            
            List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select Name,sum(isnull(Area,0)) as Area,format(convert(DATE,DATE),'dd/MMM/yyyy') as DateTime  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_OnLoom Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + " Group By Name,DATE Order By DATE ").ToList();

            var Join = (from p in Name
                        join t in JWDetails on p.Name equals t.Name
                        into table
                        from ta in table.DefaultIfEmpty()
                        group new { p, ta } by p.Name into g
                        select new SubProduction
                        {
                            Name = g.Key,
                            Area = g.Max(m => m.p.Area),
                            NameDetails = g.Select(m => m.ta).ToList(),
                        }).ToList();

            DS.Name = Join.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;

            
        }
        #endregion


        #region DyeingBalance
        public ActionResult DyeingBalance()
        {
            ViewBag.Color = "bg-aqua-active";
            return PartialView("_DyeingBalance", ShowDyeingBalance());
        }
        public SubProductionDataset ShowDyeingBalance()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Jobworker").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Jobworker = Jobworker.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult SubDyeingBalance(string Type, string Value)
        {
            ViewBag.Title = Value.ToString();
            ViewBag.Color = "bg-aqua-active";
            return PartialView("_SubDyeingBalance", SDyeingBalance(Type, Value));
        }
        public SubProductionDataset SDyeingBalance(string Type, string Value)
        {
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + "  group By Jobworker").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " ").ToList();
                DS.Jobworker = Jobworker.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
           else if (Type == "Name")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "").ToList();
                DS.Month = Month.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }


        public ActionResult DyeingBalanceName(string Type, string Jobworker, string Month)
        {
            string Title = "";
            string Link = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Title = Title + " >> " + Jobworker.ToString();
            }
            if (Month != "" && Month != null)
            {
                Title = Title + " >> " + Month.ToString();
            }
          

            if (Type == "Month")
            {
                Link = "/Production/SubDyeingBalance?Type=" + Type.ToString() + "&Value=" + Month.ToString();
            }
            else if (Type == "Name")
            {
                Link = "/Production/SubDyeingBalance?Type=" + Type.ToString() + "&Value=" + Jobworker.ToString();
            }
          
            ViewBag.Title = "Dyeing Balance " + Title.ToString();
            ViewBag.Type = Link.ToString();
            ViewBag.Color = "bg-aqua-active";
            return PartialView("_DyeingOrderBalanceName", ShowDyeingBalanceName(Jobworker, Month));
        }
        public SubProductionDataset ShowDyeingBalanceName(string Jobworker, string Month)
        {
            string Condition = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Condition = Condition + " AND Jobworker=" + "'" + Jobworker + "'";
            }
            if (Month != "" && Month != null)
            {
                Condition = Condition + " AND Month=" + "'" + Month + "'";
            }

            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select Name,sum(isnull(Qty,0)) as Qty  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + " Group By Name ").ToList();

            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(max(convert(DATE,DATE)),'dd/MMM/yyyy') as MaxDate,format(min(convert(DATE,DATE)),'dd/MMM/yyyy')  as MinDate  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString()).ToList();

            List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select Name,sum(isnull(Qty,0)) as Qty,format(convert(DATE,DATE),'dd/MMM/yyyy') as DateTime  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingBal Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + " Group By Name,DATE Order By DATE ").ToList();

            var Join = (from p in Name
                        join t in JWDetails on p.Name equals t.Name
                        into table
                        from ta in table.DefaultIfEmpty()
                        group new { p, ta } by p.Name into g
                        select new SubProduction
                        {
                            Name = g.Key,
                            Qty = g.Max(m => m.p.Qty),
                            NameDetails = g.Select(m => m.ta).ToList(),
                        }).ToList();

            DS.Name = Join.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;


        }
        #endregion 


        #region DyeingPlanBalance

        public ActionResult DyeingPlanBalance()
        {
            ViewBag.Color = "bg-red-active";
            return PartialView("_DyeingPlanBalance", ShowDyeingPlanBalance());
        }
        public SubProductionDataset ShowDyeingPlanBalance()
        {
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(DyeingPlanQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Month order By max(Month1) desc").ToList();
            IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Name as Name,sum(isnull(DyeingPlanQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "  group By Name").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Month = Month.ToList();
            DS.Name = Jobworker.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }

        public ActionResult SubDyeinPlanBalance(string Type, string Value)
        {

            ViewBag.Type = Type == "Month" ? "Name" : "Month";
            ViewBag.Title = Value.ToString();
            ViewBag.Color = "bg-red-active";
            return PartialView("_SubDyeingPlanBalance", SDyeingPlanBlance(Type, Value));
        }
        public SubProductionDataset SDyeingPlanBlance(string Type, string Value)
        {
           
            if (Type == "Month")
            {
                IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select  Name as Name,sum(isnull(DyeingPlanQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + "  group By Name").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " ").ToList();
                List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select Name,sum(isnull(DyeingPlanQty,0)) as Qty,format(convert(DATE,DATE),'dd/MMM/yyyy') as DateTime  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " Group By Name,DATE Order By DATE ").ToList();
                var Join = (from p in Name
                            join t in JWDetails on p.Name equals t.Name
                            into table
                            from ta in table.DefaultIfEmpty()
                            group new { p, ta } by p.Name into g
                            select new SubProduction
                            {
                                Name = g.Key,
                                Qty = g.Max(m => m.p.Qty),
                                NameDetails = g.Select(m => m.ta).ToList(),
                            }).ToList();

                DS.Name = Join.ToList();
                //DS.Name = Name.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            else if (Type == "Name")
            {
                IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(DyeingPlanQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Name=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(Date),'dd/MMM/yy') as MinDate,format(max(Date),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Name=" + "'" + Value + "'" + "").ToList();
                List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select Month as Name,sum(isnull(DyeingPlanQty,0)) as Qty,format(convert(DATE,DATE),'dd/MMM/yyyy') as DateTime  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_DyeingPlan Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Name=" + "'" + Value + "'" + " Group By Month,DATE Order By DATE ").ToList();
                var Join = (from p in Month
                            join t in JWDetails on p.Month equals t.Name
                            into table
                            from ta in table.DefaultIfEmpty()
                            group new { p, ta } by p.Month into g
                            select new SubProduction
                            {
                                Name = g.Key,
                                Qty = g.Max(m => m.p.Qty),
                                NameDetails = g.Select(m => m.ta).ToList(),
                            }).ToList();

                DS.Name = Join.ToList();
                DS.MinMaxDate = MinMaxDate.ToList();
            }
            return DS;
        }
        #endregion


        #region WeavingRequestBalance
        public ActionResult WeaveRequestBalance()
        {
            ViewBag.Color = "bg-blue-active";
            return PartialView("_WeavinRequestBalance", RequestBalance());
        }
         public SubProductionDataset RequestBalance()
        {
            IEnumerable<SubProduction> JobWorker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker ,sum(isnull(BalanceQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By JobWorker").ToList();
            IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select  Month ,sum(isnull(BalanceQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By Month order By max(Month1)").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(DocDate),'dd/MMM/yy') as MinDate,format(max(DocDate),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "").ToList();
            DS.Jobworker = JobWorker.ToList();
            DS.Month = Month.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
         }

         public ActionResult SubWeavingRequestBalance(string Type, string Value)
         {
             ViewBag.Title = Value.ToString();
             ViewBag.Color = "bg-blue-active";
             return PartialView("_SubWeavingRequestBalance", SubRequestBalance(Type, Value));
         }
         public SubProductionDataset SubRequestBalance(string Type, string Value)
         {
             if (Type == "Month")
             {
                 IEnumerable<SubProduction> Jobworker = DB.Database.SqlQuery<SubProduction>("Select  Jobworker as Jobworker,sum(isnull(BalanceQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + "  group By Jobworker").ToList();
                 IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(DocDate),'dd/MMM/yy') as MinDate,format(max(DocDate),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Month=" + "'" + Value + "'" + " ").ToList();
                 DS.Jobworker = Jobworker.ToList();
                 DS.MinMaxDate = MinMaxDate.ToList();
             }
             else if (Type == "Name")
             {
                 IEnumerable<SubProduction> Month = DB.Database.SqlQuery<SubProduction>("Select Month,sum(isnull(BalanceQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "  group By Month order By max(Month1) desc").ToList();
                 IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(DocDate),'dd/MMM/yy') as MinDate,format(max(DocDate),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " and  Jobworker=" + "'" + Value + "'" + "").ToList();
                 DS.Month = Month.ToList();
                 DS.MinMaxDate = MinMaxDate.ToList();
             }
             return DS;
         }
        public ActionResult WeavingRequestBalance(string Type,string Month,string Jobworker)
        {
            string Title = "";
            string Link = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Title = Title + " >> " + Jobworker.ToString();
            }
            if (Month != "" && Month != null)
            {
                Title = Title + " >> " + Month.ToString();
            }


            if (Type == "Month")
            {
                Link = "/Production/SubWeavingRequestBalance?Type=" + Type.ToString() + "&Value=" + Month.ToString();
            }
            else if (Type == "Name")
            {
                Link = "/Production/SubWeavingRequestBalance?Type=" + Type.ToString() + "&Value=" + Jobworker.ToString();
            }

            ViewBag.Title = "Weaving Request Balance " + Title.ToString();
            ViewBag.Color = "bg-blue-active";
            ViewBag.Type = Link.ToString();
            return PartialView("_WeavingRequestBalance", ShowWeavingRequestBalance(Jobworker, Month));
        }
        public SubProductionDataset ShowWeavingRequestBalance(string Jobworker, string Month)
        {
            string Condition = "";
            if (Jobworker != "" && Jobworker != null)
            {
                Condition = Condition + " AND Jobworker=" + "'" + Jobworker + "'";
            }
            if (Month != "" && Month != null)
            {
                Condition = Condition + " AND Month=" + "'" + Month + "'";
            }

            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select  Name as Name,sum(isnull(BalanceQty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + " group By Name").ToList();
            IEnumerable<SubProduction> MinMaxDate = DB.Database.SqlQuery<SubProduction>("Select format(min(DocDate),'dd/MMM/yy') as MinDate,format(max(DocDate),'dd/MMM/yy') as MaxDate from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + ""+ Condition.ToString()+"").ToList();
            List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select Name as Name,sum(isnull(BalanceQty,0)) as Qty,format(convert(DATE,DocDate),'dd/MMM/yyyy') as DateTime,CostCenterName as CostCenter  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MdDashBoard_MReqBalIssue Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + "" + Condition.ToString() + "Group By Name,CostCenterName,DocDate Order By DocDate ").ToList();
            var Join = (from p in Name
                        join t in JWDetails on p.Name equals t.Name
                        into table
                        from ta in table.DefaultIfEmpty()
                        group new { p, ta } by p.Name into g
                        select new SubProduction
                        {
                            Name = g.Key,
                            Qty = g.Max(m => m.p.Qty),
                            NameDetails = g.Select(m => m.ta).ToList(),
                        }).ToList();
            DS.Name = Join.ToList();
            DS.MinMaxDate = MinMaxDate.ToList();
            return DS;
        }
        #endregion

        #region TodayIssueReceive
        public ActionResult ShowTodayIssueReceive(string Type)
        {
            string title = "";
            string Unit="";
            string Color = "";
           
            if (Type == "WeaingOrder")
            {
                title = "Weaing Order";
                Unit="Sq.Yard";
                Color = "bg-yellow";

            }
            if (Type == "WeaingReceive")
            {
                title = "Weaing Receive";
                 Unit="Sq.Yard";
                 Color = "bg-green";
            }
            if (Type == "DyeingReceive")
            {
                title = "Dyeing Receive";
                 Unit="Kg";
                 Color = "bg-aqua";
            }
            if (Type == "DyeingOrder")
            {
                title = "Dyeing Order";
                Unit="Kg";
                Color = "bg-red";
            }
            if (Type == "MaterialIssue")
            {
                title = "Weaving Material Issue";
                Unit="Kg";
                Color = "bg-blue-gradient";
            }
            ViewBag.Title = title.ToString();
            ViewBag.Unit = Unit.ToString();
            ViewBag.Color = Color.ToString();
            return PartialView("_TodayIssueReceive", TodayIssueReceive(Type));
        }
        public SubProductionDataset TodayIssueReceive(string Type)
        {
            string TableName = "";
            if (Type == "WeaingOrder")
            {
                TableName = ".MD_dashBoard_WeaingOrder";
            }
            if (Type == "WeaingReceive")
            {
                TableName = ".MD_dashBoard_WeaingReceive";
            }
            if (Type == "DyeingReceive")
            {
                TableName = ".MD_dashBoard_DyeingReceive";
            }
            if (Type == "DyeingOrder")
            {
                TableName = ".MD_dashBoard_DyeingOrder";
            }
             if (Type == "MaterialIssue")
            {
                TableName = ".MD_dashBoard_MaterialIssue";
            }

           
            IEnumerable<SubProduction> Name = DB.Database.SqlQuery<SubProduction>("Select  Jobworker,sum(isnull(Qty,0)) as Qty from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + TableName.ToString()  + "  Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " group By JobWorker").ToList();
            List<JobworkerDetail> JWDetails = DB.Database.SqlQuery<JobworkerDetail>("Select JobWorker,Name as Name,sum(isnull(Qty,0)) as Qty  from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + TableName.ToString() + "  Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId + " Group By JobWorker,Name").ToList();
            var Join = (from p in Name
                        join t in JWDetails on p.Jobworker equals t.JobWorker
                        into table
                        from ta in table.DefaultIfEmpty()
                        group new { p, ta } by p.Jobworker into g
                        select new SubProduction
                        {
                            Jobworker = g.Key,
                            Qty = g.Max(m => m.p.Qty),
                            NameDetails = g.Select(m => m.ta).ToList(),
                        }).ToList();
            DS.Name = Join.ToList();
            return DS;
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
