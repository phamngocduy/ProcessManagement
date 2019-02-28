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
    public class GroupAuthorizeAttribute : ActionFilterAttribute
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        CommonService commonService = new CommonService();
        ///=============================================================================================
        public UserRole[] Role { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            int groupid;
            
            string idUser = HttpContext.Current.User.Identity.GetUserId();
            if (HttpContext.Current.Request.RequestContext.RouteData.Values["groupid"] != null)
                groupid = int.Parse(HttpContext.Current.Request.RequestContext.RouteData.Values["groupid"].ToString());
            else if (HttpContext.Current.Session["groupid"] != null)
                groupid = (int)HttpContext.Current.Session["groupid"];
            else
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
                return;
            }
             
           
            Group group = groupService.findGroup(groupid);
            if (group == null)
            { 
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
                return;
            }
            var user = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == group.Id).FirstOrDefault();
            if (user == null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                //return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                return;
            }
            if (Role != null)
            {
                var isOwner = group.IdOwner == idUser ? true : false;
                UserRole[] userRoles = new UserRole[3];
                if (isOwner) userRoles[0] = UserRole.Owner;
                if (user.IsAdmin) userRoles[1] = UserRole.Admin;
                if (user.IsManager) userRoles[2] = UserRole.Manager;
                var checkrole = userRoles.Intersect(this.Role).Any();
                if (!checkrole)
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                    return;
                }
            }
            HttpContext.Current.Session["groupid"] = groupid;
            filterContext.Controller.ViewBag.currentGroup = group;
        }
        public override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
            //if(HttpContext.Current.Session["groupid"] != null)
            //    HttpContext.Current.Session.Remove("groupid");
        }
        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    filterContext.Result = new HttpUnauthorizedResult();
        //}
    }
}
