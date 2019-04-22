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
    public class FileController : Controller
    {

        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        FileService fileService = new FileService();
        TaskService taskService = new TaskService();
        ///=============================================================================================

        [GroupAuthorize]
        [HttpPost]
        public JsonResult uploadFile(int groupid, int? processid, int? stepid, int? taskid, HttpPostedFileBase FileUpload, Direction direction)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            if (FileUpload.ContentLength == 0)
            {
                status = HttpStatusCode.NoContent;
                message = "Your File is empty";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            ConfigRule fileSizeRule = db.ConfigRules.Find("filesize");
            bool isFileOverSize = fileService.checkFileOverSize(FileUpload);
            if (isFileOverSize)
            {
                status = HttpStatusCode.InternalServerError;
                message = string.Format("This file is too big ({0} {1} maximum)",fileSizeRule.Value,fileSizeRule.Unit);
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            string filePath;
            if (direction == Direction.Process)
            {
                Process pr = processService.findProcess(processid.GetValueOrDefault());
                if (pr == null)
                {
                    status = HttpStatusCode.NotFound;
                    message = "Process not found";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                filePath = String.Format("Upload/{0}/{1}", pr.IdGroup, pr.Id);
            }
            else if (direction == Direction.Step)
            {
                Step st = stepService.findStep(stepid.GetValueOrDefault());
                if (st == null)
                {
                    status = HttpStatusCode.NotFound;
                    message = "Step not found";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                filePath = String.Format("Upload/{0}/{1}/{2}", st.Process.Group.Id, st.Process.Id, st.Id);

            }
            else if (direction == Direction.Task)
            {
                TaskProcess tp = taskService.findTask(taskid.GetValueOrDefault());
                if (tp == null)
                {
                    status = HttpStatusCode.NotFound;
                    message = "Task not found";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                filePath = String.Format("Upload/{0}/{1}/{2}/{3}", tp.Step.Process.Group.Id, tp.Step.Process.Id, tp.Step.Id, tp.Id);
            }
            else
            {
                Group gr = groupService.findGroup(groupid);
                if (gr == null)
                {
                    status = HttpStatusCode.NotFound;
                    message = "Group not found";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                filePath = String.Format("Upload/{0}", gr.Id);
            }
            FileManager f = fileService.saveFile(groupid, FileUpload, filePath, direction);
            object data = new
            {
                id = f.Id,
                name = f.Name
            };

            message = "Save File Sucessfull";
            response = new { message = message, data = data, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [GroupAuthorize]
        [HttpPost]
        public JsonResult removeFile(int groupid, string id)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            FileManager file = fileService.findFile(id);
            if (file == null)
            {
                status = HttpStatusCode.NotFound;
                message = "File not found";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            fileService.removeFile(file);
            message = "Remove File Sucessfull";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult changeFileName(int groupid, string id, string filename)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            FileManager file = fileService.findFile(id);
            filename = filename.Trim();
            if (file == null)
            {
                status = HttpStatusCode.NotFound;
                message = "File not found";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            var isFileExisted = fileService.checkFileExist(groupid, filename, (Direction)Enum.Parse(typeof(Direction), file.Direction), file.Path);
            if (isFileExisted)
            {
                status = HttpStatusCode.InternalServerError;
                message = "File Existed";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            var f = fileService.changeFileName(file, filename);
            object data = new
            {
                id = f.Id,
                name = f.Name
            };
            message = "Change Name sucessfully";
            response = new { message = message, data = data, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}