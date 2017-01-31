using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.Models;
using Utils;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace DevelopmentDashboardCore
{
    public class DevelopmentDashboardCore
    {
        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);
        
        #region Helpers
        public IEnumerable<UsersByGroup> GetUserNamesByGroups()
        {
            List<UsersByGroup> usersByGroups = new List<UsersByGroup>();
            string query = "SELECT DISTINCT Name FROM Groups ORDER BY Name";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            string[] groups = dt.AsEnumerable().Select(r => r["Name"].ToString()).ToArray();

            foreach (string group in groups)
            {
                query = "SELECT DISTINCT UserName FROM Users u LEFT JOIN UsersInGroups uig ON u.Id = uig.UserId LEFT JOIN Groups g ON uig.GroupId = g.Id WHERE g.Name = @GroupName";
                dt = dbu.ExecuteDataTable(query, new Dictionary<string, object> { { "@GroupName", group } });

                UsersByGroup ubg = new UsersByGroup();               
                ubg.key = group;
                ubg.items = dt.AsEnumerable().Select(r => r["UserName"].ToString()).ToArray();
                usersByGroups.Add(ubg);
            }

            query = @"SELECT DISTINCT l.UserName FROM Logins l
                      LEFT JOIN Users u ON u.UserName = l.UserName
                      LEFT JOIN UsersInGroups uig ON u.Id = uig.UserId
                      WHERE uig.UserId IS NULL";
            dt = dbu.ExecuteDataTable(query, null);

            UsersByGroup noGroup = new UsersByGroup();
            noGroup.key = "No Group";
            noGroup.items = dt.AsEnumerable().Select(r => r["UserName"].ToString()).ToArray();
            usersByGroups.Add(noGroup);

            return usersByGroups;
        }

        public IEnumerable<string> GetMachineNames()
        {
            string query = "SELECT DISTINCT MachineName FROM Logins ORDER BY MachineName";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => r["MachineName"].ToString());
        }

        public DateTime GetStartDate()
        {
            string query = "SELECT TOP 1 SessionBegin FROM Logins ORDER BY SessionBegin";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => Convert.ToDateTime(r["SessionBegin"])).FirstOrDefault();
        }

        public DateTime GetEndDate()
        {
            string query = "SELECT TOP 1 SessionBegin FROM Logins ORDER BY SessionBegin desc";
            DataTable dt = dbu.ExecuteDataTable(query, null);

            return dt.AsEnumerable().Select(r => Convert.ToDateTime(r["SessionBegin"])).FirstOrDefault();
        }

        #endregion
        
    }
}
