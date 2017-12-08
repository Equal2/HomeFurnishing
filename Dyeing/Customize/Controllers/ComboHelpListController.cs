using System.Web.Mvc;
using Presentation.Helper;
using Service;
using Services.BasicSetup;
using Services.Customize;


namespace Customize.Controllers
{
    [Authorize]
    public class ComboHelpListController : System.Web.Mvc.Controller
    {
        private readonly ICustomizeComboHelpListService _cusComboHelpListService;

        public ComboHelpListController(ICustomizeComboHelpListService cusComboHelpListServ)
        {
            _cusComboHelpListService = cusComboHelpListServ;
        }

        public ActionResult GetJobWorker(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetJobWorkers(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetJobWorkersWithProcess(string searchTerm, int pageSize, int pageNum, int filter)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetJobWorkersWithProcess(searchTerm, pageSize, pageNum, filter),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleJobWorker(int Ids)
        {
            return Json(_cusComboHelpListService.GetJobWorkerById(Ids));
        }

        public JsonResult SetJobWorkers(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleJobWorkers(Ids));
        }

        public ActionResult GetEmployee(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetEmployees(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetEmployeeWithProcess(string searchTerm, int pageSize, int pageNum, int filter)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetEmployeesWithProcess(searchTerm, pageSize, pageNum, filter),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleEmployee(int Ids)
        {
            return Json(_cusComboHelpListService.GetEmployeeById(Ids));
        }

        public JsonResult SetEmployees(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleEmployees(Ids));
        }

        public ActionResult GetGodown(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetGodowns(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleGodown(int Ids)
        {
            return Json(_cusComboHelpListService.GetGodownById(Ids));
        }

        public JsonResult SetGodowns(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleGodowns(Ids));
        }

        public ActionResult GetMachine(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetMachines(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleMachine(int Ids)
        {
            return Json(_cusComboHelpListService.GetMachineById(Ids));
        }

        public JsonResult SetMachines(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleMachines(Ids));
        }

        public ActionResult GetDimension1(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetDimension1(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleDimension1(int Ids)
        {
            return Json(_cusComboHelpListService.GetDimension1ById(Ids));
        }

        public JsonResult SetDimension1s(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleDimension1(Ids));
        }

        public ActionResult GetDimension2(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetDimension2(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleDimension2(int Ids)
        {
            return Json(_cusComboHelpListService.GetDimension2ById(Ids));
        }

        public JsonResult SetDimension2s(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleDimension2(Ids));
        }
        public ActionResult GetProcess(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetProcess(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProcess(int Ids)
        {
            return Json(_cusComboHelpListService.GetProcessById(Ids));
        }

        public JsonResult SetProcesss(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleProcess(Ids));
        }

        public ActionResult GetProduct(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetProduct(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProduct(int Ids)
        {
            return Json(_cusComboHelpListService.GetProductById(Ids));
        }

        public JsonResult SetProducts(string Ids)
        {
            return Json(_cusComboHelpListService.GetMultipleProduct(Ids));
        }

        public ActionResult GetPersonWithProcess(string searchTerm, int pageSize, int pageNum, int filter)
        {
            return new JsonpResult
            {
                Data = _cusComboHelpListService.GetPersonWithProcess(searchTerm, pageSize, pageNum, filter),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cusComboHelpListService.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult SetSinglePerson(int Ids)
        {
            return Json(_cusComboHelpListService.GetJobWorkerById(Ids));
        }
    }
}