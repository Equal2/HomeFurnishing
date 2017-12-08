using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace MDashBoard
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
             //ApplicationDbContext context = new Data.Models.ApplicationDbContext();
             //var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
             //var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            SqlDependency.Start(ConfigurationManager.ConnectionStrings["LoginDB"].ToString());
        }

        //public static ApplicationDbContext ApplicationDbContext
        //{
        //    get
        //    {
        //        return DependencyResolver.Current.GetService<ApplicationDbContext>();
        //    }
        //}

        public override void Init()
        {
            base.Init();
            try
            {
                // Get the app name from config file...
                string appName = ConfigurationManager.AppSettings["ApplicationName"];

                if (string.IsNullOrEmpty(appName))
                {
                    throw new Exception("Application Name is not set in HR Project");
                }
                else if (!string.IsNullOrEmpty(appName))
                {
                    foreach (string moduleName in this.Modules)
                    {
                        IHttpModule module = this.Modules[moduleName];
                        SessionStateModule ssm = module as SessionStateModule;
                        if (ssm != null)
                        {
                            FieldInfo storeInfo = typeof(SessionStateModule).GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic);
                            SessionStateStoreProviderBase store = (SessionStateStoreProviderBase)storeInfo.GetValue(ssm);
                            if (store == null) //In IIS7 Integrated mode, module.Init() is called later
                            {
                                FieldInfo runtimeInfo = typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.Static | BindingFlags.NonPublic);
                                HttpRuntime theRuntime = (HttpRuntime)runtimeInfo.GetValue(null);
                                FieldInfo appNameInfo = typeof(HttpRuntime).GetField("_appDomainAppId", BindingFlags.Instance | BindingFlags.NonPublic);
                                appNameInfo.SetValue(theRuntime, appName);
                            }
                            else
                            {
                                Type storeType = store.GetType();
                                if (storeType.Name.Equals("OutOfProcSessionStateStore"))
                                {
                                    FieldInfo uribaseInfo = storeType.GetField("s_uribase", BindingFlags.Static | BindingFlags.NonPublic);
                                    uribaseInfo.SetValue(storeType, appName);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
