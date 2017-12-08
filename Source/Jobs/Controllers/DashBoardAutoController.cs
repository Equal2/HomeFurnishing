using System.Collections.Generic;
using System.Web.Mvc;
using Service;

//using ProjLib.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System;

namespace Module
{
    [Authorize]
    public class DashBoardAutoController : Controller
    {
        IDashBoardAutoService _DashBoardAutoService;
        public DashBoardAutoController(IDashBoardAutoService DashBoardAutoService)
        {
            _DashBoardAutoService = DashBoardAutoService;
        }

        public ActionResult DashBoardAuto()
        {
            return View();
        }
        public JsonResult GetVehicleSale()
        {
            IEnumerable<DashBoardSale> VehicleSale = _DashBoardAutoService.GetVehicleSale();

            JsonResult json = Json(new { Success = true, Data = VehicleSale }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetVehicleProfit()
        {
            IEnumerable<DashBoardProfit> VehicleProfit = _DashBoardAutoService.GetVehicleProfit();

            JsonResult json = Json(new { Success = true, Data = VehicleProfit }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetVehicleOutstanding()
        {
            IEnumerable<DashBoardOutstanding> VehicleOutstanding = _DashBoardAutoService.GetVehicleOutstanding();

            JsonResult json = Json(new { Success = true, Data = VehicleOutstanding }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetVehicleStock()
        {
            IEnumerable<DashBoardStock> VehicleStock = _DashBoardAutoService.GetVehicleStock();

            JsonResult json = Json(new { Success = true, Data = VehicleStock }, JsonRequestBehavior.AllowGet);
            return json;
        }



        public JsonResult GetSpareSale()
        {
            IEnumerable<DashBoardSale> SpareSale = _DashBoardAutoService.GetSpareSale();

            JsonResult json = Json(new { Success = true, Data = SpareSale }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetSpareProfit()
        {
            IEnumerable<DashBoardProfit> SpareProfit = _DashBoardAutoService.GetSpareProfit();

            JsonResult json = Json(new { Success = true, Data = SpareProfit }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetSpareOutstanding()
        {
            IEnumerable<DashBoardOutstanding> SpareOutstanding = _DashBoardAutoService.GetSpareOutstanding();

            JsonResult json = Json(new { Success = true, Data = SpareOutstanding }, JsonRequestBehavior.AllowGet);
            return json;
        }
        public JsonResult GetSpareStock()
        {
            IEnumerable<DashBoardStock> SpareStock = _DashBoardAutoService.GetSpareStock();

            JsonResult json = Json(new { Success = true, Data = SpareStock }, JsonRequestBehavior.AllowGet);
            return json;
        }


        public JsonResult GetChartData()
        {
            IEnumerable<DashBoardChartData> ChartData = _DashBoardAutoService.GetChartData();

            JsonResult json = Json(new { Success = true, Data = ChartData }, JsonRequestBehavior.AllowGet);
            return json;
        }

        public JsonResult GetPieChartData()
        {
            IEnumerable<DashBoardPieChartData> PieChartData = _DashBoardAutoService.GetPieChartData();

            JsonResult json = Json(new { Success = true, Data = PieChartData }, JsonRequestBehavior.AllowGet);
            return json;
        }

        public JsonResult GetVehicleSaleChartData()
        {
            IEnumerable<DashBoardVehicleSaleChartData> VehicleSaleChartData = _DashBoardAutoService.GetVehicleSaleChartData();

            JsonResult json = Json(new { Success = true, Data = VehicleSaleChartData }, JsonRequestBehavior.AllowGet);
            return json;
        }


    }   


}