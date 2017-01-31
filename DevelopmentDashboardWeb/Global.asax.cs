using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using DevelopmentDashboardWeb.App_Start;
using DevelopmentDashboardWeb.Models;
using DevelopmentDashboardWeb.Global.Binders;

namespace DevelopmentDashboardWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.Add(typeof(DXGridOptions), new GridOptionsJsonModelBinder());

            Utils.LoggingUtils.DefaultLogger = new Utils.DatabaseLogger(DevelopmentDashboardCore.DevelopmentDashboardConfig.Instance.ConnectionString);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            string controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"] as string;
            string action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"] as string;

            List<string> excludeControllers = new List<string>() { "scripts", "content" };

            string requestType = HttpContext.Current.Request.HttpMethod;

            // фиксируем все запросы к методам контроллера, исключая все запросы кроме GET 
            if (!string.IsNullOrEmpty(controller) && !string.IsNullOrEmpty(action) && requestType.ToLower().Contains("get"))
            {
                var isFound = excludeControllers.Where(x => x.Contains(controller.ToLower())).FirstOrDefault() != null;
                if (!isFound)  
                      ActivityMonitor.Instance.EventHandling(sender, e);
            }
        }
    }
}