using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessManagement.Models;
using Microsoft.AspNet.Identity;
using ProcessManagement.Services;
namespace ProcessManagement.Filters
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        private readonly string idUser = null;

        private readonly int groupid;
        private readonly string[] allowedRoles;
        private readonly string[] userRoles = new string[3];
        private readonly string ownerSlug;
        private readonly string groupSlug;
        public RoleAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
            this.idUser = System.Web.HttpContext.Current.User.Identity.GetUserId();
            this.ownerSlug = HttpContext.Current.Request.RequestContext.RouteData.Values["userslug"] != null ?
                HttpContext.Current.Request.RequestContext.RouteData.Values["userslug"].ToString() : null;
            this.groupSlug = HttpContext.Current.Request.RequestContext.RouteData.Values["groupslug"] != null ?
                HttpContext.Current.Request.RequestContext.RouteData.Values["groupslug"].ToString() : null;
            int groupid = HttpContext.Current.Session["groupid"] != null ?
                (int)HttpContext.Current.Session["groupid"] : -1;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            Group group;
            if (ownerSlug != null || groupSlug != null) group = groupService.findGroup(ownerSlug, groupSlug);
            else group = groupService.findGroup(groupid);
            Participate user = db.Participates.Where(m => m.IdGroup == group.Id && m.IdUser == this.idUser).FirstOrDefault();
            bool isOwner = db.Groups.Where(m => m.Id == group.Id && m.IdOwner == this.idUser).Count() > 1 ? true : false;
            if (user.IsAdmin) this.userRoles[0] = "Admin";
            if (user.IsManager) this.userRoles[1] = "Manager";
            if (isOwner) this.userRoles[2] = "Owner";
            bool checkrole = this.userRoles.Intersect(this.allowedRoles).Any();
            if (!checkrole)
            {
                filterContext.Result = new HttpStatusCodeResult(404);
            }
            //return checkrole;
        }
        
    }
}