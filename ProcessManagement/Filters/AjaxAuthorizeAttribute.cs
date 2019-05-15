using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ninject.Web.Mvc.FilterBindingSyntax;
using ProcessManagement.Controllers;
using ProcessManagement.Models;
using ProcessManagement.Services;
namespace ProcessManagement.Filters
{
    public class AjaxAuthorizeAttribute : ActionFilterAttribute
    {
        ///=============================================================================================
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!isAjaxRequest(filterContext))
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { message = "Not Ajax Request" , status = HttpStatusCode.BadRequest },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
        public override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
        private bool isAjaxRequest(ControllerContext controllerContext)
        {
            HttpRequestBase request = controllerContext.RequestContext.HttpContext.Request;
            return request.IsAjaxRequest();
        }
    }
}
