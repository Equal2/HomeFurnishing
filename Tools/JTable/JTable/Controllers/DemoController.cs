using System.Web.Mvc;
using System.Web.SessionState;
using System.Linq;

namespace CrudPlugInDevelop.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class DemoController : RepositoryBasedController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PagingAndSorting()
        {
            return View();
        }

        public ActionResult SelectingRows()
        {
            return View();
        }

        public ActionResult MasterChild()
        {
            return View();
        }

        public ActionResult UsingWithValidationEngine1()
        {
            return View();            
        }

        public ActionResult UsingWithValidationEngine2()
        {
            return View();
        }

        public ActionResult Theme()
        {
            return View();            
        }

        public ActionResult TurkishLocalization()
        {
            return View();            
        }

        public ActionResult Filtering()
        {
            var cities = _repository.CityRepository.GetAllCities().Select(city => new SelectListItem {Text = city.CityName, Value = city.CityId.ToString()}).ToList();
            cities.Insert(0, new SelectListItem {Selected = true, Text = "All cities", Value = "0"});

            ViewBag.Cities = cities;
            return View();
        }

        public ActionResult ColumnResizing()
        {
            return View();
        }

        public ActionResult ColumnHideShow()
        {
            return View();
        }
    }
}
