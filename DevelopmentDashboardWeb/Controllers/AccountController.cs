using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AuthenticationProvider.Filters;

namespace DevelopmentDashboardWeb.Controllers
{
    [NonSecurableController]
    public class AccountController : BaseController
    {
        //
        // GET: /Account/

        [HttpGet]
        public ActionResult LogIn()
        {
            if(CurrentUser != null)
                return RedirectToAction("Index", "Home");
            string returnUrl = !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) ? Request.QueryString["ReturnUrl"] : "";
            Auth.HttpContext.Session["LoginReturnUrl"] = returnUrl;
            //Session["LoginReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            string activationCode = !string.IsNullOrEmpty(Request.QueryString["ActivationCode"]) ? Request.QueryString["ActivationCode"] : "";
            string login = !string.IsNullOrEmpty(Request.QueryString["Login"]) ? Request.QueryString["Login"] : "";

            if (!string.IsNullOrEmpty(activationCode) && Repository.ActivateAccount(activationCode))
            {
                ViewData["Message"] = "Account activated succefully";
                if (!string.IsNullOrEmpty(login))
                    Auth.Login(login, "tempPass");
            }
            else
            {
                ViewData["Message"] = "Activation code not found. You will be redirect to main page in 5 sec.";
                Response.AddHeader("REFRESH", "5;URL=" + Url.Action("Index", "Home"));
            }
            return View();
        }

        [HttpPost]
        public ActionResult Register(string password)
        {
            var user = CurrentUser;
            user.Password = Repository.CalculatePasswordHash(password);
            Repository.UpdateUser(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult LogIn(string login, string password)
        {
            if (ModelState.IsValid)
            {
                var user = Auth.Login(login, password);
                if (user != null)
                {
                    string returnUrl = Auth.HttpContext.Session["LoginReturnUrl"] == null ? "" : Auth.HttpContext.Session["LoginReturnUrl"].ToString();
                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect("~" + returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                ModelState["Password"].Errors.Add("User doesn't exists or password is incorrect");
            }

            return View();
        }

        public ActionResult Logout()
        {
            Auth.LogOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
