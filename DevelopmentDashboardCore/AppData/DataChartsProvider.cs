using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using DevelopmentDashboardCore.Models;
using System.Data;
using System.IO;

namespace DevelopmentDashboardCore.AppData
{
    public class DataChartsProvider
    {
        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);

        public IEnumerable<UserActiveHours> GetUsersActiveHours(DateTime dateFrom, DateTime dateTo, string userNames)
        {
            List<UserActiveHours> usersActiveHours = new List<UserActiveHours>();

            string query = @"SELECT UserName, SessionBegin, SessionEnd, ActiveHours, ClientUtcOffset
                            FROM [DevelopmentDashboard].[dbo].[Logins]
                            WHERE SessionBegin BETWEEN @StartDate AND @EndDate AND UserName IN 
                            (" + String.Join(",", userNames.Split(',').Select(un => { string newUserName = "'" + un + "'"; return newUserName; }).ToArray()) + ")";

            DataTable dt = dbu.ExecuteDataTable(query, new Dictionary<string, object>
            {
                { "@StartDate", dateFrom },
                { "@EndDate", dateTo }
            });

            usersActiveHours = dt.AsEnumerable().Select(r => new UserActiveHours
            {
                UserName = r["UserName"].ToString(),
                SessionBegin = Convert.ToDateTime(r["SessionBegin"]),
                SessionEnd = Convert.ToDateTime(r["SessionEnd"]),
                ActiveHours = Convert.ToDouble(r["ActiveHours"])
            }).ToList();

            return usersActiveHours;
        }

        public IEnumerable<ConnectedAndActiveSessionModel> BuildConnectedAndActiveSessionsModel(DateTime dateFrom, DateTime dateTo, int interval, string groupName, string userNames)
        {
            List<ConnectedAndActiveSessionModel> connectedAndActiveSessions = new List<ConnectedAndActiveSessionModel>();

            DataTable userNamesDT = new DataTable();
            userNamesDT.Columns.Add("Value", typeof(string));

            foreach (string userName in userNames.Split(','))
            {
                DataRow row = userNamesDT.NewRow();
                row["Value"] = userName;

                userNamesDT.Rows.Add(row);
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@DateFrom", dateFrom);
            parameters.Add("@DateTo", dateTo);
            parameters.Add("@Interval", interval);
            parameters.Add("@GroupName", groupName);
            parameters.Add("@UserNames", userNamesDT);

            DataTable dt = dbu.ExecuteDataTable(@"GetGroupedIntervals", parameters, true);

            connectedAndActiveSessions = dt.AsEnumerable().Select(r=> new ConnectedAndActiveSessionModel
            {
                Moment = Convert.ToDateTime(r["ChunkInterval"]),
                ActiveSessions = Convert.ToInt32(r["Active"]),
                ConnectedSessions = Convert.ToInt32(r["Connected"])
            }).ToList();

            return connectedAndActiveSessions;
        }

        public string[] GetMomentSessions(DateTime DateFrom, DateTime DateTo, DateTime ChunkStartMoment, DateTime ChunkEndMoment, string userNames, bool returnActive)
        {
            string chunkStartString = ChunkStartMoment.ToString("yyyy-MM-dd HH:mm:ss");
            string chunkEndString = ChunkEndMoment.ToString("yyyy-MM-dd HH:mm:ss");

            string dateFromString = DateFrom.ToString("yyyy-MM-dd HH:mm:ss");
            string dateToString = DateTo.ToString("yyyy-MM-dd HH:mm:ss");

            string query = @"SELECT DISTINCT CONVERT(varchar(50),l.Id) +' - '+l.UserName+' ('+l.MachineName+')' AS SessionId FROM SessionActivityProfiles sap
                            LEFT JOIN Logins l ON sap.SessionId = l.Id
                            WHERE ChunkBegin > '" + chunkStartString + "' AND ChunkBegin < '" + chunkEndString + "' " + (returnActive ? "AND IsUserActive = 1" : "") + " AND l.UserName IN (" + userNames + ")  AND " +
                            "SessionId IN  (SELECT Id FROM Logins WHERE SessionBegin >= '" + dateFromString + "' " +
                            "AND (SessionEnd < '" + dateToString + "' OR SessionBegin < '" + dateToString + "'))";

            DataTable dt = dbu.ExecuteDataTable(query, /*new Dictionary<string, object> 
            {
                {"@StartMoment", ChunkStartMoment},
                {"@EndMoment", ChunkEndMoment},
                {"@DateFrom", DateFrom},
                {"@DateTo", DateTo}
            }*/null);

            return dt.AsEnumerable().Select(r => r["SessionId"].ToString()).ToArray();
        }

        public IEnumerable<ConnectedAndActiveSessionModel> GetSessionsFrofileStatistics(DateTime dateFrom, DateTime dateTo, string userNames, string machineNames, string aggregationMode, string aggregationInterval)
        {
            List<ConnectedAndActiveSessionModel> sessionsProfileStatistics = new List<ConnectedAndActiveSessionModel>();

            if (string.IsNullOrEmpty(userNames) || string.IsNullOrEmpty(machineNames))
                return sessionsProfileStatistics;

            DataTable userNamesDT = new DataTable();
            userNamesDT.Columns.Add("Value", typeof(string));

            DataTable machineNamesDT = new DataTable();
            machineNamesDT.Columns.Add("Value", typeof(string));

            foreach (string userName in userNames.Split(','))
            {
                DataRow row = userNamesDT.NewRow();
                row["Value"] = userName;

                userNamesDT.Rows.Add(row);
            }

            foreach (string machineName in machineNames.Split(','))
            {
                DataRow row = machineNamesDT.NewRow();
                row["Value"] = machineName;

                machineNamesDT.Rows.Add(row);
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@DateFrom", dateFrom);
            parameters.Add("@DateTo", dateTo);
            parameters.Add("@UserNames", userNamesDT);
            parameters.Add("@MachineNames", machineNamesDT);
            switch (aggregationInterval)
            {
                case "minute":
                    parameters.Add("@Interval", 1);
                    parameters.Add("@GroupName", "minute");
                    break;
                case "15 min":
                    parameters.Add("@Interval", 15);
                    parameters.Add("@GroupName", "minute");
                    break;
                case "hour":
                    parameters.Add("@Interval", 1);
                    parameters.Add("@GroupName", "hour");
                    break;
            }

            DataTable dt = dbu.ExecuteDataTable(@"GetGroupedIntervalsAdd", parameters, true);

            sessionsProfileStatistics = dt.AsEnumerable().Select(r => new ConnectedAndActiveSessionModel
            {
                Moment = Convert.ToDateTime(r["ChunkInterval"]),
                ActiveSessions = Convert.ToInt32(r["Active"]),
                ConnectedSessions = Convert.ToInt32(r["Connected"])
            }).ToList();

            return sessionsProfileStatistics;
        }

        private DataTable GetGroupedIntervalsFromDB(Dictionary<string, object> parameters)
        {
            string query = @"GetGroupedIntervalsAdd";
            DataTable dt = dbu.ExecuteDataTable(query, parameters, true);
            return dt;
        }

        private DataTable GetSessionsIdsFromDB(DateTime dateFrom, DateTime dateTo, string userNames, string machineNames)
        {
            string query = @"SELECT Id FROM Logins WHERE UserName IN(" + userNames + ") AND MachineName IN(" + machineNames + ") AND SessionBegin >= @StartPeriod AND (SessionEnd <= @EndPeriod OR SessionBegin <= @EndPeriod)";
            DataTable SessionIds = dbu.ExecuteDataTable(query, new Dictionary<string, object>
            {
                {"@StartPeriod", dateFrom},
                {"@EndPeriod", dateTo}
            });

            return SessionIds;
        }

        public IEnumerable<MomentSessionProfile> GetMomentSessionsProfiles(DateTime dateFrom, DateTime dateTo, string userNames, string machineNames,
            string aggregationMode, string aggregationInterval, bool returnActive)
        {
            List<MomentSessionProfile> sessionsProfileStatistics = new List<MomentSessionProfile>();

            if (string.IsNullOrEmpty(userNames) || string.IsNullOrEmpty(machineNames))
                return sessionsProfileStatistics;

            DataTable userNamesDT = new DataTable();
            userNamesDT.Columns.Add("Value", typeof(string));

            DataTable machineNamesDT = new DataTable();
            machineNamesDT.Columns.Add("Value", typeof(string));

            foreach (string userName in userNames.Split(','))
            {
                DataRow row = userNamesDT.NewRow();
                row["Value"] = userName;

                userNamesDT.Rows.Add(row);
            }

            foreach (string machineName in machineNames.Split(','))
            {
                DataRow row = machineNamesDT.NewRow();
                row["Value"] = machineName;

                machineNamesDT.Rows.Add(row);
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@DateFrom", dateFrom);
            parameters.Add("@DateTo", dateTo);
            parameters.Add("@UserNames", userNamesDT);
            parameters.Add("@MachineNames", machineNamesDT);
            switch (aggregationInterval)
            {
                case "minute":
                    parameters.Add("@Interval", 1);
                    parameters.Add("@GroupName", "minute");
                    break;
                case "15 min":
                    parameters.Add("@Interval", 15);
                    parameters.Add("@GroupName", "minute");
                    break;
                case "hour":
                    parameters.Add("@Interval", 1);
                    parameters.Add("@GroupName", "hour");
                    break;
            }
            
            string query = @"GetMomentSessionProfiles";
            DataTable dt = dbu.ExecuteDataTable(query, parameters, true);

            sessionsProfileStatistics = dt.AsEnumerable().Select(r =>
            {
                MomentSessionProfile model = new MomentSessionProfile();
                model.Moment = Convert.ToDateTime(r["ChunkInterval"]);
                model.UserId = r["UserId"].ToString();
                if(!returnActive)
                    model.SessionsCount = Convert.ToInt32(r["Connected"]);
                else
                    model.SessionsCount = Convert.ToInt32(r["Active"]);
                return model;
            }).ToList();

            return sessionsProfileStatistics;
        }
    }
}
