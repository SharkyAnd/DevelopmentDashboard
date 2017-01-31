using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.Models;
using System.Data;
using Utils;
using System.Globalization;
using DevelopmentDashboardCore.WebConfigurations;

namespace DevelopmentDashboardCore.AppData
{
    public class MainPageProvider
    {
        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);
        public IEnumerable<MainPageUser> GetMainPageUsers()
        {
            string query = @"SELECT DISTINCT u.UserName, 
                            (SELECT SUM(DISTINCT ActiveHours)
	                            FROM Logins 
	                            WHERE SessionBegin > FORMAT(GETDATE(), 'yyyy-MM-dd 00:00:00') 
	                            AND SessionBegin <= FORMAT(GETDATE(), 'yyyy-MM-dd 23:59:59') AND UserName = u.UserName) AS DayActiveHours,
                            (SELECT SUM(DISTINCT l.ActiveHours) 
	                            FROM Logins l 
	                            WHERE  SessionBegin > DATEADD(wk,DATEDIFF(wk,0,GETDATE()-1),0) 
	                            AND SessionBegin <= FORMAT(GETDATE(), 'yyyy-MM-dd 23:59:59') AND l.UserName = u.UserName) AS WeekActiveHours,
                            (SELECT TOP 1 LastInputTime FROM Logins WHERE UserName = u.UserName ORDER BY LastInputTime desc) AS LastInputTime,
                            (SELECT TOP 1 ClientUtcOffset FROM Logins WHERE UserName = u.UserName ORDER BY LastInputTime desc) AS LastInputTimeUtcOffset,
                            (SELECT COUNT(DISTINCT r.Id) FROM Revisions r
	                            WHERE Moment > DATEADD(wk,DATEDIFF(wk,0,GETDATE()-1),0) 
	                            AND Moment <= FORMAT(GETDATE(), 'yyyy-MM-dd 23:59:59') AND r.UserId = u.Id ) AS WeekCommits
                        FROM Users u
                        LEFT JOIN Logins l ON u.UserName = l.UserName
                        ORDER BY LastInputTime desc";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new MainPageUser
            {
                UserName = r["UserName"].ToString(),
                DayActiveHours = r["DayActiveHours"] == DBNull.Value ? 0 : Convert.ToDouble(r["DayActiveHours"]),
                WeekActiveHours = r["WeekActiveHours"] == DBNull.Value ? 0 : Convert.ToDouble(r["WeekActiveHours"]),
                LastInputTime = r["LastInputTime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["LastInputTime"]),
                LastInputTimeUtcOffset = r["LastInputTimeUtcOffset"] == DBNull.Value ? 0 : Convert.ToInt32(r["LastInputTimeUtcOffset"]),
                WeekCommitsCount = r["WeekCommits"] == DBNull.Value ? 0 : Convert.ToInt32(r["WeekCommits"]),
            }).Select(mpu =>
            {
                if (mpu.LastInputTime == DateTime.MinValue)
                {
                    mpu.LastInputTime = null;
                    mpu.Status = "red";
                }
                else
                {
                    mpu.LastInputTime = mpu.LastInputTime.Value.AddHours(-mpu.LastInputTimeUtcOffset);

                    if ((AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, mpu.LastInputTimeUtcOffset).Value - 
                        mpu.LastInputTime.Value.AddHours(mpu.LastInputTimeUtcOffset)).TotalMinutes < 
                        DDWebConfig.Instance.CheckActiveStatusTimeInMinutes)
                        mpu.Status = "green";
                    else if ((AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, mpu.LastInputTimeUtcOffset).Value - 
                        mpu.LastInputTime.Value.AddHours(mpu.LastInputTimeUtcOffset)).TotalDays > DDWebConfig.Instance.YellowStatusUserAbsencePeriodInDays && 
                        (AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, mpu.LastInputTimeUtcOffset).Value
                        - mpu.LastInputTime.Value.AddHours(mpu.LastInputTimeUtcOffset)).TotalDays < DDWebConfig.Instance.RedStatusUserAbsencePeriodInDays)
                        mpu.Status = "yellow";
                    else if ((AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, mpu.LastInputTimeUtcOffset).Value -
                        mpu.LastInputTime.Value.AddHours(mpu.LastInputTimeUtcOffset)).TotalDays > DDWebConfig.Instance.RedStatusUserAbsencePeriodInDays)
                        mpu.Status = "red";
                }
                    return mpu;
            });
        }

        public IEnumerable<LastSession> GetLastSessions()
        {
            string query =@"SELECT TOP 10 l.Id, l.UserName, l.SessionBegin, l.SessionEnd, l.ActiveHours, l.LastInputTime, l.ClientUtcOffset, l.SessionState FROM Logins l ORDER BY l.LastInputTime desc";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new LastSession
            {
                Id = Convert.ToInt32(r["Id"]),
                UserName = r["UserName"].ToString(),
                SessionBegin = Convert.ToDateTime(r["SessionBegin"]),
                SessionEnd = r["SessionEnd"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["SessionEnd"]),
                ActiveHours = Convert.ToDouble(r["ActiveHours"]),
                ClientUtcOffset = Convert.ToDouble(r["ClientUtcOffset"]),
                LastInputTime = r["LastInputTime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["LastInputTime"]),
                SessionState = r["SessionState"].ToString()
            }).Select(ls =>
            {
                ls.SessionBegin = ls.SessionBegin.Value.AddHours(-ls.ClientUtcOffset);

                if (ls.SessionEnd == DateTime.MinValue)
                    ls.SessionEnd = null;
                else
                    ls.SessionEnd = ls.SessionEnd.Value.AddHours(-ls.ClientUtcOffset);
                if (ls.LastInputTime == DateTime.MinValue)
                    ls.LastInputTime = null;
                else
                {
                    ls.LastInputTime = ls.LastInputTime.Value.AddHours(-ls.ClientUtcOffset);

                    if ((AppData.DateTimeConvertUtils.ConvertTimeByUtcOffset(DateTime.Now, ls.ClientUtcOffset).Value - 
                        ls.LastInputTime.Value.AddHours(ls.ClientUtcOffset)).TotalMinutes <
                        DDWebConfig.Instance.CheckActiveStatusTimeInMinutes)
                        ls.Status = "green";                  
                }
                return ls;
            });
        }

        public IEnumerable<LastCommit> GetLastCommits()
        {
            string query = @"SELECT TOP 10 r.Moment, u.UserName, r.RevisionNumber, r.RevisionMessage
                             FROM Revisions r
                             LEFT JOIN Users u ON r.UserId=u.Id
                             ORDER BY r.Moment desc";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => new LastCommit
            {
                UserName = r["UserName"].ToString(),
                Moment = r["Moment"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["Moment"]),
                RevisionNumber = r["RevisionNumber"] == DBNull.Value ? null : r["RevisionNumber"].ToString(),
                RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : r["RevisionMessage"].ToString()
            });
        }
    }
}
