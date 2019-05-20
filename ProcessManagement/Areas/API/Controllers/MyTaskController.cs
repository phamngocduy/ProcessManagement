using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Filters;
using System.Dynamic;
using System.Web;
using System.IO;

namespace ProcessManagement.Areas.API.Controllers
{
    public class MyTaskController : ProcessManagement.Controllers.BaseController
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
        FileService fileService = new FileService();
        ///=============================================================================================

        // GET: MyTask
        public PartialViewResult ListTaskInProcess(int groupid)
        {
            string idUser = User.Identity.GetUserId();
            List<Process> listprocess = processService.findListProcess(groupid);
            //if (listprocess.Count == 0) return HttpNotFound();
            //tìm role user trong group
            Participate userrole = participateService.getRoleOfMember(idUser, groupid);

            //tìm tất cả các role nào user đc phân trong processrun
            List<RoleRun> listrolerun = roleService.findlistrolerunbyiduser(idUser);
            List<Role> listrole = new List<Role>();
            foreach (var rolerun in listrolerun)
            {
                Role role = roleService.findrolebyidrolerun(rolerun);
                listrole.Add(role);
            }

            //tìm tất cả các task user đc gán trong từng process
            // sau đó kiểm trả các task nào user đc gán bằng roleid trong list task trên
            // => tìm ra đc các task user đc gán có trong từng process
            List<ProcessRun> listprocessrun = new List<ProcessRun>();
            List<object> jListProcessRun = new List<object>();
            foreach (var process in listprocess)
            {
                ProcessRun processrun = processService.findRunProcessbyidprorun(process.Id);
                if (processrun != null)
                {
                    listprocessrun.Add(processrun);
                }
            }

            foreach (var processrun in listprocessrun)
            {
                List<StepRun> liststeprun = stepService.findStepsOfRunProcess(processrun.Id);
                //tạo Jprocessrun
                List<object> jlisttaskrunopen = new List<object>();
                List<object> jlisttaskrundone = new List<object>();
                List<object> jlisttaskrunfinish = new List<object>();
                foreach (var steprun in liststeprun)
                {
                    List<TaskProcessRun> listtaskrun = taskService.findruntaskofstep(steprun.Id);
                    if (listtaskrun.Any())
                    {
                        foreach (var taskrun in listtaskrun)
                        {

                            object jTaskRun = new
                            {
                                id = taskrun.Id,
                                name = taskrun.Name,
                                description = taskrun.Description,
                                rolename = taskrun.Role.Name,
                                status = taskrun.Status1.Name,
                                valueform = taskrun.ValueFormJson
                            };
                            if (userrole.IsManager)
                            {
                                if (taskrun.Status1.Name == "Open")
                                {
                                    jlisttaskrunopen.Add(jTaskRun);
                                }
                                if (taskrun.Status1.Name == "Done")
                                {
                                    jlisttaskrundone.Add(jTaskRun);
                                }
                                if (taskrun.Status1.Name == "Finish")
                                {
                                    jlisttaskrunfinish.Add(jTaskRun);
                                }
                            }
                            else
                            {
                                foreach (var role in listrole)
                                {
                                    if (taskrun.IdRole == role.Id)
                                    {
                                        if (taskrun.Status1.Name == "Open")
                                        {
                                            jlisttaskrunopen.Add(jTaskRun);
                                        }
                                        if (taskrun.Status1.Name == "Done")
                                        {
                                            jlisttaskrundone.Add(jTaskRun);
                                        }
                                        if (taskrun.Status1.Name == "Finish")
                                        {
                                            jlisttaskrunfinish.Add(jTaskRun);
                                        }
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }

                object jprocessrun = new {
                    id = processrun.Id,
                    name = processrun.Name,
                    status = processrun.Status1.Name,
                    task = new
                    {
                        open = jlisttaskrunopen,
                        done = jlisttaskrundone,
                        finish = jlisttaskrunfinish
                    }
                };
                jListProcessRun.Add(jprocessrun);
            }
            //khúc này nó sẽ return cái html về client
            //nên mình phải gán data vào view
            var listprocesses = JArray.FromObject(jListProcessRun);
            //ViewData["ListProcessRun"] = JArray.FromObject(jListProcessRun);
            return PartialView(listprocesses);
        }
    }
}