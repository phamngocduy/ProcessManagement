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
        CommonService commonService = new CommonService();
        ///=============================================================================================


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            int groupid;
            string idUser = HttpContext.Current.User.Identity.GetUserId();
            if (HttpContext.Current.Request.RequestContext.RouteData.Values["idgroup"] != null)
            {
                groupid = int.Parse(HttpContext.Current.Request.RequestContext.RouteData.Values["idgroup"].ToString());
            }
            else if (HttpContext.Current.Session["groupid"] != null)
            {

                groupid = (int)HttpContext.Current.Session["groupid"];
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(404);
                return;
            }
             
           
            Group group = groupService.findGroup(groupid);
            if (group == null)
            {
                filterContext.Result = new HttpStatusCodeResult(404);
                return;
            }
            var check = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == group.Id).FirstOrDefault();
            if (check == null) filterContext.Result = new HttpStatusCodeResult(404);
            HttpContext.Current.Session["groupid"] = groupid;
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if(HttpContext.Current.Session["groupid"] != null)
                HttpContext.Current.Session.Remove("groupid");
        }
        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    filterContext.Result = new HttpUnauthorizedResult();
        //}
    }
}
