using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using DevelopmentDashboardWeb.Models;
using Ninject;
using AuthenticationProvider;
using System.Reflection;
using AuthenticationProvider.Filters;
using System.Web.Script.Serialization;
using AuthenticationProvider.Models;

namespace DevelopmentDashboardWeb.Controllers
{
    public class AdministrationController : BaseController
    {
        //
        // GET: /Settings/
        ConfigurationUtils.DatabaseConfigProvider serviceSettingsProvider = new ConfigurationUtils.DatabaseConfigProvider(
            DevelopmentDashboardCore.DevelopmentDashboardConfig.Instance.ConnectionString);
        DevelopmentDashboardCore.AppData.SiteAdministrationProvider siteAdminProvider = new DevelopmentDashboardCore.AppData.SiteAdministrationProvider();

        [SecurableAction]
        public ActionResult Settings()
        {
            ViewData["Services"] = serviceSettingsProvider.GetServices();
            return View();
        }
        
        public JsonResult GetSettingsJson()
        {
            var model = serviceSettingsProvider.GetServiceSettings(typeof(DevelopmentDashboardCore.WebConfigurations.DDWebConfig)).Select(ss => new
            {
                Id = ss.Id,
                ServiceName = ss.ServiceName,
                Name = ss.Name,
                Value = ss.Value,
                Description = ss.Description
            });
            
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult UpdateService(int serviceId, string value, string description = null)
        {
            var result = serviceSettingsProvider.UpdateSeviceSettingsValue(serviceId, value, description);
            DevelopmentDashboardCore.WebConfigurations.DDWebConfig.Instance.Reset();
            return Json(result);
        }
        [SecurableAction]
        public ActionResult UsersGroups()
        {
            return View();
        }
        
        public JsonResult GetUsersGroupsJson()
        {
            return Json(siteAdminProvider.GetGroups(), JsonRequestBehavior.AllowGet);
        }

        [SecurableAction]
        public ActionResult UsersRoles()
        {
            Repository.SynchronizeWebPermissions(Assembly.GetExecutingAssembly());
            return View();
        }
        
        public JsonResult GetUsersRolesJson()
        {
            return Json(Repository.GetRoles(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPublicAccessRulesJson()
        {
            return Json(Repository.GetRules(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUsersJson()
        {
            return Json(Repository.GetUsers().Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Confirmed = u.Confirmed,
                    LastVisitDate = u.LastVisitDate.HasValue ? u.LastVisitDate.Value.ToUniversalTime().ToJson() : 0,
                    AddedDate = u.AddedDate.HasValue ? u.AddedDate.Value.ToUniversalTime().ToJson() : 0,
                    ActivateDate = u.ActivateDate.HasValue ? u.ActivateDate.Value.ToUniversalTime().ToJson() : 0
                })
                , JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetUsersGroupsMatrixJson()
        {
            var managementMatrix = siteAdminProvider.GetUsersGroupsMatrix();
            var model = new
            {
                Groups = managementMatrix.Groups,
                MatrixData = managementMatrix.UsersInGroups.Select(uig => new
                {
                    UserName = uig.UserName,
                    UserInGroups = uig.IsUserInGroups
                })
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetUsersRolesMatrixJson()
        {
            var managementMatrix = siteAdminProvider.GetUsersRolesMatrix();
            var model = new
            {
                Roles = managementMatrix.Roles,
                MatrixData = managementMatrix.UsersInRoles.Select(uig => new
                {
                    UserName = uig.UserName,
                    UserInRoles = uig.IsUserInRoles
                })
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetUsersRolesAdditionalDataJson()
        {
            return Json(new
            {
                UserNames = Repository.GetUserNames(),
                RolesNames = Repository.GetRolesNames(),
                Permissions = Repository.GetActionsByControllers()
            }, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult SendConfirmationEmail(ConfirmationDataModel confirmationData)
        {
            if (confirmationData.ActivationCode == null)
                confirmationData.ActivationCode = Guid.NewGuid().ToString("N");
            Repository.AddActivationCode(confirmationData.ActivationCode, CurrentUser == null ? "AAkulin" : CurrentUser.UserName, confirmationData.UserName == null ? confirmationData.Email : confirmationData.UserName);
            string url = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            Repository.SendConfirmationEmail(confirmationData, "Development Dashboard Web", url);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        #region Groups
        
        public JsonResult AddNewGroup(string groupName)
        {
            return Json(siteAdminProvider.CreateNewGroup(groupName));
        }
        
        public JsonResult DeleteGroup(int groupId)
        {
            return Json(siteAdminProvider.DeleteGroup(groupId));
        }
        
        public JsonResult UpdateGroup(int groupId, string groupName)
        {
            return Json(siteAdminProvider.UpdateGroup(groupId, groupName));
        }
        #endregion

        #region UsersGroups
        
        public JsonResult AddUserToGroup(string userName, string groupName)
        {
            return Json(siteAdminProvider.AddUserToGroup(userName, groupName));
        }
        
        public JsonResult RemoveUserFromGroup(string userName, string groupName)
        {
            return Json(siteAdminProvider.RemoveUserFromGroup(userName, groupName));
        }
        #endregion

        #region Roles
        
        public JsonResult AddNewRole(string role)
        {
            Role Role = new JavaScriptSerializer().Deserialize<Role>(role);

            return Json(Repository.CreateRole(Role));
        }
        
        public JsonResult DeleteRole(int roleId)
        {
            return Json(Repository.RemoveRole(roleId));
        }

        public JsonResult UpdateRole(string role)
        {
            Role Role = new JavaScriptSerializer().Deserialize<Role>(role);

            return Json(Repository.UpdateRole(Role));
        }
        #endregion

        #region Rules

        public JsonResult AddNewRule(string rule)
        {
            Rule Rule = new JavaScriptSerializer().Deserialize<Rule>(rule);

            return Json(Repository.CreateRule(Rule));
        }

        public JsonResult DeleteRule(int ruleId)
        {
            return Json(Repository.RemoveRule(ruleId));
        }

        public JsonResult UpdateRule(string rule)
        {
            Rule Rule = new JavaScriptSerializer().Deserialize<Rule>(rule);

            return Json(Repository.UpdateRule(Rule));
        }
        #endregion

        #region UsersRoles
    
        public JsonResult AddUserToRole(AuthenticationProvider.UserRole userRole)
        {
            return Json(Repository.CreateUserRole(userRole));
        }
    
        public JsonResult RemoveUserRole(string userName, string roleName)
        {
            return Json(Repository.RemoveUserRole(userName, roleName));
        }
        #endregion

        #region Users

        public JsonResult AddNewUser(User user)
        {
            string confirmPassword = user.Password;
            user.Password = Repository.CalculatePasswordHash(user.Password);
            Repository.CreateUser(user);
            SendConfirmationEmail(new ConfirmationDataModel
            {
                Email = user.Email,
                UserName = user.UserName,
                Password = confirmPassword,
                ActivationCode = Guid.NewGuid().ToString("N"),
                AssociatedRoles = user.AssociatedRoles
            });
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateUser(AuthenticationProvider.User user)
        {
            if (user.Password != null)
                user.Password = Repository.CalculatePasswordHash(user.Password);
            return Json(Repository.UpdateUser(user));
        }

        public JsonResult DeleteUser(int idUser)
        {
            return Json(Repository.RemoveUser(idUser));
        }
        #endregion
    }
}
