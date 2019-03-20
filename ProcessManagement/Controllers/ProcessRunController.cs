using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessManagement.Services;
using ProcessManagement.Filters;
using ProcessManagement.Models;

namespace ProcessManagement.Controllers
{
    public class ProcessRunController : Controller
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        CommonService commonService = new CommonService();
        ParticipateService participateService = new ParticipateService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        TaskService taskService = new TaskService();
        RoleService roleService = new RoleService();
        ///=============================================================================================
        [GroupAuthorize]
        public ActionResult AssignRole(int groupid)
        {
            var group = groupService.findGroup(groupid);
            ViewData["Group"] = group;
            return View();
        }
        public ActionResult Detail(int groupid)
        {
            var group = groupService.findGroup(groupid);
            ViewData["Group"] = group;
            return View();
        }
    }
}