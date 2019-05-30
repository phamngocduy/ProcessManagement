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
using System.Text;
using System.Transactions;

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
        public JsonResult Editstep(int stepid, string des)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
        public JsonResult AddTask(int stepid, string name, int? idRole, string description, string inputConfig, string fileConfig, HttpPostedFileBase fileupload)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
                Role role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            TaskProcess task = taskService.addtask(step.Id, name, idRole, description, inputConfig, fileConfig);
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
        public JsonResult EditTask(string name, int? idRole, string description, string inputConfig, string fileConfig)
        {
            ///////////////////////////
            /// chỉ được edit task thuộc process mà mình quản lý
            ///////////////////////////
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            int idTask = int.Parse(Session["idTask"].ToString());
            TaskProcess task = taskService.findTask(idTask);


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
                Role role = roleService.findRoleOfProcess(idR, step.Process.Id);
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
            HttpStatusCode status = HttpStatusCode.OK;
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
                Role role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            TaskProcess task = taskService.AddFormTask(step.Id, name, idRole, description, formBuilder);
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
        public JsonResult EditFormTask(string name, int? idRole, string description, string formBuilder)
        {
            ///////////////////////////
            /// chỉ được edit task thuộc process mà mình quản lý
            ///////////////////////////
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            int idTask = int.Parse(Session["idTask"].ToString());
            TaskProcess task = taskService.findTask(idTask);


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
                Role role = roleService.findRoleOfProcess(idR, step.Process.Id);
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
        public JsonResult ChangeTaskPosition(string position)
        {
            taskService.changePosition(position);
            var response = new { message = "Change position sucess", status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProcessList(int groupid)
        {
            List<Process> processes = processService.getProcesses(groupid);
            List<object> jProcesses = new List<object>();
            foreach (Process process in processes.Where(x => x.IsRun == null || x.IsRun == false))
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
        public JsonResult ShowProcessList(string key, int groupid)
        {
            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToLower().Trim();
            }
            List<Process> processes = processService.searchProcesses(groupid, key, 5);
            List<object> jProcesses = new List<object>();
            foreach (Process process in processes.Where(x => x.IsRun == null || x.IsRun == false))
            {
                object jProcess = new
                {
                    id = process.Id,
                    text = process.Name,
                    avatar = process.Avatar,
                };
                jProcesses.Add(jProcess);
            }

            var response = new { results = jProcesses, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddProcessRun(int processid, string name,string des)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            Process process = processService.findProcess(processid);
            Process processrun = processService.createProcessRun(process, name, des);
            
            List<Role> role = roleService.findListRoleOfProcess(processid);
            List<Role> rolerun = roleService.addRoleRun(role, processrun.Id);
            List<Step> liststep = stepService.findStepsOfProcess(processid);
            List<Step> liststeprun = stepService.addStepRun(liststep, processrun.Id);
            foreach (Step step in liststep)
            {
                List<TaskProcess> listtask = taskService.findTaskOfStep(step.Id);
                taskService.addListTaskRun(listtask, rolerun, liststeprun);
            }
            message = "Created ProcessRun Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AssignRole(int processid, int roleid, string members)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
                foreach (string member in assignList)
                {
                    bool isMemberInGroup = participateService.checkMemberInGroup(member, role.Process.Group.Id);
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
        public JsonResult RunProcess(int idprocess)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
        public JsonResult AddNextStepInRunProcess(int idStep, int idnextstep, int idstepdiamond)
        {
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            Step stepchoosenext = stepService.findStep(idnextstep);
            Step idstepdk = stepService.findStep(idstepdiamond);
            StepRun runstep = stepService.findsteprun(idStep);
            stepService.changestatustep(runstep.Id, IdUser);
            ProcessRun processrun = processService.findProcessRun(runstep.idProcess);
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
        public JsonResult SubmitDoneTask(int idtask)
        {
            HttpStatusCode status = HttpStatusCode.OK;
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
        public JsonResult DeleteProcess(int processid)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                processService.removeProcess(processid);
                message = "Delete process Successfully";
                response = new { message = message, status = status };
                SetFlash(FlashType.success, "Removed Process Successfully");
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
        public JsonResult DeleteNextStepRun(int idStep)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            StepRun runstep = stepService.findsteprun(idStep);
            List<StepRun> liststeprun = stepService.findStepsOfRunProcess(runstep.idProcess);
            StepRun stepback = new StepRun();
            foreach (StepRun item in liststeprun)
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
        public JsonResult CompleteStepInRunProcess(int idStep)
        {
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            StepRun runstep = stepService.findsteprun(idStep);
            stepService.changestatustep(runstep.Id, IdUser);
            ProcessRun processrun = processService.findProcessRun(runstep.idProcess);
            List<Step> liststep = stepService.findStepsOfProcess(processrun.IdProcess);

            List<TaskProcessRun> listruntask = taskService.findruntaskofstep(idStep);
            List<TaskProcessRun> listtaskclose = listruntask.Where(x => x.Status1.Name == "Finish").ToList();

            List<Step> nextstep = new List<Step>();
            StepRun runnextstep = new StepRun();
            foreach (Step item in liststep)
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
            foreach (Step nexts in nextstep)
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
        public JsonResult Export(int processid)
        {
            string IdUser = User.Identity.GetUserId();
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                Process process = processService.findProcess(processid);
                if (process == null)
                {
                    throw new ServerSideException("Process not found");
                }
                //create folder
                string copyPath = string.Format("Copy/{0}", process.Id);
                fileService.removeDirectory(copyPath);
                fileService.createDirectory(copyPath);

                //role
                List<Role> roles = roleService.findListRoleOfProcess(processid);
                List<object> copyRoles = new List<object>();
                foreach (Role role in roles)
                {
                    object copyRole = new
                    {
                        rolename = role.Name,
                        description = role.Description ?? ""
                    };
                    copyRoles.Add(copyRole);
                }
                //step
                List<Step> steps = stepService.findStepsOfProcess(processid);
                List<object> copySteps = new List<object>();
                foreach (Step step in steps)
                {
                    
                    //task
                    List<TaskProcess> tasks = taskService.findTaskOfStep(step.Id);
                    List<object> copyTasks = new List<object>();
                    foreach (TaskProcess task in tasks)
                    {
                        //file
                        string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", process.IdGroup,process.Id,step.Id,task.Id);
                        List<FileManager> taskFiles = fileService.findFiles(process.IdGroup, taskPath);
                        List<string> fileTaskName = taskFiles.Select(x => x.Name).ToList();

                        object input = task.ValueInputText ?? "";
                        object file = task.ValueInputFile?? "";
                        object form = task.ValueFormJson ?? "";


                        if (input.ToString() != "")
                        {
                            input = JObject.Parse(input.ToString());
                        }
                        if (file.ToString() != "")
                        {
                            file = JObject.Parse(file.ToString());
                        }
                        if (form.ToString() != "")
                        {
                            form = JArray.Parse(form.ToString());
                        }

                        object copyTask = new
                        {
                            taskid = task.Id,
                            taskname = task.Name,
                            description = task.Description ?? "",
                            role = task.IdRole == null ? "" : task.Role.Name,
                            position = task.Position,
                            files = fileTaskName,
                            config = new
                            {
                                input = input,
                                file = file,
                                form = form,
                            }
                        };
                        copyTasks.Add(copyTask);
                    }

                    //file
                    string stepPath = string.Format("Upload/{0}/{1}/{2}", process.IdGroup,process.Id,step.Id);
                    List<FileManager> stepFiles = fileService.findFiles(process.IdGroup, stepPath);
                    List<string> fileStepName = stepFiles.Select(x => x.Name).ToList();

                    //step
                    object copyStep = new
                    {
                        stepid = step.Id,
                        stepname = step.Name,
                        description = step.Description ?? "",
                        draw = new
                        {
                            isStartStep = step.StartStep,
                            key = step.Key,
                            figure = step.Figure,
                            nextstep1 = step.NextStep1,
                            nextstep2 = step.NextStep2
                        },
                        files = fileStepName,
                        tasks = copyTasks
                    };
                    copySteps.Add(copyStep);
                }

                //file
                string processPath = string.Format("Upload/{0}/{1}", process.IdGroup, process.Id);
                List<FileManager> processFiles = fileService.findFiles(process.IdGroup, processPath);
                List<string> fileProcessName = processFiles.Select(x => x.Name).ToList();
                //process 
                object copyProcess = new
                {
                    processid = process.Id,
                    processname = process.Name,
                    description = process.Description ?? "",
                    draw = JObject.Parse(process.DataJson),
                    files = fileProcessName,
                    avatar = process.Avatar,
                    steps = copySteps,
                    roles = copyRoles
                };

                //create jsonfile
                fileService.createJsonFile(copyPath, copyProcess);

                //copy dirs and sub-dirs
                string uploadPath = Path.Combine(copyPath, string.Format("upload/Upload/{0}", process.Id));
                fileService.copyDirectory(processPath, uploadPath, copyOnly: true, copySubDirs: true);

                //zip
                string fileName = string.Format("{0}-download.pms", process.Name.ToLower());
                FileManager f = fileService.zipFile(groupid: process.IdGroup, fileName: fileName, copyPath);

                fileService.removeDirectory(Path.Combine(copyPath, "Upload"));
                fileService.removeFile(Path.Combine(copyPath, "data.json"));

                response = new { data = f.Id, status = status };
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
        public JsonResult Import(int groupid, HttpPostedFileBase fileupload)
        {
            string IdUser = User.Identity.GetUserId();
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            HttpStatusCode status = HttpStatusCode.OK;
            string message;
            object response;
            try
            {
                using (var scope = new TransactionScope())
                {
                    ZipFile zip = ZipFile.Read(fileupload.InputStream);
                    ZipEntry jsonFile = zip.FirstOrDefault(x => x.FileName == "data.json");
                    if (jsonFile == null)
                    {
                        throw new ServerSideException("File is damaged, please try other file");
                    }
                    //jsonFile.Password = "clockworks-pms";
                    var passsword = "clockworks-pms";
                    JObject data;
                    using (StreamReader sr = new StreamReader(jsonFile.OpenReader(passsword), Encoding.UTF8))
                    {
                        data = JsonConvert.DeserializeObject<JObject>(sr.ReadToEnd());
                    }
                    //xử lý data
                
                    //process
                    Process pr = new Process();
                    pr.Name = (string)data["processname"];
                    pr.Description = (string)data["description"];
                    pr.IdOwner = IdUser;
                    pr.IdGroup = groupid;
                    pr.DataJson = data["draw"].ToString();
                    pr.Avatar = (string)data["avatar"];
                    pr.IsRun = false;
                    pr.Created_At = DateTime.Now;   
                    pr.Updated_At = DateTime.Now;
                    db.Processes.Add(pr);
                    db.SaveChanges();

                    //file process
                    JArray processFile = (JArray)data["files"];
                    if (processFile.Any())
                    {
                        foreach (JToken file in processFile)
                        {
                            ZipEntry f = zip.FirstOrDefault(x => x.FileName == string.Format("Upload/{0}/{1}", (string)data["processid"], file.ToString()));
                            if (f != null)
                            {
                                Stream st = f.OpenReader();
                                ZipInputStream zp = new ZipInputStream(st);
                                string processPath = string.Format("Upload/{0}/{1}", groupid.ToString(), pr.Id.ToString());
                                fileService.createDirectory(processPath);
                                //System.IO.File.SetAttributes(Path.Combine(AppPath, processPath), FileAttributes.Temporary);
                                using (FileStream fileStream = System.IO.File.Create(string.Format("{0}/{1}/{2}", AppPath, processPath, file.ToString())))
                                {
                                    string fileId;
                                    do
                                    {
                                        fileId = commonService.getRandomString(50);
                                    } while (fileService.findFile(fileId) != null);
                                    FileManager fm = new FileManager();
                                    fm.Id = fileId;
                                    fm.IdGroup = groupid;
                                    fm.Name = Path.GetFileName(fileStream.Name);
                                    fm.Type = Path.GetExtension(fileStream.Name);
                                    fm.Path = processPath;
                                    fm.Direction = Direction.Process.ToString();
                                    fm.Create_At = DateTime.Now;
                                    fm.Update_At = DateTime.Now;
                                    db.FileManagers.Add(fm);
                                    //zp.Seek(0, SeekOrigin.Begin);
                                    zp.CopyTo(fileStream);
                                }
                            }
                        }
                    }
                    //role
                    JArray roles = (JArray)data["roles"];
                    List<Role> roleList = new List<Role>();
                    foreach (var role in roles)
                    {
                        Role rl = new Role { Process = pr };
                        rl.IdProcess = pr.Id;
                        rl.Name = (string)role["rolename"];
                        rl.Description = (string)role["description"];
                        rl.IsRun = false;
                        rl.Create_At = DateTime.Now;
                        rl.Update_At = DateTime.Now;
                        roleList.Add(rl);
                    }
                    db.Roles.AddRange(roleList);
                    db.SaveChanges();

                    JArray steps = (JArray)data["steps"];
                    foreach (JToken step in steps)
                    {
                        Step st = new Step { Process = pr };
                        st.IdProcess = pr.Id;
                        st.Name = (string)step["stepname"];
                        st.Description = (string)step["description"];
                        st.StartStep = (bool)step["draw"]["isStartStep"];
                        st.Key = (int)step["draw"]["key"];
                        st.Figure = (string)step["draw"]["figure"];
                        st.NextStep1 = (int)step["draw"]["nextstep1"];
                        st.NextStep2 = (int)step["draw"]["nextstep2"];
                        st.Color = commonService.getRandomColor();
                        st.IsRun = false;
                        st.Created_At = DateTime.Now;
                        st.Updated_At = DateTime.Now;
                        db.Steps.Add(st);
                        db.SaveChanges();
                        
                        //File Step
                        JArray stepFile = (JArray)step["files"];
                        if (stepFile.Any())
                        {
                            foreach (JToken file in stepFile)
                            {
                                ZipEntry f = zip.FirstOrDefault(x => x.FileName == string.Format("Upload/{0}/{1}/{2}", (string)data["processid"], (string)step["stepid"], file.ToString()));
                                if (f != null)
                                {
                                    Stream stm = f.OpenReader();
                                    ZipInputStream zp = new ZipInputStream(stm);
                                    string stepPath = string.Format("Upload/{0}/{1}/{2}", groupid.ToString(), pr.Id.ToString(), st.Id.ToString());
                                    fileService.createDirectory(stepPath);
                                    //System.IO.File.SetAttributes(Path.Combine(AppPath, processPath), FileAttributes.Temporary);
                                    using (FileStream fileStream = System.IO.File.Create(string.Format("{0}/{1}/{2}", AppPath, stepPath, file.ToString())))
                                    {
                                        string fileId;
                                        do
                                        {
                                            fileId = commonService.getRandomString(50);
                                        } while (fileService.findFile(fileId) != null);
                                        FileManager fm = new FileManager();
                                        fm.Id = fileId;
                                        fm.IdGroup = groupid;
                                        fm.Name = Path.GetFileName(fileStream.Name);
                                        fm.Type = Path.GetExtension(fileStream.Name);
                                        fm.Path = stepPath;
                                        fm.Direction = Direction.Step.ToString();
                                        fm.Create_At = DateTime.Now;
                                        fm.Update_At = DateTime.Now;
                                        db.FileManagers.Add(fm);

                                        //zp.Seek(0, SeekOrigin.Begin);
                                        zp.CopyTo(fileStream);
                                    }
                                }
                            }
                        }

                        //Task
                        JArray tasks = (JArray)step["tasks"];
                        foreach (JToken task in tasks)
                        {
                            int? rid;
                            if ((string)task["role"] == "") {
                                rid = null;
                            } else {
                                rid = roleList.First(x => x.Name == (string)task["role"]).Id;
                            }
                            string jText = task["config"]["input"].ToString();
                            string jFile = task["config"]["file"].ToString();
                            string jForm = task["config"]["form"].ToString();
                            TaskProcess tk = new TaskProcess { Step = st };
                            tk.IdStep = st.Id;
                            tk.IdRole = rid;
                            tk.Name = (string)task["taskname"];
                            tk.Description = (string)task["description"];
                            if (jText != "")
                                tk.ValueInputText = jText;
                            if(jFile != "")
                                tk.ValueInputFile = jFile;
                            if (jForm != "")
                                tk.ValueFormJson = jForm;
                            tk.Color = commonService.getRandomColor();
                            tk.Position = (int)task["position"];
                            tk.IsRun = false;
                            tk.Created_At = DateTime.Now;
                            tk.Updated_At = DateTime.Now;
                            db.TaskProcesses.Add(tk);
                            db.SaveChanges();
                            //file task
                            //File Step
                            JArray taskFile = (JArray)task["files"];
                            if (taskFile.Any())
                            {
                                foreach (JToken file in taskFile)
                                {
                                    ZipEntry f = zip.FirstOrDefault(x => x.FileName == string.Format("Upload/{0}/{1}/{2}/{3}", (string)data["processid"], (string)step["stepid"], (string)step["taskid"], file.ToString()));
                                    if (f != null)
                                    {
                                        Stream stm = f.OpenReader();
                                        ZipInputStream zp = new ZipInputStream(stm);
                                        string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", groupid.ToString(), pr.Id.ToString(), st.Id.ToString(), tk.Id.ToString());
                                        fileService.createDirectory(taskPath);
                                        //System.IO.File.SetAttributes(Path.Combine(AppPath, processPath), FileAttributes.Temporary);
                                        using (FileStream fileStream = System.IO.File.Create(string.Format("{0}/{1}/{2}", AppPath, taskPath, file.ToString())))
                                        {
                                            string fileId;
                                            do
                                            {
                                                fileId = commonService.getRandomString(50);
                                            } while (fileService.findFile(fileId) != null);
                                            FileManager fm = new FileManager();
                                            fm.Id = fileId;
                                            fm.IdGroup = groupid;
                                            fm.Name = Path.GetFileName(fileStream.Name);
                                            fm.Type = Path.GetExtension(fileStream.Name);
                                            fm.Path = taskPath;
                                            fm.Direction = Direction.Task.ToString();
                                            fm.Create_At = DateTime.Now;
                                            fm.Update_At = DateTime.Now;
                                            db.FileManagers.Add(fm);

                                            //zp.Seek(0, SeekOrigin.Begin);
                                            zp.CopyTo(fileStream);
                                        }
                                    }
                                }
                            }
                        }
                        db.SaveChanges();
                    }

                    //process
                    scope.Complete();
                }
                message = "Import Sucess";
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
    }
}