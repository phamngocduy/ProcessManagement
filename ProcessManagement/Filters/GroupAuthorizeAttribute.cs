using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessManagement.Models;
using Microsoft.AspNet.Identity;
namespace ProcessManagement.Filters
{
    public class GroupAuthorizeAttribute : AuthorizeAttribute
    {
        PMSEntities db = new PMSEntities();
        
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string idUser = System.Web.HttpContext.Current.User.Identity.GetUserId();
            int idGroup = int.Parse(HttpContext.Current.Request.RequestContext.RouteData.Values["id"].ToString());
            if (idUser == null || idGroup == -1)
            {
                return false;
            }
            var check = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == idGroup).FirstOrDefault();
            return check == null ? false : true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}