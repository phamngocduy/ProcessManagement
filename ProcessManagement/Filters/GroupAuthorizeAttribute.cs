using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
namespace ProcessManagement.Filters
{
    public class GroupAuthorizeAttribute : ActionFilterAttribute
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        ///=============================================================================================


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string idUser = System.Web.HttpContext.Current.User.Identity.GetUserId();
            //int idGroup = int.Parse(HttpContext.Current.Request.RequestContext.RouteData.Values["id"].ToString());

            string ownerSlug = HttpContext.Current.Request.RequestContext.RouteData.Values["userslug"] != null ?
                HttpContext.Current.Request.RequestContext.RouteData.Values["userslug"].ToString() : null;
            string groupSlug = HttpContext.Current.Request.RequestContext.RouteData.Values["groupslug"] != null ?
                HttpContext.Current.Request.RequestContext.RouteData.Values["groupslug"].ToString() : null;
            int groupid = HttpContext.Current.Session["groupid"] != null ? 
                (int)HttpContext.Current.Session["groupid"] : -1;
            Group group;
            if (ownerSlug == null && groupSlug == null && groupid == -1)
            {
                filterContext.Result = new HttpStatusCodeResult(404);
                return;
            }
            if (ownerSlug != null || groupSlug != null) group = groupService.findGroup(ownerSlug, groupSlug);
            else group = groupService.findGroup(groupid);

            if (group == null)
            {
                filterContext.Result = new HttpStatusCodeResult(404);
                return;
            }
            var check = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == group.Id).FirstOrDefault();
            if (check == null) filterContext.Result = new HttpStatusCodeResult(404);

        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if(HttpContext.Current.Session["groupid"] != null)
                HttpContext.Current.Session.Remove("groupid");
        }


    }
    //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //{
    //    filterContext.Result = new HttpUnauthorizedResult();
    //}
}
