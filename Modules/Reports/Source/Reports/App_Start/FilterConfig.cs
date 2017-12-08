using Reports;
using System.Web;
using System.Web.Mvc;

namespace Reports
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthenticationFilter());
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}
