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
        private readonly string idUser = null;
        private readonly int idGroup = -1;
        public GroupAuthorizeAttribute()
        {
            this.idUser = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (System.Web.HttpContext.Current.Session["idGroup"] != null)
            {
                this.idGroup = (int)System.Web.HttpContext.Current.Session["idGroup"];
                System.Web.HttpContext.Current.Session.Remove("idGroup");
            }
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            if (this.idUser == null || this.idGroup == -1)
            {
                return false;
            }
            var Group = db.Groups.Find(idGroup);
            switch (Group.Visibility1.Id)
            {
                case '1':
                    f
                case '2':

                case '3':

                default:
                    return false;
                    break;
            }
            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }
        private void privateGroup()
        {
            
        }
    }
}