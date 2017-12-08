using System.Web.Mvc;

namespace Jobs.Areas.Rug
{
    public class RugAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Rug";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Rug_default",
                "Rug/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Jobs.Areas.Rug.Controllers" }
            );
        }
    }
}