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
    [AjaxAuthorize]
    public class ProcessController : ProcessManagement.Controllers.BaseController
    {
        ///=============================================================================================
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
        ParticipateService participateService = new ParticipateService();
        ///=============================================================================================

        [HttpPost]
        [GroupAuthorize(Role = new UserRole[] { UserRole.Manager })]
        public JsonResult addTask(string name, int? idRole, string description, string inputConfig, string fileConfig)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            int idstep = (int)Session["idStep"];
            Step step = stepService.findStep(idstep);


            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Created Task Successfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            if (idRole != null)
            {
                int idR = idRole.GetValueOrDefault();
                var role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            taskService.addtask(step.Id, name, idRole, description, inputConfig, fileConfig);
            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddFormTask(string name, int? idRole, string description, string formBuilder)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            int idstep = (int)Session["idStep"];
            Step step = stepService.findStep(idstep);


            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Created Task Successfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            if (idRole != null)
            {
                int idR = idRole.GetValueOrDefault();
                var role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            taskService.AddFormTask(step.Id, name, idRole, description, formBuilder);
            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult editTask(string name, int? idRole, string description, string inputConfig, string fileConfig)
        {
            ///////////////////////////
            /// chỉ được edit task thuộc process mà mình quản lý
            ///////////////////////////
            var status = HttpStatusCode.OK;
            string message;
            object response;
            int idTask = int.Parse(Session["idTask"].ToString());
            var task = taskService.findTask(idTask);


            if (task == null)
            {
                message = "Task not exit";
                status = HttpStatusCode.InternalServerError;
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            Step step = stepService.findStep(task.Step.Id);
            //lấy group thuộc process
            int idGroup = task.Step.Process.Group.Id;
            string idUser = User.Identity.GetUserId();

            //check xem có thuộc process trong group
            Participate user = participateService.findMemberInGroup(idUser, idGroup);
            if (user == null)
            {
                message = "Task not found";
                status = HttpStatusCode.Forbidden;
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Created Task Successfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }




            if (idRole != null)
            {
                int idR = idRole.GetValueOrDefault();
                var role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            taskService.editTask(task.Id, name, idRole, description, inputConfig, fileConfig);
            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);


        }
        [HttpPost]
        public JsonResult changeTaskPosition(string position)
        {
            taskService.changePosition(position);
            var response = new { message = "Change position sucess", status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}