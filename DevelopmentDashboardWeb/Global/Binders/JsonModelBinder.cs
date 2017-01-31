using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DevelopmentDashboardWeb.Models;

namespace DevelopmentDashboardWeb.Global.Binders
{
    public class GridOptionsJsonModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                string json = controllerContext.HttpContext.Request.Params["options"]; ;

                return serializer.Deserialize<DXGridOptions>(json);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError("", "The item could not be serialized");
                return null;
            }
        }
    }
}