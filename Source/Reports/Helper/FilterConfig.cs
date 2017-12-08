using Reports.Presentation.Models;
using System.Web;
using System.Web.Mvc;

namespace Reports.Presentation
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
            filters.Add(new AuthenticationFilter());
            filters.Add(new RequestLogFilter());
        }
    }
}
