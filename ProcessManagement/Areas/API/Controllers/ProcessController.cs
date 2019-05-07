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
using Newtonsoft.Json;
using System.IO;
using Ionic.Zip;

namespace ProcessManagement.Areas.API.Controllers
{
    
    public class ProcessController : ProcessManagement.Controllers.BaseController
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        CommonService commonService = new CommonService();
        GroupService groupService = new GroupService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
        ParticipateService participateService = new ParticipateService();
        FileService fileService = new FileService();
        ///=============================================================================================
        [HttpPost]
        [GroupAuthorize(Role = new UserRole[] { UserRole.Manager })]
        public JsonResult editstep(int stepid, string des)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            Step step = db.Steps.Find(stepid);
            step.Description = des.Trim();
            step.Updated_At = DateTime.Now;
            db.SaveChanges();
            
            message = "Update Step Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [GroupAuthorize(Role = new UserRole[] { UserRole.Manager })]
        public JsonResult addTask(int stepid, string name, int? idRole, string description, string inputConfig, string fileConfig, HttpPostedFileBase fileupload)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            //int idstep = (int)Session["idStep"];
            Step step = stepService.findStep(stepid);


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

            var task = taskService.addtask(step.Id, name, idRole, description, inputConfig, fileConfig);
            //create directory
            Group group = groupService.findGroup(step.Process.Group.Id);
            string directoryPath = String.Format("Upload/{0}/{1}/{2}/{3}", group.Id, step.Process.Id, step.Id, task.Id);
            fileService.createDirectory(directoryPath);
            //file 
            fileService.saveFile(group.Id, fileupload, directoryPath, Direction.Task);

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
        [GroupAuthorize(Role = new UserRole[] { UserRole.Manager })]
        public JsonResult AddFormTask(string name, int? idRole, string description, string formBuilder, HttpPostedFileBase fileupload)
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

            var task = taskService.AddFormTask(step.Id, name, idRole, description, formBuilder);
            //create directory
            Group group = groupService.findGroup(step.Process.Group.Id);
            string directoryPath = String.Format("Upload/{0}/{1}/{2}/{3}", group.Id, step.Process.Id, step.Id, task.Id);
            fileService.createDirectory(directoryPath);

            fileService.saveFile(group.Id, fileupload, directoryPath, Direction.Task);

            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult editFormTask(string name, int? idRole, string description, string formBuilder)
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

            taskService.editFromTask(task.Id, name, idRole, description, formBuilder);
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
        public JsonResult getProcessList(int groupid)
        {
            List<Process> processes = processService.getProcess(groupid);
            List<object> jProcesses = new List<object>();
            foreach (var process in processes.Where(x => x.IsRun == null || x.IsRun == false))
            {
                object jProcess = new
                {
                    id = process.Id,
                    name = process.Name,
                    search = process.Name.ToLower(),
                    des = process.Description,
                    owner = new {
                        name = process.AspNetUser.UserName,
                        avatar = process.AspNetUser.Avatar,
                        avatardefault = process.AspNetUser.AvatarDefault
                    },
                    avatar = process.Avatar,
                    selected = false,
                    update_at = process.Updated_At,
                    time_ralitive = commonService.TimeAgo(process.Updated_At)
                };
                jProcesses.Add(jProcess);
            }
            
            var response = new { data = jProcesses, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult addProcessRun(int processid, string name,string des)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            Process process = processService.findProcess(processid);
            Process processrun = processService.createProcessRun(process, name, des);
            
            List<Role> role = roleService.findListRoleOfProcess(processid);
            List<Role> rolerun = roleService.addRoleRun(role, processrun.Id);
            List<Step> liststep = stepService.findStepsOfProcess(processid);
            List<Step> liststeprun = stepService.addStepRun(liststep, processrun.Id);
            foreach (var step in liststep)
            {
                List<TaskProcess> listtask = taskService.findTaskOfStep(step.Id);
                taskService.addListTaskRun(listtask, rolerun, liststeprun);
            }
            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult assignRole(int processid, int roleid, string members)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            Role role = roleService.findRoleOfProcess(roleid, processid);
            if (role == null)
            {
                status = HttpStatusCode.NotFound;
                message = "Role isn't exist in process";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            roleService.removeRoleRun(roleid);
            List<string> assignList = members.Split(',').ToList();
            if (assignList.Any())
            {
                foreach (var member in assignList)
                {
                    var isMemberInGroup = participateService.checkMemberInGroup(member, role.Process.Group.Id);
                    if (isMemberInGroup)
                    {
                        roleService.assignrolerun(roleid, member);
                    }
                }
            }
            message = "Assign Role Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult runprocess(int idprocess)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            Process findprocess = processService.findProcess(idprocess);
            List<Step> liststep = stepService.findStepsOfProcess(idprocess);
            processService.addrunprocess(findprocess);
            StepRun runstep = stepService.addstartstep(idprocess);
            Step idsteprunstart = liststep.Where(x => x.Key == runstep.Key && x.StartStep == true).FirstOrDefault();
            List<TaskProcess> listtaskrun = taskService.findTaskOfStep(idsteprunstart.Id);
            taskService.addlistruntask(listtaskrun, runstep);
            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Create Run Process");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult addnextstepinrunprocess(int idStep, int idnextstep, int idstepdiamond)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            Step stepchoosenext = stepService.findStep(idnextstep);
            Step idstepdk = stepService.findStep(idstepdiamond);
            StepRun runstep = stepService.findsteprun(idStep);
            stepService.changestatustep(runstep.Id, IdUser);
            ProcessRun processrun = processService.findRunProcess(runstep.idProcess);
            if (idstepdk != null)
            {
                stepService.addrunnextstep(processrun.Id, idstepdk);
            }
            //List<Step> liststep = stepService.findStepsOfProcess(processrun.IdProcess);
            
            List<TaskProcessRun> listruntask = taskService.findruntaskofstep(idStep);
            List<TaskProcessRun> listtaskclose = listruntask.Where(x => x.Status1.Name == "Finish").ToList();

            //List<Step> nextstep = new List<Step>();
            StepRun runnextstep = new StepRun();
            //foreach (var item in liststep)
            //{
            //    if (runstep.NextStep1 == item.Key && item.StartStep == false)
            //    {
            //        nextstep.Add(item);
            //    }
            //}
            if (stepchoosenext != null)
            {
                if (listtaskclose.Count == listruntask.Count)
                {
                   runnextstep = stepService.addrunnextstep(processrun.Id, stepchoosenext);
                }
            }
            //foreach (var nexts in nextstep)
            //{
                List<TaskProcess> listtasknextstep = taskService.findTaskOfStep(stepchoosenext.Id);
                taskService.addlistruntask(listtasknextstep, runnextstep);
            //}

            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Next step success");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult submitdonetask(int idtask)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;

            TaskProcessRun taskrun = taskService.findTaskRun(idtask);
            taskService.submitdonetask(taskrun.Id);

            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Step Done");
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult deletenextsteprun(int idStep)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            StepRun runstep = stepService.findsteprun(idStep);
            List<StepRun> liststeprun = stepService.findStepsOfRunProcess(runstep.idProcess);
            StepRun stepback = new StepRun();
            foreach (var item in liststeprun)
            {
                if (item.Id != runstep.Id)
                {
                    stepback = item;
                }
            }
           
            if (stepback.Figure == "Diamond")
            {
                StepRun stepbacknotdiamond = liststeprun.Where(x => x.NextStep1 == stepback.Key).OrderByDescending(x => x.Created_at).FirstOrDefault();
                stepService.removeStepRun(stepback);
                stepService.deletenextsteprun(runstep, stepbacknotdiamond);
            }
            else
            {
                stepService.deletenextsteprun(runstep, stepback);
            }
            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Delete Step");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult completestepinrunprocess(int idStep)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            StepRun runstep = stepService.findsteprun(idStep);
            stepService.changestatustep(runstep.Id, IdUser);
            ProcessRun processrun = processService.findRunProcess(runstep.idProcess);
            List<Step> liststep = stepService.findStepsOfProcess(processrun.IdProcess);

            List<TaskProcessRun> listruntask = taskService.findruntaskofstep(idStep);
            List<TaskProcessRun> listtaskclose = listruntask.Where(x => x.Status1.Name == "Finish").ToList();

            List<Step> nextstep = new List<Step>();
            StepRun runnextstep = new StepRun();
            foreach (var item in liststep)
            {
                if (runstep.NextStep1 == item.Key && item.StartStep == false)
                {
                    nextstep.Add(item);
                }
            }
            if (nextstep != null)
            {
                if (listtaskclose.Count == listruntask.Count)
                {
                    runnextstep = stepService.completestepinrunprocess(processrun.Id, nextstep);
                }
            }
            foreach (var nexts in nextstep)
            {
                List<TaskProcess> listtasknextstep = taskService.findTaskOfStep(nexts.Id);
            taskService.addlistruntask(listtasknextstep, runnextstep);
            }

            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            SetFlash(FlashType.success, "Next step success");
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult createProcessCopy(int processid)
        {
            string IdUser = User.Identity.GetUserId();
            var status = HttpStatusCode.OK;
            string message;
            object response;
            var process = processService.findProcess(processid);
            if (process == null)
            {
                status = HttpStatusCode.InternalServerError;
                message = "process not found";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }


            try
            {
                //create folder
                string copyPath = string.Format("Copy/{0}", process.Id);
                fileService.removeDirectory(copyPath);
                fileService.createDirectory(copyPath);

                //role
                var roles = roleService.findListRoleOfProcess(processid);
                List<object> copyRoles = new List<object>();
                foreach (var role in roles)
                {
                    object copyRole = new
                    {
                        roleid = role.Id,
                        rolename = role.Name
                    };
                    copyRoles.Add(copyRole);
                }
                //step
                var steps = stepService.findStepsOfProcess(processid);
                List<object> copySteps = new List<object>();
                foreach (var step in steps)
                {
                    //task
                    var tasks = taskService.findTaskOfStep(step.Id);
                    List<object> copyTasks = new List<object>();
                    foreach (var task in tasks)
                    {
                        object input, file, form;
                        if (task.ValueInputText != null)
                        {
                            input = JObject.Parse(task.ValueInputText);
                            file = JObject.Parse(task.ValueInputFile);
                            form = task.ValueFormJson;
                        }
                        else
                        {
                            input = task.ValueInputText;
                            file = task.ValueInputFile;
                            form = JObject.Parse(task.ValueFormJson);
                        }

                        object copyTask = new
                        {
                            taskid = task.Id,
                            taskname = task.Name,
                            description = task.Description,
                            role = task.IdRole,
                            position = task.Position,
                            config = new
                            {
                                input = input,
                                file = file,
                                form = form
                            }
                        };
                        copyTasks.Add(copyTask);
                    }
                    //step
                    object copyStep = new
                    {
                        stepid = step.Id,
                        stepname = step.Name,
                        description = step.Description,
                        draw = new
                        {
                            isStartStep = step.StartStep,
                            key = step.Key,
                            figure = step.Figure,
                            nextstep1 = step.NextStep1,
                            nextstep2 = step.NextStep2
                        },
                        tasks = copyTasks
                    };
                    copySteps.Add(copyStep);
                }

                //process 
                object copyProcess = new
                {
                    processid = process.Id,
                    processname = process.Name,
                    description = process.Description,
                    draw = JObject.Parse(process.DataJson),
                    avatar = process.Avatar,
                    steps = copySteps,
                    roles = copyRoles
                };
                //create jsonfile
                fileService.createJsonFile(copyPath, copyProcess);
                //copy dirs and sub-dirs
                string processPath = string.Format("Upload/{0}/{1}", process.IdGroup, process.Id);
                fileService.copyDirectory(processPath, Path.Combine(copyPath, string.Format("upload/{0}", process.Id)), copyOnly: true, copySubDirs: true);
                string AppPath = AppDomain.CurrentDomain.BaseDirectory;


                //zip
                string fileName = string.Format("{0}.copy.zip", process.Name);
                string filePath = Path.Combine(AppPath, copyPath, fileName);
                using (ZipFile zip = new ZipFile())
                {
                    
                    zip.AlternateEncoding = System.Text.Encoding.Unicode;
                    zip.AddDirectory(Path.Combine(AppPath, copyPath));
                    zip.Save(filePath);
                }

                //create filemangement
                FileManager fn = new FileManager();
                fn.Id = commonService.getRandomString(50);
                fn.IdGroup = process.IdGroup;
                fn.Name = fileName;
                fn.Type = ".zip";
                fn.Path = copyPath;
                fn.Direction = Direction.Zip.ToString();
                fn.Create_At = DateTime.Now;
                fn.Update_At = DateTime.Now;
                db.FileManagers.Add(fn);
                db.SaveChanges();

                response = new { data = fn.Id, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                status = HttpStatusCode.InternalServerError;
                message = "Something not right";
                response = new { message = message, detail = e, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}