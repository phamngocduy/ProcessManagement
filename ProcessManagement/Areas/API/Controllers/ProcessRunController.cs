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
    public class ProcessRunController : ProcessManagement.Controllers.BaseController
    {
        ///=============================================================================================
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
        ParticipateService participateService = new ParticipateService();
        FileService fileService = new FileService();
        PMSEntities db = new PMSEntities();
        ///=============================================================================================

        [HttpPost]
        public JsonResult savetaskrun(int idtaskrun, string valuetext, string valuefile, HttpPostedFileBase fileupload, bool isEdit)
        {
            //TODO: Chưa phân quyền
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
            if (taskrun == null)
            {
                status = HttpStatusCode.NotFound;
                message = "TaskRun Not Found";
                response = new { message = message, status = status };
                SetFlash(FlashType.error, message);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
            Participate user = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
            bool haveRole = false;
            foreach (var role in listrole)
            {
                if (IdUser == role.IdUser)
                {
                    haveRole = true;
                    break;
                }
            }
            if (user.IsManager == true || haveRole)
            {
                taskService.submitvaluetask(IdUser, valuetext, valuefile, idtaskrun);
                int groupid = taskrun.StepRun.ProcessRun.Process.IdGroup;
                string taskRunPath = string.Format("Upload/{0}/run/{1}/{2}/{3}", groupid, taskrun.StepRun.ProcessRun.Id, taskrun.StepRun.Id, taskrun.Id);
                if (!isEdit)
                {
                    fileService.emptyDirectory(taskRunPath);
                }
                fileService.createDirectory(taskRunPath);
                fileService.saveFile(groupid, fileupload, taskRunPath, Direction.TaskRun);
            }

            message = "Save Task Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Save Task Successfully");
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult donetaskrun(int idtaskrun, string valuetext, string valuefile, HttpPostedFileBase fileupload, string comment, bool isEdit)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
            if (taskrun == null)
            {
                status = HttpStatusCode.NotFound;
                message = "TaskRun Not Found";
                response = new { message = message, status = status };
                SetFlash(FlashType.error, message);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
            Participate user = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
            bool haveRole = false;
            foreach (var role in listrole)
            {
                if (IdUser == role.IdUser)
                {
                    haveRole = true;
                    break;
                }
            }
            if (user.IsManager == true || haveRole)
            {
                taskService.submitvaluetask(IdUser, valuetext, valuefile, idtaskrun, true);
                int groupid = taskrun.StepRun.ProcessRun.Process.IdGroup;
                string taskRunPath = string.Format("Upload/{0}/run/{1}/{2}/{3}", groupid, taskrun.StepRun.ProcessRun.Id, taskrun.StepRun.Id, taskrun.Id);
                if (!isEdit)
                {
                    fileService.emptyDirectory(taskRunPath);
                }
                if (comment.Trim() != "")
                {
                    Comment cm = new Comment();
                    cm.IdUser = IdUser;
                    cm.IdDirection = taskrun.Id;
                    cm.Direction = Direction.TaskRun.ToString();
                    cm.Content = comment;
                    cm.isAction = true;
                    cm.Create_At = DateTime.Now;
                    cm.Update_At = DateTime.Now;
                    db.Comments.Add(cm);
                    db.SaveChanges();
                }
                fileService.createDirectory(taskRunPath);
                fileService.saveFile(groupid, fileupload, taskRunPath, Direction.TaskRun);
            }


            message = "Submit Task Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, message);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult savetaskform(int idtaskrun, string formrender)
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
        public JsonResult donetaskform(int idtaskrun, string formrender,JObject files)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;

            //TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
            //List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
            //Participate useringroup = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
            //if (useringroup.IsManager == true)
            //{
            //    taskService.donetaskform(idtaskrun, formrender, IdUser);
            //}
            //else
            //{
            //    foreach (var item in listrole)
            //    {
            //        if (IdUser == item.IdUser)
            //        {
            //            taskService.donetaskform(idtaskrun, formrender, IdUser);
            //        }
            //    }
            //}

            message = "Done Task";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Done Task");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult submitfinishtask(int idtask, string comment)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;

            TaskProcessRun taskrun = taskService.findTaskRun(idtask);
            taskService.submitclosetask(taskrun.Id, IdUser);
            if (comment.Trim() != "")
            {
                Comment cm = new Comment();
                cm.IdUser = IdUser;
                cm.IdDirection = taskrun.Id;
                cm.Direction = Direction.TaskRun.ToString();
                cm.Content = comment;
                cm.isAction = true;
                cm.Create_At = DateTime.Now;
                cm.Update_At = DateTime.Now;
                db.Comments.Add(cm);
                db.SaveChanges();
            }
            message = "Close Task Sucessfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, message);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult submitopentask(int idtask,string comment)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            string IdUser = User.Identity.GetUserId();

            TaskProcessRun taskrun = taskService.findTaskRun(idtask);
            taskService.submitopentask(taskrun.Id);
            if (comment.Trim() != "")
            {
                Comment cm = new Comment();
                cm.IdUser = IdUser;
                cm.IdDirection = taskrun.Id;
                cm.Direction = Direction.TaskRun.ToString();
                cm.Content = comment;
                cm.isAction = true;
                cm.Create_At = DateTime.Now;
                cm.Update_At = DateTime.Now;
                db.Comments.Add(cm);
                db.SaveChanges();
            }
            message = "Open Task Sucessfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, message);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult addComment(int id, Direction direction, string content)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            string IdUser = User.Identity.GetUserId();
            Comment com = new Comment();
            com.IdUser = IdUser;
            com.IdDirection = id;
            com.Direction = direction.ToString();
            com.Content = content;
            com.isAction = false;
            com.Create_At = DateTime.Now;
            com.Update_At = DateTime.Now;
            db.Comments.Add(com);
            db.SaveChanges();
            message = "Comment sucessfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, message);
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public PartialViewResult getComments(int id, string direction)
        {
            var comments = db.Comments.Where(x => x.IdDirection == id && x.Direction == direction.Trim()).OrderByDescending(x => x.Create_At).ToList();
            ViewData["comments"] = comments;
            return PartialView("~/Areas/API/Views/ProcessRun/Comment.cshtml");
        }

        [HttpPost]
        public JsonResult DeleteProcessRun(int idprocess)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            //find process
            ProcessRun processrun = processService.findRunProcessbyidprorun(idprocess);
            processService.removeprocessrun(idprocess);

            message = "Delete process Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Removed " + processrun.Name + " Successfully");

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}