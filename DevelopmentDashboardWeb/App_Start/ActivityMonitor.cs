using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace DevelopmentDashboardWeb.App_Start
{
    public class ActivityMonitor
    {
        DevelopmentDashboardCore.AppData.DataViewProvider dataViewProvider = new DevelopmentDashboardCore.AppData.DataViewProvider();

        private static ActivityMonitor instance;
        private ActivityMonitor() { }

        public static ActivityMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ActivityMonitor();
                }
                return instance;
            }
        }

        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        /// <summary>
        /// Обработка запроса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="type"></param>
        public void EventHandling(object sender, EventArgs e)
        {
            DateTime moment = DateTime.Now;

            Dictionary<string, string> info = new Dictionary<string, string>();

            info.Add("RawUrl", HttpContext.Current.Request.RawUrl); 
            info.Add("UserHostAddress", HttpContext.Current.Request.UserHostAddress);
            info.Add("OriginalString", HttpContext.Current.Request.Url.OriginalString);
            info.Add("AbsoluteUri", HttpContext.Current.Request.Url.AbsoluteUri);
            string urlReferrer = HttpContext.Current.Request.UrlReferrer != null ?
                HttpContext.Current.Request.UrlReferrer.AbsoluteUri : null;
            info.Add("UrlReferrer", urlReferrer);
            info.Add("UserAgent", HttpContext.Current.Request.UserAgent);

            if (HttpContext.Current.User != null)
                info.Add("UserName", HttpContext.Current.User.Identity.Name);
            else info.Add("UserName", "NULL");
            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(GetIPAddress());
                int counter = 0;
                foreach (IPAddress ip in ips)
                {
                    info.Add("Remote_Addr[" + counter + "]", ip.ToString());
                    counter++; 
                }
                
            }
            catch (Exception ex)
            {
            } 

            dataViewProvider.WriteActivityMessage(moment, DevelopmentDashboardCore.AppData.ActivityType.Application_EndRequest, info);
        }
    }

}