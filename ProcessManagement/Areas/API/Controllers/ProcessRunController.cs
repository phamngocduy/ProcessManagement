using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Controllers;
using ProcessManagement.Filters;

namespace ProcessManagement.Areas.API.Controllers
{
    public class ProcessRunController : ProcessManagement.Controllers.BaseController
    {

        ///=============================================================================================
        CommonService commonService = new CommonService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
        ParticipateService participateService = new ParticipateService();
        ///=============================================================================================

        [HttpPost]
        public JsonResult savetaskrun(string valuetext, string valuefile, int idtaskrun)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            
            taskService.submitvaluetask(IdUser,valuetext, valuefile, idtaskrun);

            message = "Save Task Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Save Task Successfully");
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult submittaskrun(string valuetext, string valuefile, int idtaskrun)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
            List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
            Participate useringroup = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
            if (useringroup.IsManager == true)
            {
                taskService.submitvaluetask(IdUser, valuetext, valuefile, idtaskrun, true);
            }
            else
            {
                foreach (var item in listrole)
                {
                    if (IdUser == item.IdUser)
                    {
                        taskService.submitvaluetask(IdUser, valuetext, valuefile, idtaskrun, true);
                    }
                }
            }
            
            message = "Submit Task Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Submit Task Successfully");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult savetaskform(int idtaskrun,string formrender)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;

            taskService.savevaluetaskform(idtaskrun, formrender);

            message = "Save Task";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Save Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult donetaskform(int idtaskrun,string formrender)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;

            TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
            List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
            Participate useringroup = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
            if (useringroup.IsManager == true)
            {
                taskService.donetaskform(idtaskrun, formrender, IdUser);
            }
            else
            {
                foreach (var item in listrole)
                {
                    if (IdUser == item.IdUser)
                    {
                        taskService.donetaskform(idtaskrun, formrender, IdUser);
                    }
                }
            }

            message = "Done Task";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Done Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult submitfinishtask(int idtask)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;

            TaskProcessRun taskrun = taskService.findTaskRun(idtask);
            taskService.submitclosetask(taskrun.Id, IdUser);

            message = "Close Task";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Close Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult submitopentask(int idtask, string formrender)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;

            TaskProcessRun taskrun = taskService.findTaskRun(idtask);
            taskService.submitopentask(taskrun.Id, formrender);

            message = "Open Task";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Open Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}