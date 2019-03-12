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
        private Participate user {get;set;}
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!isAjaxRequest(filterContext))
            {
                //form request
                BaseAuthorize(filterContext);
            }
            else
            {
                //ajax request
                AjaxAuthorize(filterContext);
            }

        }
        public override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }
        private bool isAjaxRequest(ControllerContext controllerContext)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            return request.IsAjaxRequest();
        }
        public void BaseAuthorize(ActionExecutingContext filterContext)
        {
            int groupid;

            if (filterContext.RouteData.Values["groupid"] != null)
                groupid = int.Parse(filterContext.RouteData.Values["groupid"].ToString());
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

            string idUser = HttpContext.Current.User.Identity.GetUserId();
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
            HttpContext.Current.Session["idgroup"] = groupid;
        }
        public void AjaxAuthorize(ActionExecutingContext filterContext)
        {
            string idUser = HttpContext.Current.User.Identity.GetUserId();
            int groupid;

            if (filterContext.Controller.ValueProvider.GetValue("groupid") != null)
                groupid = int.Parse(filterContext.Controller.ValueProvider.GetValue("groupid").AttemptedValue);
            //else if (HttpContext.Current.Session["groupid"] != null)
            //    groupid = (int)HttpContext.Current.Session["groupid"];
            else if (filterContext.Controller.ValueProvider.GetValue("processid") != null)
            {
                int processId = int.Parse(filterContext.Controller.ValueProvider.GetValue("processid").AttemptedValue);
                Process process = db.Processes.Find(processId);
                if (process == null)
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { message = "Process Not Found", status = HttpStatusCode.NotFound },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
                groupid = process.IdGroup;
            }
            else if (filterContext.Controller.ValueProvider.GetValue("stepid") != null)
            {
                int stepId = int.Parse(filterContext.Controller.ValueProvider.GetValue("stepid").AttemptedValue);
                Step step = db.Steps.Find(stepId);
                if (step == null)
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { message = "Step Not Found", status = HttpStatusCode.NotFound },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
                groupid = step.Process.IdGroup;
            }
            else if (filterContext.Controller.ValueProvider.GetValue("taskid") != null)
            {
                int taskId = int.Parse(filterContext.Controller.ValueProvider.GetValue("taskid").AttemptedValue);
                TaskProcess task = db.TaskProcesses.Find(taskId);
                if (task == null)
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { message = "Task Not Found", status = HttpStatusCode.NotFound },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
                groupid = task.Step.Process.IdGroup;
            }
            else
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { message = "Group Not Found", status = HttpStatusCode.NotFound },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }


            Group group = groupService.findGroup(groupid);
            if (group == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { message = "Group Not Found", status = HttpStatusCode.NotFound },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return; 
            }
            user = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == group.Id).FirstOrDefault();
            if (user == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { message = "You not belong to this group", status = HttpStatusCode.NotFound },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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
                    filterContext.Result = new JsonResult
                    {
                        Data = new { message = "You dont have permission", status = HttpStatusCode.Forbidden },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
            }
            HttpContext.Current.Session["idgroup"] = groupid;
        }
    }
}
