using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentDashboardCore.Models
{
    public class UserManagementAddDataModel
    {
        public List<User> Users { get; set; }
        public List<Group> Groups { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public DateTime? ActivateDate { get; set; }
        public DateTime? AddedDate { get; set; }
        public bool Confirmed { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UsersInGroup { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UsersInRole { get; set; }
    }

    public class UserGroupManagementMatrix
    {
        public List<string> Groups { get; set; }
        public List<UserGroupOwning> UsersInGroups { get; set; }
    }

    public class UserGroupOwning
    {
        public string UserName { get; set; }
        public List<bool> IsUserInGroups { get; set; }
    }

    public class UserRoleManagementMatrix
    {
        public List<string> Roles { get; set; }
        public List<UserRoleOwning> UsersInRoles { get; set; }
    }

    public class UserRoleOwning
    {
        public string UserName { get; set; }
        public List<bool> IsUserInRoles { get; set; }
    }
}
