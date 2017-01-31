using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.Models;
using Utils;
using System.Data;
using System.Globalization;
namespace DevelopmentDashboardCore.AppData
{
    public enum ActivityType
    {
        Application_BeginRequest = 0,
        Application_EndRequest
    }



    public class DataViewProvider
    {
        public enum DisplaySessionsMode
        {
            All,
            Active
        }

        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);

        public IEnumerable<Session> GetSessions(DisplaySessionsMode displaySessionsMode)
        {
            List<Session> sessions = new List<Session>();

            string query = @"select Id, UserName, MachineName, SessionBegin, SessionEnd, ActiveHours, SessionState, ClientUtcOffset, LastInputTime, ClientName, ClientDisplayDetails, ClientReportedIPAddress, ClientBuildNumber FROM Logins ";
            if (displaySessionsMode != DisplaySessionsMode.All)
                query += string.Format("WHERE SessionState = '{0}'", displaySessionsMode.ToString());
            query += " ORDER BY Id DESC";

            DataTable dt = dbu.ExecuteDataTable(query, null);

            sessions = dt.AsEnumerable().Select(r => new Session()
            {
                Id = (int)r["Id"],
                UserName = r["UserName"].ToString(),
                MachineName = r["MachineName"].ToString(),
                SessionBegin = Convert.ToDateTime(r["SessionBegin"]),
                SessionEnd = r["SessionEnd"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["SessionEnd"]),
                ActiveHours = Convert.ToDouble(r["ActiveHours"]),
                SessionState = r["SessionState"].ToString(),
                UtcOffset = Convert.ToDouble(r["ClientUtcOffset"] == DBNull.Value ? 0 : r["ClientUtcOffset"]),
                LastInputTime = r["LastInputTime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["LastInputTime"]),
                ClientName = r["ClientName"] == DBNull.Value ? "-" : r["ClientName"].ToString(),
                ClientDisplayDetails = r["ClientDisplayDetails"] == DBNull.Value ? "-" : r["ClientDisplayDetails"].ToString(),
                ClientIPAddress = r["ClientReportedIPAddress"] == DBNull.Value ? "-" : r["ClientReportedIPAddress"].ToString(),
                ClientBuildNumber = r["ClientBuildNumber"] == DBNull.Value ? 0 : (int)r["ClientBuildNumber"]
            }).Select(s =>
            {
                if (s.SessionEnd == DateTime.MinValue)
                    s.SessionEnd = null;
                else
                    s.SessionEnd = s.SessionEnd.Value.AddHours(-s.UtcOffset);
                if (s.LastInputTime == DateTime.MinValue)
                    s.LastInputTime = null;
                else
                    s.LastInputTime = s.LastInputTime.Value.AddHours(-s.UtcOffset);
                s.SessionBegin = s.SessionBegin.Value.AddHours(-s.UtcOffset);
                s.LinearHours = Math.Round(Convert.ToDouble(
                                s.SessionEnd.HasValue ? (s.SessionEnd.Value - s.SessionBegin.Value).TotalMinutes : (AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, s.UtcOffset).Value - s.SessionBegin.Value).TotalMinutes
                                ) / 60, 4);
                s.ActiveTime = s.LinearHours == 0 ? 0 : s.ActiveHours / s.LinearHours;

                return s;
            }).ToList();

            return sessions;
        }

        public IEnumerable<Revision> GetRevisions()
        {
            string query = "select r.Id as Id, rep.Name as RepositoryName, r.Moment as Moment, r.RevisionNumber as RevisionNumber, r.RevisionMessage as RevisionMessage, u.Id as UserID, u.UserName as UserName " +
                           "from Revisions r " +
                           "left join Users u on r.UserId = u.Id " +
                           "left join Repositories rep on r.RepositoryId = rep.Id " +
                           "where r.UserID IS NOT NULL AND u.UserName IS NOT NULL " +
                           "order by r.Id desc";


            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new Revision()
            {
                Id = (int)r["Id"],
                RepositoryName = (string)r["RepositoryName"],
                Moment = DateTime.Parse(r["Moment"].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None),
                RevisionNumber = (string)r["RevisionNumber"],
                RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : (string)r["RevisionMessage"],
                UserId = (int)r["UserID"],
                UserName = (string)r["UserName"]
            });
        }

        public IEnumerable<LocalBuild> GetLocalBuilds()
        {
            string query = @"SELECT DISTINCT lb.Id AS LBId, lb.Moment, lb.UserName, lb.MachineName, lb.Project, lb.RevisionNumber, u.UserName AS RevisionAuthor, r.RevisionMessage,
                            lb.ErrorCount, lb.Comment, lb.BuildPath
                            FROM [DevelopmentDashboard].[dbo].[LocalBuilds] lb
                            LEFT JOIN Revisions r ON lb.RevisionNumber = r.RevisionNumber
                            LEFT JOIN Users u ON r.UserId = u.Id 
                            ORDER BY lb.Id desc";

            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new LocalBuild()
            {
                Id = (int)r["LBId"],
                MachineName = (string)r["MachineName"],
                Moment = DateTime.Parse(r["Moment"].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None),
                RevisionNumber = r["RevisionNumber"] == DBNull.Value ? null : (string)r["RevisionNumber"],
                ErrorsCount = r["ErrorCount"] == DBNull.Value ? 0 : (int)r["ErrorCount"],
                Comment = r["Comment"] == DBNull.Value ? null : (string)r["Comment"],
                BuildPath = r["BuildPath"] == DBNull.Value ? null : (string)r["BuildPath"],
                Project = r["Project"] == DBNull.Value ? null : (string)r["Project"],
                RevisionAuthor = r["RevisionAuthor"] == DBNull.Value ? null : (string)r["RevisionAuthor"],
                RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : (string)r["RevisionMessage"],
                UserName = (string)r["UserName"]
            });
        }

        public IEnumerable<Publication> GetPublications()
        {
            string query = @"SELECT pub.*, u.UserName AS RevisionAuthor, r.RevisionMessage
                             FROM Publications pub
                             LEFT JOIN Revisions r ON pub.RevisionNumber = r.RevisionNumber
                             LEFT JOIN Users u ON r.UserId = u.Id 
                             ORDER BY pub.Id desc";

            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new Publication()
            {
                Id = (int)r["Id"],
                SourceMachine = (string)r["SourceMachine"],
                TargetMachine = (string)r["TargetMachine"],
                Moment = DateTime.Parse(r["Moment"].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None),
                RevisionNumber = r["RevisionNumber"] == DBNull.Value ? null : (string)r["RevisionNumber"],
                ProjectFullPath = r["ProjectFullPath"] == DBNull.Value ? null : (string)r["ProjectFullPath"],
                Url = r["Url"] == DBNull.Value ? null : (string)r["Url"],
                Comment = r["Comment"] == DBNull.Value ? null : (string)r["Comment"],
                Project = r["Project"] == DBNull.Value ? null : (string)r["Project"],
                UserName = (string)r["UserName"],
                RevisionAuthor = r["RevisionAuthor"] == DBNull.Value ? null : (string)r["RevisionAuthor"],
                RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : (string)r["RevisionMessage"]
            });
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="moment">время наступления события</param>
        /// <param name="type">Тип события</param>
        /// <param name="info"></param>
        public void WriteActivityMessage(DateTime moment, ActivityType type, Dictionary<string, string> info)//(string requiredPage, string userName, DateTime moment, string userHostAddress, string queryString, string userMachineName, string userAgent)
        {
            StringBuilder comment = new StringBuilder();

            try
            {
                var newId = dbu.ExecuteScalarQuery("INSERT INTO ActivityMonitor(Moment, Type)  VALUES(@moment, @type)  SELECT SCOPE_IDENTITY()", new Dictionary<string, object>()
                {
                    {"@moment",  moment.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"@type",  Enum.GetName(typeof(ActivityType), type)}
                });
                if (newId != null)
                {
                    int id = Decimal.ToInt32((Decimal)newId);
                    foreach (var key in info.Keys)
                    {
                        dbu.ExecuteNonQuery(string.Format("INSERT INTO ActivityMonitorValues(ParentId, Property, Value)  VALUES('{0}', '{1}', '{2}')", id.ToString(), key, info[key]), null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtils.DefaultLogger.AddLogMessage(null, MessageType.Error, "Error Operation Insert In SQL, Moment=" + moment.ToString("yyyy-MM-dd HH:mm:ss") + ", Type=" + Enum.GetName(typeof(ActivityType), type), ex.GetType(), ex.Message);
            }
        }

        List<SessionActivityProfile> sessionsActivityProfiles;
        public IEnumerable<SessionActivityProfile> GetSessionsActivityProfiles(string[] sessionIds)
        {
            sessionsActivityProfiles = new List<SessionActivityProfile>();
            int position = 1;
            foreach (string sessionId in sessionIds)
            {
                if (string.IsNullOrEmpty(sessionId))
                    continue;

                string query = @"select l.UserName, sap.ChunkBegin, sap.ChunkEnd, sap.IsUSerActive FROM SessionActivityProfiles sap LEFT JOIN Logins l ON l.id = sap.SessionId WHERE SessionId = @SessionId Order By ChunkBegin";

                DataTable dt = dbu.ExecuteDataTable(query, new Dictionary<string, object>()
                {
                    {"@SessionId", sessionId}
                });
                string sessionUserName = dt.AsEnumerable().Select(r => (string)r["UserName"]).FirstOrDefault();
                string sessionBegin = dt.AsEnumerable().Select(r => Convert.ToDateTime(r["ChunkBegin"]).ToString("dd-MM-yyyy HH:mm")).FirstOrDefault();
                string currentSessionId = string.Format("{0} started at {1}", sessionUserName, sessionBegin);

                sessionsActivityProfiles.AddRange(dt.AsEnumerable().Select(r => new SessionActivityProfile
                {
                    SessionId = currentSessionId,
                    ChunkPoint = Convert.ToDateTime(r["ChunkBegin"]),
                    Position = (bool)r["IsUserActive"] ? position : 0,
                    UserName = (bool)r["IsUserActive"] ? (string)r["UserName"] : null
                }).ToList().Select(sap => { if (sap.Position == 0) sap.Position = null; return sap; }).ToList());
                position += 1;
            }
            return sessionsActivityProfiles;
        }
    }
}
