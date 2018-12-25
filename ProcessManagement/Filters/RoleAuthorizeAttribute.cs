using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessManagement.Models;
using Microsoft.AspNet.Identity;
namespace ProcessManagement.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        PMSEntities db = new PMSEntities();
        private readonly string idUser = null;

        private readonly int idGroup=-1;
        private readonly string[] allowedRoles;
        private readonly string[] userRoles = new string[10];
        public RoleAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
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
            var user = db.Participates.Where(m => m.IdGroup == this.idGroup && m.IdUser == this.idUser).FirstOrDefault();
            var isOwner = db.Groups.Where(m => m.Id == this.idGroup && m.IdOwner == this.idUser).Count() > 1 ? true : false;
            if (user.IsAdmin) this.userRoles[0] = "Admin";
            if (user.IsManager) this.userRoles[1] = "Manager";
            if (isOwner) this.userRoles[2] = "Owner";
            var checkrole = this.userRoles.Intersect(this.allowedRoles).Any();
            return checkrole;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}