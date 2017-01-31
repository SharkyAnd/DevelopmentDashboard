using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using AuthenticationProvider;
using DevelopmentDashboardWeb.Global.Auth;
using AuthenticationProvider.Web.Authentication;
using System.Reflection;
using AuthenticationProvider.Filters;

namespace DevelopmentDashboardWeb.Controllers
{
    [NonSecurableController]
    public class BaseController : Controller
    {
        [Inject]
        public IRepository Repository { get; set; }

        [Inject]
        public IAuthentication Auth { get; set; }

        public User CurrentUser
        {
            get
            {
                return ((IUserProvider)Auth.CurrentUser.Identity).User;
            }
        }
    }
}
