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
        public JsonResult SaveTaskRun(int idtaskrun, string valuetext, string valuefile, HttpPostedFileBase fileupload, bool isEdit)
        {
            //TODO: Chưa phân quyền
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
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
            foreach (RoleRun role in listrole)
            {
                if (IdUser == role.IdUser)
                {
                    haveRole = true;
                    break;
                }
            }
            if (user.IsManager == true || haveRole)
            {
                taskService.submitTask(IdUser, valuetext, valuefile, idtaskrun);
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
        public JsonResult DoneTaskRun(int idtaskrun, string valuetext, string valuefile, HttpPostedFileBase fileupload, string comment, bool isEdit)
        {
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
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
            foreach (RoleRun role in listrole)
            {
                if (IdUser == role.IdUser)
                {
                    haveRole = true;
                    break;
                }
            }
            if (user.IsManager == true || haveRole)
            {
                taskService.submitTask(IdUser, valuetext, valuefile, idtaskrun, true);
                int groupid = taskrun.StepRun.ProcessRun.Process.IdGroup;
                string taskRunPath = string.Format("Upload/{0}/run/{1}/{2}/{3}", groupid, taskrun.StepRun.ProcessRun.Id, taskrun.StepRun.Id, taskrun.Id);
                if (!isEdit)
                {
                    fileService.removeDirectory(taskRunPath);
                    fileService.createDirectory(taskRunPath);
                    fileService.saveFile(groupid, fileupload, taskRunPath, Direction.TaskRun);
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
               
            }


            message = "Submit Task Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, message);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult SaveTaskForm(int idtaskrun, string formrender, string comment, string info)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                string IdUser = User.Identity.GetUserId();
                TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
                if (taskrun == null) throw new ServerSideException("TaskRun Not Found");
                List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
                Participate useringroup = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
                if (useringroup == null) throw new ServerSideException("User not in group");
                bool haveRole = false;
                foreach (RoleRun role in listrole)
                {
                    if (IdUser == role.IdUser)
                    {
                        haveRole = true;
                        break;
                    }
                }
                if (useringroup.IsManager == true || haveRole)
                {
                    HttpFileCollectionBase files = Request.Files;
                    //TODO: Chưa check form rule
                    //Upload File
                    int groupid = taskrun.StepRun.ProcessRun.Process.IdGroup;
                    //parse string to jArray
                    JArray jInfo = JArray.Parse(info);
                    JArray jFormRender = JArray.Parse(formrender);
                    int position = 0;
                    int positionFile = 0;
                    foreach (JToken input in jFormRender)
                    {
                        if ((string)input["type"] == "uploadFile")
                        {
                            JToken currentFileInfor = jInfo[position];
                            string taskFormRunPath = string.Format("Upload/{0}/run/{1}/{2}/{3}/{4}", groupid, taskrun.StepRun.ProcessRun.Id, taskrun.StepRun.Id, taskrun.Id, input["name"]);
                            if ((bool)currentFileInfor["isEdit"] == false)
                            {
                                fileService.removeDirectory(taskFormRunPath);
                                if ((bool)currentFileInfor["isEmpty"])
                                {
                                    input["path"] = "";
                                    input["filename"] = "";
                                    input["download"] = "";
                                }
                                else
                                {
                                    HttpPostedFileBase file = files[positionFile];
                                    fileService.createDirectory(taskFormRunPath);
                                    FileManager f = fileService.saveFile(groupid, file, taskFormRunPath, Direction.TaskFormRun);
                                    input["path"] = taskFormRunPath;
                                    input["filename"] = file.FileName;
                                    input["download"] = f.Id;
                                    positionFile++;
                                }
                                position++;
                            }
                        }
                    }
                    string newFormString = jFormRender.ToString();
                    taskService.submitTaskForm(IdUser, idtaskrun, newFormString);
                }
                else throw new ServerSideException("It is not your task");



                message = "Save Task Sucessfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                status = HttpStatusCode.InternalServerError;
                message = e.GetType().Name == "ServerSideException" ? e.Message : "Something not right";
                response = new { message = message, detail = e.Message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult DoneTaskForm(int idtaskrun, string formrender, string comment, string info)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                string IdUser = User.Identity.GetUserId();
                TaskProcessRun taskrun = taskService.findTaskRun(idtaskrun);
                if (taskrun == null) throw new ServerSideException("TaskRun Not Found");
                List<RoleRun> listrole = roleService.findlistrolerunbyidroleprocess(taskrun.IdRole);
                Participate useringroup = participateService.findMemberInGroup(IdUser, taskrun.StepRun.ProcessRun.Process.IdGroup);
                if (useringroup == null) throw new ServerSideException("User not in group");
                bool haveRole = false;
                foreach (RoleRun role in listrole)
                {
                    if (IdUser == role.IdUser)
                    {
                        haveRole = true;
                        break;
                    }
                }
                if (useringroup.IsManager == true || haveRole)
                {
                    HttpFileCollectionBase files = Request.Files;
                    //TODO: Chưa check form rule
                    //Upload File
                    int groupid = taskrun.StepRun.ProcessRun.Process.IdGroup;
                    //parse string to jArray
                    JArray jInfo = JArray.Parse(info);  
                    JArray jFormRender = JArray.Parse(formrender);
                    int position = 0;
                    int positionFile = 0;
                    foreach (JToken input in jFormRender)
                    {
                        if ((string)input["type"] == "uploadFile")
                        {
                            JToken currentFileInfor = jInfo[position];
                            string taskFormRunPath = string.Format("Upload/{0}/run/{1}/{2}/{3}/{4}", groupid, taskrun.StepRun.ProcessRun.Id, taskrun.StepRun.Id, taskrun.Id, input["name"]);
                            if ((bool)currentFileInfor["isEdit"] == false)
                            {
                                fileService.removeDirectory(taskFormRunPath);
                                if ((bool)currentFileInfor["isEmpty"])
                                {
                                    input["path"] = "";
                                    input["filename"] = "";
                                    input["download"] = "";
                                }
                                else {
                                    HttpPostedFileBase file = files[positionFile];
                                    fileService.createDirectory(taskFormRunPath);
                                    FileManager f = fileService.saveFile(groupid, file, taskFormRunPath, Direction.TaskFormRun);
                                    input["path"] = taskFormRunPath;
                                    input["filename"] = file.FileName;
                                    input["download"] = f.Id;
                                    positionFile++;
                                }
                                position++;
                            }
                        }
                    }
                    string newFormString = jFormRender.ToString();
                    taskService.submitTaskForm(IdUser,idtaskrun, newFormString, true);

                    //comment
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
                }
                else throw new ServerSideException("It is not your task");
                


                message = "Done Task Successfully";
                response = new { message = message, status = status };
                SetFlash(FlashType.success, message);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                status = HttpStatusCode.InternalServerError;
                message = e.GetType().Name == "ServerSideException" ? e.Message : "Something not right";
                response = new { message = message, detail = e.Message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SubmitFinishTask(int idtask, string comment)
        {
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
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
        public JsonResult SubmitOpenTask(int idtask,string comment)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
        public JsonResult AddComment(int id, Direction direction, string content)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
        public PartialViewResult GetComments(int id, string direction)
        {
            List<Comment> comments = db.Comments.Where(x => x.IdDirection == id && x.Direction == direction.Trim()).OrderByDescending(x => x.Create_At).ToList();
            ViewData["comments"] = comments;
            return PartialView("~/Areas/API/Views/ProcessRun/Comment.cshtml");
        }

        [HttpPost]
        public JsonResult DeleteProcessRun(int processid)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                processService.removeprocessrun(processid);
                message = "Remove ProcessRun Successfully";
                response = new { message = message, status = status };
                SetFlash(FlashType.success, message);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                status = HttpStatusCode.InternalServerError;
                message = e.GetType().Name == "ServerSideException" ? e.Message : "Something not right";
                response = new { message = message, detail = e.Message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}