using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevelopmentDashboardWeb.Models
{
    public class UserViewModel
    {
        public AuthenticationProvider.User User { get; set; }
        public string[] Roles { get; set; }
    }
}