using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using DevelopmentDashboardWeb.Models;
using AuthenticationProvider.Filters;

namespace DevelopmentDashboardWeb.Controllers
{
    [NonSecurableController]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        DevelopmentDashboardCore.AppData.MainPageProvider mainPageProvider = new DevelopmentDashboardCore.AppData.MainPageProvider();
        ConfigurationUtils.DatabaseConfigProvider ssProvider = new ConfigurationUtils.DatabaseConfigProvider(
            DevelopmentDashboardCore.DevelopmentDashboardConfig.Instance.ConnectionString);
        public JsonResult GetUsersJson()
        {
            var model = mainPageProvider.GetMainPageUsers().Select(mpu => new
                {
                    UserName = mpu.UserName,
                    DayActiveHours = Math.Round(mpu.DayActiveHours, 4),
                    WeekActiveHours = Math.Round(mpu.WeekActiveHours, 4),
                    LastInputTime = mpu.LastInputTime.HasValue ? mpu.LastInputTime.Value.ToJson() : 0,
                    WeekCommitsCount = mpu.WeekCommitsCount,
                    Status = mpu.Status
                }).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLastSessionsJson()
        {
            var model = mainPageProvider.GetLastSessions().Select(ls => new
            {
                Id = ls.Id,
                UserName = ls.UserName,
                SessionBegin = ls.SessionBegin.Value.ToJson(),
                SessionEnd = ls.SessionEnd == null ? 0 : ls.SessionEnd.Value.ToJson(),
                ActiveHours = Math.Round(ls.ActiveHours, 4),
                SessionState = ls.SessionState,
                Status = ls.Status
            }).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLastCommitsJson()
        {
            var model = mainPageProvider.GetLastCommits().Select(lc => new
            {
                UserName = lc.UserName,
                Moment = lc.Moment.Value.ToJson(),
                RevisionNumber = lc.RevisionNumber,
                RevisionMessage = lc.RevisionMessage
            }).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUsersWidgetOptions()
        {
            return Json(new
            {
                yellowStatusValue = DevelopmentDashboardCore.WebConfigurations.DDWebConfig.Instance.YellowStatusUserAbsencePeriodInDays,
                redStatusValue = DevelopmentDashboardCore.WebConfigurations.DDWebConfig.Instance.RedStatusUserAbsencePeriodInDays
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUsersWidgetOptions(string yellowStatusVal, string redStatusVal)
        {
            ssProvider.UpdateSeviceSettingsValue("YellowStatusUserAbsencePeriodInDays", yellowStatusVal);
            ssProvider.UpdateSeviceSettingsValue("RedStatusUserAbsencePeriodInDays", redStatusVal);
            DevelopmentDashboardCore.WebConfigurations.DDWebConfig.Instance.Reset();
            return Json("");
        }
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserLogin()
        {
            return PartialView(CurrentUser);
        }
    }
}
