using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevelopmentDashboardWeb.Models;
using AuthenticationProvider.Filters;

namespace DevelopmentDashboardWeb.Controllers
{
    public class SummaryController : BaseController
    {
        [SecurableAction]
        public ActionResult ActivitySummary()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult MetricsHistory()
        {
            return View();
        }

        DevelopmentDashboardCore.AppData.SummaryProvider ddSummaryProvider = new DevelopmentDashboardCore.AppData.SummaryProvider();
        DevelopmentDashboardCore.DevelopmentDashboardCore ddCore = new DevelopmentDashboardCore.DevelopmentDashboardCore();

        [HttpGet]
        
        public JsonResult GetMetricsHistoryJson(DXGridOptions options)
        {
            var model = ddSummaryProvider.GetCodeMetricsHistorySimple(options.Take, options.Skip).Select(cmh => new
            {
                Id = cmh.Id,
                CalculationMoment = cmh.CalculationMoment.Value.ToJson(),
                CalculationMachine = cmh.CalculationMachine,
                ProjectGroup = cmh.ProjectGroup,
                RevisionNumber = cmh.RevisionNumber,
                RevisionAuthor = cmh.RevisionAuthor,
                ProjectName = cmh.ProjectName,
                MeasurementTool = cmh.MeasurementTool,
                MetricName = cmh.MetricName,
                MetricValue = cmh.MetricValue,
                RevisionMessage = cmh.RevisionMessage
            });
            //return Json(model, JsonRequestBehavior.AllowGet);

            return new JsonResult()
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }

        public JsonResult GetMetricsHistoryCountJson()
        {
            return Json(ddSummaryProvider.GetCodeMetricsHistoryCount(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        
        public JsonResult GetMetricsHistoryExpandedJson()
        {
            var cdmHistory = ddSummaryProvider.GetCodeMetricsHistoryExpanded();

            var model = new
            {
                MetricsHistoryData = cdmHistory.MetricsHistoryData.Select(mhd => new
                {
                    CalculationMoment = mhd.CalculationMoment.Value.ToJson(),
                    ProjectName = mhd.ProjectName,
                    RevisionNumber = mhd.RevisionNumber,
                    UserName = mhd.UserName,
                    RevisionMessage = mhd.RevisionMessage,
                    MetricValues = mhd.MetricValues
                }),
                MetricNames = cdmHistory.MetricNames
            };

            return new JsonResult()
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }

        [HttpPost]
        
        public JsonResult GetActivitySummaryJson(string dateFrom, string dateTo, string userNames)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo + " 23:59:59");

            var model = ddSummaryProvider.GetActivitySummary(DateFrom, DateTo, userNames).OrderByDescending(asm => asm.Moment).Select(asm => new
            {
                UserName = asm.UserName,
                Moment = asm.Moment.HasValue? asm.Moment.Value.ToJson():0,
                ActiveHours = asm.ActiveHours,
                LinearHours = asm.LinearHours,
                SessionsCount = asm.SessionsCount,
                CommitsCount = asm.CommitsCount,
                PublicationsCount = asm.PublicationsCount,
                LocalBuildsCount = asm.LocalBuildsCount
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        
        public JsonResult ActivitySummaryGetAdditionalDataJson()
        {
            var model = new
            {
                PeriodBegin = DateTime.Now.AddMonths(-1).ToJson(),
                PeriodEnd = DateTime.Now.ToJson(),
                UserNamesByGroup = ddCore.GetUserNamesByGroups()
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

    }
}
