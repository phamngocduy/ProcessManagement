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
        public JsonResult savevalueruntask(string valuetext, string valuefile, int idtaskrun)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            
            taskService.submitvaluetask(valuetext, valuefile, idtaskrun);

            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Save Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult savetaskform(int idtaskrun, string formrender)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;

            taskService.savevaluetaskform(idtaskrun, formrender);

            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}