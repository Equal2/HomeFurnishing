using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Quartz;
using Schedules.SchedulerClasses;
using Quartz.Impl;
using System.Web.Caching;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Schedules
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string DummyCacheItemKey = "GagaGuguGigi";
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Scheduler.Start();

            RegisterCacheEntry();
        }
        private bool RegisterCacheEntry()
        {

            if (null != HttpContext.Current.Cache[DummyCacheItemKey]) return false;


            HttpContext.Current.Cache.Add(DummyCacheItemKey, "Test", null,
                DateTime.MaxValue, TimeSpan.FromMinutes(2),
                CacheItemPriority.Normal,
                new CacheItemRemovedCallback(CacheItemRemovedCallback));

            return true;
        }
        public void CacheItemRemovedCallback(string key,
            object value, CacheItemRemovedReason reason)
        {
           // Debug.WriteLine("Cache item callback: " + DateTime.Now.ToString());

            // Do the service works

            
            HitPage();
        }


        private const string DummyPageUrl = "http://192.168.2.110:81/";
        //private const string DummyPageUrl = "https://localhost:44312/";

        private void HitPage()
        {

            WebClient client = new WebClient();
            client.DownloadData(DummyPageUrl);
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            // If the dummy page is hit, then it means we want to add another item

            // in cache

            if (HttpContext.Current.Request.Url.ToString() == DummyPageUrl)
            {
                // Add the item in cache and when succesful, do the work.
                DoSomeFileWritingStuff();
                RegisterCacheEntry();

            }
        }

        private void DoSomeFileWritingStuff()
        {
            Debug.WriteLine("Writing to file...");

            try
            {
                using (StreamWriter writer =
                 new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine("Cache Callback: {0}", DateTime.Now);
                    writer.Close();
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }

            Debug.WriteLine("File write successful");
        }
    }
}
