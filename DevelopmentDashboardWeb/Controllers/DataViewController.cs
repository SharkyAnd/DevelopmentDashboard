using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNet.SignalR;
using System.Web.Services;
using DevelopmentDashboardCore.Models;
using DevelopmentDashboardWeb.Models;
using System.Globalization;
using AuthenticationProvider.Filters;

namespace DevelopmentDashboardWeb.Controllers
{

    public class DataViewController : BaseController
    {
        DevelopmentDashboardCore.AppData.DataViewProvider dataViewProvider = new DevelopmentDashboardCore.AppData.DataViewProvider();
        [SecurableAction]
        public ActionResult Sessions()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult ActiveSessions()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult Revisions()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult LocalBuilds()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult Publications()
        {
            return View();
        }

        
        [HttpGet]
        public JsonResult GetActiveSessionsAdditionalParameters()
        {
            return Json(DevelopmentDashboardCore.WebConfigurations.DDWebConfig.Instance.PageAutoUpdatePeriodInMinutes * 60000, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult GetSessionDetailJson(string _sessionsIDsString)
        {
            string[] _sessionsIDs = null;
            if (_sessionsIDsString.EndsWith(","))
                _sessionsIDs = _sessionsIDsString.Remove(_sessionsIDsString.Length - 1, 1).Split(',');
            else
                _sessionsIDs = _sessionsIDsString.Split(',');
            var model = dataViewProvider.GetSessionsActivityProfiles(_sessionsIDs).Select(sap => { sap.ChunkPoint = Convert.ToDateTime(sap.ChunkPoint.ToString("HH:mm")); return sap; }).ToList();
            return new JsonResult()
            {
                Data = model.Select(m => new
                {
                    SessionId = m.SessionId,
                    Position = m.Position,
                    ChunkPoint = m.ChunkPoint.ToJson(),
                    UserName = m.UserName
                }),
                MaxJsonLength = Int32.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [WebMethod]
        
        public JsonResult GetActiveSessionsJson()
        {
            var model = dataViewProvider.GetSessions(DevelopmentDashboardCore.AppData.DataViewProvider.DisplaySessionsMode.Active).Select(s => new ActiveSession(s)).Select(s => new
            {
                Id = s.Id,
                UserName = s.UserName,
                MachineName = s.MachineName,
                SessionBegin = s.SessionBegin.Value.ToJson(),
                SessionEnd = s.SessionEnd.HasValue ? s.SessionEnd.Value.ToJson() : 0,
                LastInputTime = s.LastInputTime.HasValue ? s.LastInputTime.Value.ToJson() : 0,
                SessionState = s.SessionState,
                ActiveHours = s.ActiveHours,
                UtcOffset = s.UtcOffset,
                ClientName = s.ClientName,
                ClientDisplayDetails = s.ClientDisplayDetails,
                ClientReportedIpAddress = s.ClientIPAddress,
                ClientBuildNumber = s.ClientBuildNumber,
                ActiveTime = s.ActiveTime,
                LinearHours = s.LinearHours,
                UserActivity = s.UserActivity
            });

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        
        public JsonResult GetSessionsJson()
        {
            var model = dataViewProvider.GetSessions(DevelopmentDashboardCore.AppData.DataViewProvider.DisplaySessionsMode.All).Select(s => new
                {
                    Id = s.Id,
                    UserName = s.UserName,
                    MachineName = s.MachineName,
                    SessionBegin = s.SessionBegin.Value.ToJson(),
                    SessionEnd = s.SessionEnd.HasValue ? s.SessionEnd.Value.ToJson() : 0,
                    LastInputTime = s.LastInputTime.HasValue ? s.LastInputTime.Value.ToJson() : 0,
                    SessionState = s.SessionState,
                    ActiveHours = s.ActiveHours,
                    UtcOffset = s.UtcOffset,
                    ClientName = s.ClientName,
                    ClientDisplayDetails = s.ClientDisplayDetails,
                    ClientReportedIpAddress = s.ClientIPAddress,
                    ClientBuildNumber = s.ClientBuildNumber,
                    ActiveTime = s.ActiveTime,
                    LinearHours = s.LinearHours
                });

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetAllRevisionsJson()
        {
            var model = dataViewProvider.GetRevisions().Select(rev => new
            {
                Id = rev.Id,
                RepositoryName = rev.RepositoryName,
                Moment = rev.Moment.ToJson(),
                RevisionNumber = rev.RevisionNumber,
                RevisionMessage = rev.RevisionMessage,
                UserId = rev.UserId,
                UserName = rev.UserName
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetAllLocalBuildsJson()
        {
            var model = dataViewProvider.GetLocalBuilds().Select(lb => new
            {
                Id = lb.Id,
                MachineName = lb.MachineName,
                Moment = lb.Moment.Value.ToJson(),
                RevisionNumber = lb.RevisionNumber,
                ErrorsCount = lb.ErrorsCount,
                Comment = lb.Comment,
                BuildPath = lb.BuildPath,
                Project = lb.Project,
                UserName = lb.UserName,
                RevisionAuthor = lb.RevisionAuthor,
                RevisionMessage = lb.RevisionMessage
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetAllPublicationsJson()
        {
            var model = dataViewProvider.GetPublications().Select(pub => new
            {
                Id = pub.Id,
                SourceMachine = pub.SourceMachine,
                TargetMachine = pub.TargetMachine,
                Moment = pub.Moment.Value.ToJson(),
                RevisionNumber = pub.RevisionNumber,
                ProjectFullPath = pub.ProjectFullPath,
                Url = pub.Url,
                Comment = pub.Comment,
                Project = pub.Project,
                UserName = pub.UserName,
                RevisionAuthor = pub.RevisionAuthor,
                RevisionMessage = pub.RevisionMessage
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
