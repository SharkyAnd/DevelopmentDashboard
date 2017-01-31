using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevelopmentDashboardWeb.Models;
using System.Globalization;
using System.IO;
using AuthenticationProvider.Filters;
using System.Dynamic;

namespace DevelopmentDashboardWeb.Controllers
{
    public class DataChartController : BaseController
    {
        DevelopmentDashboardCore.DevelopmentDashboardCore ddCore = new DevelopmentDashboardCore.DevelopmentDashboardCore();
        DevelopmentDashboardCore.AppData.DataChartsProvider dataChartsProvider = new DevelopmentDashboardCore.AppData.DataChartsProvider();

        [SecurableAction]
        public ActionResult ConnectedAndActive()
        {
            return View();
        }
        [SecurableAction]
        public ActionResult SessionProfileStatistics()
        {
            return View();
        }

        [SecurableAction]
        public ActionResult ActiveHoursChart()
        {
            return View();
        }

        [HttpGet]
        public JsonResult SessionProfileStatGetAdditionalDataJson()
        {
            var model = new
            {
                PeriodBegin = DateTime.Now.AddDays(-7).ToJson(),
                PeriodEnd = DateTime.Now.ToJson(),
                UserNamesByGroup = ddCore.GetUserNamesByGroups(),
                MachineNames = ddCore.GetMachineNames()
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetMomentSessionsJson(string dateFrom, string dateTo, string moment, int interval, string group, string sessionsType, string userNames, int clientOffset)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo);

            DateTime ChunkStartMoment = DateTime.Parse(moment, CultureInfo.CurrentCulture, DateTimeStyles.None).AddHours(clientOffset);
            DateTime? ChunkEndMoment = null;
            userNames = String.Join(",", userNames.Split(',').Select(un => { string newUserName = "'" + un + "'"; return newUserName; }).ToArray());
            switch (group)
            {
                case "minute":
                    ChunkEndMoment = ChunkStartMoment.AddMinutes(interval);
                    break;
                case "hour":
                    ChunkEndMoment = ChunkStartMoment.AddHours(interval);
                    break;
                case "day":
                    ChunkEndMoment = ChunkStartMoment.AddDays(interval);
                    break;
            }
            /*using (StreamWriter sw = new StreamWriter(@"C:\Logs\log.txt"))
            {
                sw.WriteLine(ChunkStartMoment.ToString("dd.MM.yyyy HH:mm:ss"));
                sw.WriteLine(ChunkEndMoment.Value.ToString("dd.MM.yyyy HH:mm:ss"));
            }*/
            var model = dataChartsProvider.GetMomentSessions(DateFrom, DateTo, ChunkStartMoment, ChunkEndMoment.Value, userNames, sessionsType == "Active Sessions" ? true : false);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetUsersActiveHoursJson(string dateFrom, string dateTo, string userNames, string aggregationInterval)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo);

            var usersActiveHours = dataChartsProvider.GetUsersActiveHours(DateFrom, DateTo, userNames);



            if(aggregationInterval == "Day")
            {
                var model = usersActiveHours.Select(uah => { uah.SessionBegin = Convert.ToDateTime(uah.SessionBegin.ToString("HH:mm")); return uah; })
                    .GroupBy(uah => new { uah.SessionBegin, uah.UserName })
                    .Select(uah => new
                    {
                        SessionBegin = uah.Key.SessionBegin.ToJson(),
                        UserName = uah.Key.UserName,
                        ActiveHours = uah.Sum(ah=>ah.ActiveHours)
                    });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else if(aggregationInterval == "Week")
            {
                /*var model = usersActiveHours.OrderBy(uah => uah.SessionBegin).Select(uah => new
                {
                    SessionBegin = uah.SessionBegin.ToString("dddd HH:mm"),
                    UserName = uah.UserName,
                    ActiveHours = uah.ActiveHours
                })
                   .GroupBy(uah => new { uah.SessionBegin, uah.UserName })
                   .Select(uah => new
                   {
                       SessionBegin = uah.Key.SessionBegin,
                       UserName = uah.Key.UserName,
                       ActiveHours = uah.Sum(ah => ah.ActiveHours)
                   }).ToList();*/

                var model = usersActiveHours.OrderBy(uah => uah.SessionBegin).Select(uah => new { SessionBegin = Convert.ToDateTime(uah.SessionBegin.ToString("dd HH:mm")) }).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var model = usersActiveHours.Select(uah => new
                {
                    SessionBegin = uah.SessionBegin.ToString("MMMM HH:mm"),
                    UserName = uah.UserName,
                    ActiveHours = uah.ActiveHours
                })
                   .GroupBy(uah => new { uah.SessionBegin, uah.UserName })
                   .Select(uah => new
                   {
                       SessionBegin = uah.Key.SessionBegin,
                       UserName = uah.Key.UserName,
                       ActiveHours = uah.Sum(ah => ah.ActiveHours)
                   });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetSessionsProfileStatJson(string dateFrom, string dateTo, string userNames, string machineNames, string aggregationMode, string aggregationOperation, string aggregationInterval)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo);

            //userNames = String.Join(",", userNames.Split(',').Select(un => { string newUserName = "'" + un + "'"; return newUserName; }).ToArray());
            //machineNames = String.Join(",", machineNames.Split(',').Select(mn => { string newMachineName = "'" + mn + "'"; return newMachineName; }).ToArray());
            var sessionProfileStat = dataChartsProvider.GetSessionsFrofileStatistics(DateFrom, DateTo, userNames, machineNames, aggregationMode, aggregationInterval);

            if (aggregationMode == "Daily")
            {
                //var tmp = sessionProfileStat
                //    .Select(sps => { sps.Moment = Convert.ToDateTime(sps.Moment.ToString("HH:mm")); return sps; })
                //    .GroupBy(sps => sps.Moment)
                //    .Select(sps => new
                //    {
                //        Moment = sps.Key,
                //        ActiveSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ActiveSessions) : sps.Sum(s => s.ActiveSessions),
                //        ConnectedSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ConnectedSessions) : sps.Sum(s => s.ConnectedSessions)
                //    }).ToList() ;

                var model = sessionProfileStat
                    .Select(sps => { sps.Moment = Convert.ToDateTime(sps.Moment.ToString("HH:mm")); return sps; })
                    .GroupBy(sps => sps.Moment)
                    .Select(sps => new
                    {
                        Moment = sps.Key.ToJson(),
                        ActiveSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ActiveSessions) : sps.Sum(s => s.ActiveSessions),
                        ConnectedSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ConnectedSessions) : sps.Sum(s => s.ConnectedSessions)
                    });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var dfi = DateTimeFormatInfo.CurrentInfo;
                var model = sessionProfileStat
                    .Select(sps => new
                    {
                        Moment = sps.Moment.ToString("dddd HH:mm"),
                        ActiveSessionsCount = sps.ActiveSessions,
                        ConnectedSessionsCount = sps.ConnectedSessions
                    })
                    .GroupBy(sps => sps.Moment)
                    .Select(sps => new
                    {
                        Moment = sps.Key,
                        ActiveSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ActiveSessionsCount) : sps.Sum(s => s.ActiveSessionsCount),
                        ConnectedSessionsCount = aggregationOperation == "Average" ? sps.Average(s => s.ConnectedSessionsCount) : sps.Sum(s => s.ConnectedSessionsCount)
                    });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetMomentSessionsProfilesJson(string dateFrom, string dateTo, string userNames, string machineNames,
            string aggregationMode, string aggregationOperation, string aggregationInterval, string chunkInterval, bool returnActive, int clientOffset)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo);
            
            //userNames = String.Join(",", userNames.Split(',').Select(un => { string newUserName = "'" + un + "'"; return newUserName; }).ToArray());
            //machineNames = String.Join(",", machineNames.Split(',').Select(mn => { string newMachineName = "'" + mn + "'"; return newMachineName; }).ToArray());
            var sessionProfileStat = dataChartsProvider.GetMomentSessionsProfiles(DateFrom, DateTo, userNames, machineNames, aggregationMode, aggregationInterval, returnActive);
            List<string> sessionsProfileStat = new List<string>();
            if (aggregationMode == "Daily")
            {
                DateTime chunk = DateTime.Parse(chunkInterval, CultureInfo.CurrentCulture, DateTimeStyles.None).AddHours(clientOffset);

                sessionsProfileStat = sessionProfileStat
                    .Select(sps => { sps.Moment = Convert.ToDateTime(sps.Moment.ToString("HH:mm")); return sps; })
                    .GroupBy(sps => new { sps.UserId, sps.Moment })
                    .Where(sps => sps.Key.Moment == chunk)
                    .Select(sps => string.Format("{0} - {1} session(s)", sps.Key.UserId, aggregationOperation == "Average" ? Math.Round(sps.Average(s => s.SessionsCount), 4).ToString() : sps.Sum(s => s.SessionsCount).ToString()))
                    .ToList();
            }
            else
            {
                sessionsProfileStat = sessionProfileStat
                    .Select(sps => new { Moment = sps.Moment.ToString("dddd HH:mm"), UserId = sps.UserId, SessionsCount = sps.SessionsCount })
                    .GroupBy(sps => new { sps.UserId, sps.Moment })
                    .Where(sps => sps.Key.Moment == chunkInterval)
                    .Select(sps => string.Format("{0} - {1} session(s)", sps.Key.UserId, aggregationOperation == "Average" ? Math.Round(sps.Average(s => s.SessionsCount), 4).ToString() : sps.Sum(s => s.SessionsCount).ToString()))
                    .ToList();
            }

            return Json(sessionsProfileStat, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetConnectedAndActiveParametersJson()
        {
            return Json(new
            {
                PeriodBegin = DateTime.Now.AddDays(-7).ToJson(),
                PeriodEnd = DateTime.Now.ToJson(),
                UserNamesByGroup = ddCore.GetUserNamesByGroups()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUsersActiveHoursParametersJson()
        {
            return Json(new
            {
                PeriodBegin = DateTime.Now.AddMonths(-1).ToJson(),
                PeriodEnd = DateTime.Now.ToJson(),
                UserNamesByGroup = ddCore.GetUserNamesByGroups()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetConnectedAndActiveSessionsJson(string dateFrom, string dateTo, int interval, string groupName, string userNames)
        {
            DateTime DateFrom = Convert.ToDateTime(dateFrom);
            DateTime DateTo = Convert.ToDateTime(dateTo);

            //userNames = String.Join(",", userNames.Split(',').Select(un => { string newUserName = "'" + un + "'"; return newUserName; }).ToArray());

            var model = dataChartsProvider.BuildConnectedAndActiveSessionsModel(DateFrom, DateTo, interval, groupName, userNames)
                .Select(asm => new
            {
                Moment = asm.Moment.ToJson(),
                ConnectedSessionsCount = asm.ConnectedSessions,
                ActiveSessionsCount = asm.ActiveSessions
            });
            //return Json(model, JsonRequestBehavior.AllowGet);
            return new JsonResult()
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }

    }
}
