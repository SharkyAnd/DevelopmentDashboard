using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.Models;
using System.Data;
using Utils;
using MailTaskAgent;
using System.Net.Mail;
using System.Net;

namespace DevelopmentDashboardCore.AppData
{
    public class SiteAdministrationProvider
    {
        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);
        public UserGroupManagementMatrix GetUsersGroupsMatrix()
        {
            UserGroupManagementMatrix userGroupManagementMatrix = new UserGroupManagementMatrix();
            userGroupManagementMatrix.Groups = new List<string>();
            string query = @"GetUserGroupMatrix";

            DataTable dt = dbu.ExecuteDataTable(query, null, true);

            foreach (DataColumn item in dt.Columns)
            {
                if (item.ColumnName != "UserName")
                    userGroupManagementMatrix.Groups.Add(item.ColumnName);
            }

            userGroupManagementMatrix.UsersInGroups = dt.AsEnumerable().Select(r =>
            {
                UserGroupOwning userGroupOwning = new UserGroupOwning();
                userGroupOwning.IsUserInGroups = new List<bool>();
                userGroupOwning.UserName = r["UserName"] == DBNull.Value ? null : (string)r["UserName"];
                foreach (string group in userGroupManagementMatrix.Groups)
                    userGroupOwning.IsUserInGroups.Add(r[group] == DBNull.Value ? false : true);
                return userGroupOwning;
            }).ToList();

            return userGroupManagementMatrix;
        }

        public UserRoleManagementMatrix GetUsersRolesMatrix()
        {
            UserRoleManagementMatrix userRoleManagementMatrix = new UserRoleManagementMatrix();
            userRoleManagementMatrix.Roles = new List<string>();
            string query = @"GetUserRoleMatrix";

            DataTable dt = dbu.ExecuteDataTable(query, null, true);

            foreach (DataColumn item in dt.Columns)
            {
                if (item.ColumnName != "UserName" && item.ColumnName != "id")
                    userRoleManagementMatrix.Roles.Add(item.ColumnName);
            }

            userRoleManagementMatrix.UsersInRoles = dt.AsEnumerable().Select(r =>
            {
                UserRoleOwning userRoleOwning = new UserRoleOwning();
                userRoleOwning.IsUserInRoles = new List<bool>();
                userRoleOwning.UserName = r["UserName"] == DBNull.Value ? null : (string)r["UserName"];
                foreach (string group in userRoleManagementMatrix.Roles)
                    userRoleOwning.IsUserInRoles.Add(r[group] == DBNull.Value ? false : true);
                return userRoleOwning;
            }).ToList();

            return userRoleManagementMatrix;
        }

        public List<Group> GetGroups()
        {
            string query = @"SELECT g.Id, g.Name, Count(u.id) AS UsersCount
                            FROM Groups g 
                            LEFT JOIN UsersInGroups uig ON g.Id = uig.GroupId 
                            LEFT JOIN Users u ON uig.UserId = u.Id
                            GROUP by g.Id, g.Name";
            DataTable dt = dbu.ExecuteDataTable(query, null);
            return dt.AsEnumerable().Select(r => new Group { Id = (int)r["Id"], Name = (string)r["Name"], UsersInGroup = (int)r["UsersCount"] }).ToList();
        }

        public List<Role> GetRoles()
        {
            string query = @"SELECT r.Id, r.Name, Count(u.id) AS UsersCount
                            FROM Roles r 
                            LEFT JOIN UsersRoles ur ON r.Id = ur.RoleId 
                            LEFT JOIN WebUsers u ON ur.UserId = u.Id
                            GROUP by r.Id, r.Name";
            DataTable dt = dbu.ExecuteDataTable(query, null);
            return dt.AsEnumerable().Select(r => new Role { Id = (int)r["Id"], Name = (string)r["Name"], UsersInRole = (int)r["UsersCount"] }).ToList();
        }

        public List<Group> GetGroupsByUser(int userId)
        {
            string query = @"SELECT g.Id, g.Name FROM Groups g LEFT JOIN UsersInGroups uig ON g.Id = uig.GroupId WHERE uig.UserId = @UserId";
            DataTable dt = dbu.ExecuteDataTable(query, new Dictionary<string, object>
                {
                    {"@UserId", userId}
                });
            return dt.AsEnumerable().Select(r => new Group { Id = (int)r["Id"], Name = (string)r["Name"] }).ToList();
        }

        #region Users and Goups
        public bool AddUserToGroup(string userName, string groupName)
        {
            try
            {
                dbu.ExecuteScalarQuery(@"INSERT INTO UsersInGroups(UserId, GroupId) 
                                         VALUES ((SELECT Id FROM Users WHERE UserName = @UserName), (SELECT Id FROM Groups WHERE Name = @GroupName)) ",
                            new Dictionary<string, object>()
                        {               
                            {"@UserName", userName},
                            {"@GroupName", groupName}
                        });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool RemoveUserFromGroup(string userName, string groupName)
        {
            try
            {
                dbu.ExecuteScalarQuery(@"DELETE FROM UsersInGroups 
                                         WHERE UserId = (SELECT Id FROM Users WHERE UserName = @UserName) 
                                         AND GroupId = (SELECT Id FROM Groups WHERE Name = @GroupName) ",
                            new Dictionary<string, object>()
                        {               
                            {"@UserName", userName},
                            {"@GroupName", groupName}
                        });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Groups
        public bool CreateNewGroup(string groupName)
        {
            try
            {
                dbu.InsertNewRowAndGetItsId("Groups", new Dictionary<string, object>()
                        {
                            {"Name", groupName}
                        }, false);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateGroup(int groupId, string groupName)
        {
            try
            {
                dbu.ExecuteScalarQuery(@"UPDATE Groups SET Name = @GroupName WHERE Id = @GroupId",
                            new Dictionary<string, object>()
                        {               
                            {"@GroupName", groupName},
                            {"@GroupId", groupId}
                        });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteGroup(int groupId)
        {
            try
            {
                dbu.ExecuteScalarQuery(@"DELETE FROM UsersInGroups WHERE GroupId = @GroupId",
                            new Dictionary<string, object>()
                        {                           
                            {"@groupId", groupId}
                        });

                dbu.ExecuteScalarQuery(@"DELETE FROM Groups WHERE Id = @GroupId",
                            new Dictionary<string, object>()
                        {                           
                            {"@GroupId", groupId}
                        });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

    }
}
