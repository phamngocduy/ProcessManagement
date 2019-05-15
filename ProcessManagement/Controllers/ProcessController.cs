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

namespace ProcessManagement.Controllers
{
    public class ProcessController : BaseController
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

        [Authorize]
        [GroupAuthorize]
        public ActionResult NewProcess(int groupid)
        {
            Group group = groupService.findGroup(groupid);
            Process pr = new Process();
            ViewData["group"] = group;
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View(pr);
        }
        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult NewProcess(int groupid, Process pro, HttpPostedFileBase FileUpload)
        {
            string idUser = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(pro.Name))
            {
                SetFlash(FlashType.error, "Process Name is required");
                return View();
            }
            ConfigRule fileSizeRule = db.ConfigRules.Find("filesize");
            bool isFileOverSize = fileService.checkFileOverSize(FileUpload);
            if (isFileOverSize)
            {
                SetFlash(FlashType.error, string.Format("This file is too big ({0} {1} maximum)", fileSizeRule.Value, fileSizeRule.Unit));
                return View();
            }
            Group group = groupService.findGroup(groupid);
            processService.createProcess(group.Id, idUser, pro);

            //create directory
            string directoryPath = String.Format("Upload/{0}/{1}", group.Id, pro.Id);
            fileService.createDirectory(directoryPath);
            //save file 
            //string savePath = Server.MapPath(String.Format("~/App_Data/{0}/{1}", group.Id,pro.Id));
            string filePath = String.Format("Upload/{0}/{1}", group.Id, pro.Id);
            fileService.saveFile(group.Id, FileUpload, filePath, Direction.Process);

            SetFlash(FlashType.success, "Created Process Successfully");
            return RedirectToAction("Draw", new { groupslug = group.groupSlug, groupid = group.Id, processid = pro.Id });
            //return RedirectToRoute("GroupControlLocalizedDefault", new { action = "DrawProcess", groupslug = group.groupSlug, groupid = group.Id, id = pro.Id });
        }
        public ActionResult NewProcessRun(int groupid)
        {
            Group group = groupService.findGroup(groupid);
            return View(group);
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult Draw(int processid)
        {
            Process ps = processService.findProcess(processid);
            if (ps == null) return HttpNotFound();
            if (ps.Steps.Count > 0)
            {
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "editprocess", processid = ps.Id });
            }
            return View(ps);
        }
        [Authorize]
        [HttpPost]
        public JsonResult Draw(int processId, string data, string nodeData, string linkData, string imageprocess)
        {
            Process ps = processService.findProcess(processId);
            processService.insertDataJson(ps, data, imageprocess);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            JToken idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            List<int> b = new List<int>();
            string circle = "Circle";
            for (int i = 0; i < nodeArray.Count; i++)
            {
                if (nodeArray[i]["figure"] != null)
                {
                    if (nodeArray[i]["figure"].ToString() == circle && nodeArray[i]["fill"].ToString() == "#f20000")
                    {
                        b.Add((int)nodeArray[i]["key"]);
                    }
                }
            }
            for (int i = 0; i < nodeArray.Count; i++)
            {
                int key = (int)nodeArray[i]["key"];
                List<JToken> from = linkArray.Where(x => (int)x["from"] == key).ToList();
                Step step = new Step();
                int j = 1;
                if (from != null)
                {
                    foreach (JToken item in from)
                    {
                        int to = (int)item["to"];
                        if (j == 1)
                        {
                            step.NextStep1 = to;
                        }
                        else if (j == 2)
                        {
                            step.NextStep2 = to;
                        }
                        j++;
                    }
                    if (step.NextStep1 == null)
                    {
                        step.NextStep1 = 0;
                    }
                    if (step.NextStep2 == null)
                    {
                        step.NextStep2 = 0;
                    }
                    foreach (int tokey in b)
                    {
                        if (step.NextStep1 == tokey)
                        {
                            step.NextStep1 = 0;
                        }
                    }
                }

                step.IdProcess = ps.Id;
                step.Name = nodeArray[i]["text"].ToString().Trim();
                step.Key = key;
                step.IsRun = false;
                step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
                step.Color = commonService.getRandomColor();
                step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
                step.Created_At = DateTime.Now;
                step.Updated_At = DateTime.Now;
                if (step.Figure != "Circle")
                {
                    db.Steps.Add(step);
                    db.SaveChanges();
                    string directoryPath = String.Format("Upload/{0}/{1}/{2}", ps.Group.Id, ps.Id, step.Id);
                    fileService.createDirectory(directoryPath);
                }
            }
            return Json(new { id = ps.Id });
        }
        [GroupAuthorize]
        public ActionResult ShowStep(int processid)
        {
            string idUser = User.Identity.GetUserId();
            Process process = processService.findProcess(processid);
            Group group = groupService.findGroup(process.IdGroup);
            List<Step> listStep = stepService.findStepsOfProcess(processid);
            List<Role> listRole = processService.findListRole(process.Id);
            //statics
            //dynamic expando = new ExpandoObject();
            //var processStatisticModel = expando as IDictionary<string, object>;
            //processStatisticModel.Add("totalstep", listStep.Count);
            //processStatisticModel.Add("totalrole", listRole.Count);

            //tìm file group
            string processPath = string.Format("Upload/{0}/{1}", group.Id, process.Id);
            List<FileManager> files = fileService.findFiles(group.Id, processPath);

            List<Step> listnextstep1 = new List<Step>();
            List<Step> listnextstep2 = new List<Step>();
            Step start = listStep.Where(x => x.StartStep == true).FirstOrDefault();
            if (start != null)
            {
                listnextstep1.Add(start);
            }
            int z = 0;
            int t = 0;
            for (int j = 0; j < listStep.Count; j++)
            {
                if (j < listnextstep1.Count())
                {
                    if (listnextstep1[j].NextStep1 == 0)
                    {
                        break;
                    }
                    do
                    {
                        if (z == listStep.Count)
                        {
                            z = 0;
                        }
                        if (listStep[z].Key == listnextstep1[j].NextStep2 && listStep[z].StartStep == false)
                        {
                            listnextstep2.Add(listStep[z]);
                            if (listnextstep1[j].Figure == "Diamond")
                            {
                            }
                            else
                            {
                                z = 0;
                                break;
                            }
                        }
                        if (listStep[z].Key == listnextstep1[j].NextStep1 && listStep[z].StartStep == false)
                        {
                            listnextstep1.Add(listStep[z]);
                            if (listnextstep1[j].Figure == "Diamond")
                            {
                            }
                            else
                            {
                                z = 0;
                                break;
                            }
                        }
                        z++;
                    } while (z < listStep.Count);
                }
            }
            if (listnextstep2.Count() > 0)
            {
                for (int j = 0; j < listStep.Count; j++)
                {
                    if (j < listnextstep2.Count())
                    {
                        if (listnextstep2[j].NextStep1 == 0)
                        {
                            break;
                        }
                        do
                        {
                            if (listStep[t].Key == listnextstep2[j].NextStep1 && listStep[t].StartStep == false)
                            {
                                listnextstep2.Add(listStep[t]);
                                t = 0;
                                break;
                            }
                            t++;
                        } while (t < listStep.Count);
                    }
                }
            }
            List<Step> liststepgiong = new List<Step>();
            for (int i = 0; i < listnextstep1.Count; i++)
            {
                for (int j = 0; j < listnextstep2.Count; j++)
                {
                    if (listnextstep1[i].Key == listnextstep2[j].Key)
                    {
                        liststepgiong.Add(listnextstep2[j]);
                    }
                }
            }

            for (int i = 0; i < listnextstep2.Count; i++)
            {
                if (liststepgiong.Count != 0 && listnextstep2.Count != 0)
                {
                    for (int j = 0; j < liststepgiong.Count; j++)
                    {
                        if (listnextstep2[i].Key == liststepgiong[j].Key)
                        {
                            listnextstep2.Remove(listnextstep2[i]);
                        }
                    }
                }
            }
            //hàm xóa các phần tử giống nhau trong mảng
            //cho list 1
            IEnumerable<Step> gionglist1 = listnextstep1.Distinct();
            listnextstep1 = gionglist1.ToList();
            // cho list 2
            IEnumerable<Step> gionglist2 = listnextstep2.Distinct();
            listnextstep2 = gionglist2.ToList();

            foreach (Step item in listnextstep2)
            {
                listnextstep1.Add(item);
            }
            List<Step> liststepshow = listnextstep1.Where(x => x.Figure == "Step" && (x.IsRun == false || x.IsRun == null)).ToList();
            
            ViewData["Group"] = group;
            ViewData["Process"] = process;
            ViewData["ListRole"] = listRole;
            //ViewData["Statistic"] = processStatisticModel;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["Files"] = files;
            ViewData["listnextstep1"] = liststepshow;
            //get maximum file config
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View();
        }
        [GroupAuthorize]
        public ActionResult EditStep(int stepid)
        {
            string idUser = User.Identity.GetUserId();
            Step step = stepService.findStep(stepid);
            if (step == null) return HttpNotFound();
            Group group = groupService.findGroup(step.Process.Group.Id);

            //file 
            string stepPath = string.Format("Upload/{0}/{1}/{2}", group.Id, step.Process.Id, step.Id);
            List<FileManager> files = fileService.findFiles(group.Id, stepPath);

            ViewData["Group"] = group;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["Files"] = files;
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View(step);
        }
        [GroupAuthorize]
        [HttpPost]
        public ActionResult EditStep(int groupid, Step model)
        {
            Group group = groupService.findGroup(groupid);
            Step step = db.Steps.Find(model.Id);
            step.Description = model.Description;
            db.SaveChanges();
            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "ShowStep", groupslug = group.groupSlug, groupid = group.Id, processid = step.Process.Id });
        }
        [GroupAuthorize]
        public ActionResult AddRole(int processid)
        {
            Process process = processService.findProcess(processid);
            if (process == null) return HttpNotFound();
            Group group = groupService.findGroup(process.Group.Id);
            //Tìm tất cả các role thuộc quy trình đó
            ViewData["ListRole"] = processService.findListRole(process.Id);
            ViewData["Group"] = group;
            Session["processid"] = process.Id;
            return View(process);
        }
        [GroupAuthorize]
        [HttpPost]
        public ActionResult AddRole(Role role)
        {
            int processId = (int)Session["processid"];
            Process process = processService.findProcess(processId);
            if (role.Name == null)
            {
                SetFlash(FlashType.error, "Name is required");

                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "addrole", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
            }
            bool isExist = roleService.isNameExist(role, process.Id);
            if (isExist)
            {
                SetFlash(FlashType.error, "This name is exist in process");
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "addrole", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
            }

            role.IdProcess = process.Id;
            processService.createRole(role);
            SetFlash(FlashType.success, "Created Role Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
        }
        [GroupAuthorize]
        public ActionResult DeleteRole(int roleid)
        {
            Role role = roleService.findRole(roleid);
            if (role == null) return HttpNotFound();
            int roleId = role.Id;
            Process process = processService.findProcess(role.IdProcess);
            Group group = groupService.findGroup(process.IdGroup);
            roleService.removeRole(role);
            SetFlash(FlashType.success, "Removed " + roleId + " Successfully");

            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
        }
        [GroupAuthorize]
        public ActionResult EditRole(int roleid)
        {
            Role role = roleService.findRole(roleid);
            if (role == null) return HttpNotFound();
            Group group = groupService.findGroup(role.Process.Group.Id);
            ViewData["Group"] = group;
            Session["roleid"] = role.Id;
            Session["processid"] = role.Process.Id;
            return View(role);
        }

        [GroupAuthorize]
        [HttpPost]
        public ActionResult EditRole(Role role)
        {
            role.Id = (int)Session["roleid"];
            int processid = (int)Session["processid"];
            Process process = processService.findProcess(processid);
            Group group = groupService.findGroup(process.Group.Id);

            if (role.Name == null)
            {
                SetFlash(FlashType.error, "Name is required");
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "editrole", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
            }
            bool isExist = roleService.isNameExist(role, process.Id);
            if (isExist)
            {
                SetFlash(FlashType.error, "This name is exist in process");
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "editrole", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
            }

            roleService.editRole(role);
            SetFlash(FlashType.success, "Edited Role Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult EditProcess(int processid)
        {
            Process ps = processService.findProcess(processid);
            if (ps == null) return HttpNotFound();
            if (ps.DataJson != null)
            {
                string loadprocess = ps.DataJson.ToString();
                JObject load = JObject.Parse(loadprocess);
                ViewData["load"] = load;
            }
            if (ps.Steps.Count < 1)
            {
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "draw", processid = ps.Id });
            }
            return View(ps);
        }
        public ActionResult Setting(int processid)
        {
            Process process = processService.findProcess(processid);
            if (process == null) return HttpNotFound();

            Group group = groupService.findGroup(process.Group.Id);
            ViewData["Group"] = group;
            Session["processid"] = process.Id;
            return View(process);
        }
        [HttpPost]
        [GroupAuthorize]
        public ActionResult Setting(Process model)
        {
            int processId = (int)Session["processid"];
            Process process = processService.findProcess(processId);
			if (process == null) return HttpNotFound();
            //edit
            Group group = groupService.findGroup(process.IdGroup);
            
			processService.EditProcess(process.Id, model);
			SetFlash(FlashType.success, "Edit Process Successfully");
			return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });

        }
        [Authorize]
        [HttpPost]
        public JsonResult EditProcess(int processId, string data, string nodeData, string linkData, string imageprocess)
        {
            Process ps = processService.findProcess(processId);
            processService.insertDataJson(ps, data, imageprocess);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            JToken idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            List<Step> liststep = db.Steps.Where(z => z.IdProcess == ps.Id).ToList();
            List<int> keystep = new List<int>();
            List<int> keynode = new List<int>();
            List<int> b = new List<int>();
            string circle = "Circle";
            for (int i = 0; i < nodeArray.Count; i++)
            {
                if (nodeArray[i]["figure"] != null)
                {
                    if (nodeArray[i]["figure"].ToString() == circle && nodeArray[i]["fill"].ToString() == "#f20000")
                    {
                        b.Add((int)nodeArray[i]["key"]);
                    }
                }
            }

            //chuyển linkstep về mặc định
            //add new step nếu step chưa có trong liststep
            //so sánh 2 list tìm giá trị giống nhau và khác nhau
            //nếu 2 list có các phần tử giống nhau sẽ lưu vào 1 mảng khác để sử giá trị của step đó
            //nếu 2 list có các phần tử khác nhau sẽ lưu vào 1 mảng khác để tạo mới step đó
            //sau đó kt lại nếu step nào không có nextstep1 and nextstep2,step đó sẽ đc xóa
            foreach (Step ls in liststep)
            {
                ls.StartStep = false;
                ls.NextStep1 = 0;
                ls.NextStep2 = 0;
                keystep.Add(ls.Key);
            }

            for (int i = 0; i < nodeArray.Count; i++)
            {
                keynode.Add((int)nodeArray[i]["key"]);
            }

            int[] same = keynode.Intersect(keystep).ToArray();
            int[] diff = keynode.Union(keystep).Except(same).ToArray();

            List<Step> keystepgiong = new List<Step>();
            List<Step> keystepkhac = new List<Step>();
            foreach (int item in same)
            {
                Step s = db.Steps.Where(p => p.Key == item && p.IdProcess == ps.Id).FirstOrDefault();
                keystepgiong.Add(s);
            }

            for (int i = 0; i < nodeArray.Count; i++)
            {
                int key = (int)nodeArray[i]["key"];
                List<JToken> from = linkArray.Where(x => (int)x["from"] == key).ToList();
                Step step = new Step();
                int j = 1;
                if (from != null)
                {
                    foreach (JToken item1 in from)
                    {
                        int to = (int)item1["to"];
                        if (j == 1)
                        {
                            step.NextStep1 = to;
                        }
                        else if (j == 2)
                        {
                            step.NextStep2 = to;
                        }
                        j++;
                    }
                    if (step.NextStep2 == null)
                    {
                        step.NextStep2 = 0;
                    }
                    if (step.NextStep1 == null)
                    {
                        step.NextStep1 = 0;
                    }
                    foreach (int tokey in b)
                    {
                        if (step.NextStep1 == tokey)
                        {
                            step.NextStep1 = 0;
                        }
                    }
                }

                step.IdProcess = ps.Id;
                step.Name = nodeArray[i]["text"].ToString();
                step.Key = key;
                step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
                step.IsRun = false;
                step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
                step.Created_At = DateTime.Now;
                step.Updated_At = DateTime.Now;
                foreach (int item in diff)
                {
                    if (item == key)
                    {
                        keystepkhac.Add(step);
                    }
                    else
                    {
                        Step ste = stepService.findStepByKey(ps.Id, item);
                        if (ste != null)
                        {
                            List<TaskProcess> listtaskofstep = db.TaskProcesses.Where(s => s.IdStep == ste.Id).ToList();
                            for (int s = 0; s < listtaskofstep.Count; s++)
                            {
                                db.TaskProcesses.Remove(listtaskofstep[s]);
                                db.SaveChanges();

                            }
                            string stepPath = string.Format("Upload/{0}/{1}/{2}", ps.Group.Id, ps.Id, ste.Id);
                            fileService.removeDirectory(stepPath);
                            stepService.removeStep(ste);
                        }
                    }
                }
            }
            foreach (Step item3 in keystepkhac)
            {
                Step step = new Step();
                step.IdProcess = ps.Id;
                step.Name = item3.Name;
                step.Key = item3.Key;
                step.StartStep = item3.StartStep;
                step.Figure = item3.Figure;
                step.Created_At = item3.Created_At;
                step.Updated_At = item3.Updated_At;
                step.NextStep1 = item3.NextStep1;
                step.NextStep2 = item3.NextStep2;
                step.Color = commonService.getRandomColor();
                if (step.Figure != "Circle")
                {
                    db.Steps.Add(step);
                    db.SaveChanges();
                    string stepPath = string.Format("Upload/{0}/{1}/{2}", ps.Group.Id, ps.Id, step.Id);
                    fileService.createDirectory(stepPath);
                }
            }

            foreach (Step listst in keystepgiong)
            {
                for (int i = 0; i < nodeArray.Count; i++)
                {
                    int key = (int)nodeArray[i]["key"];
                    if (key == listst.Key)
                    {
                        List<JToken> from = linkArray.Where(x => (int)x["from"] == key).ToList();
                        int j = 1;//if (from != null)
                        {
                            foreach (JToken item2 in from)
                            {
                                int to = (int)item2["to"];
                                if (j == 1)
                                {
                                    listst.NextStep1 = to;
                                }
                                else if (j == 2)
                                {
                                    listst.NextStep2 = to;
                                }
                                j++;
                            }
                            if (listst.NextStep2 == null)
                            {
                                listst.NextStep2 = 0;
                            }
                            if (listst.NextStep1 == null)
                            {
                                listst.NextStep1 = 0;
                            }
                            foreach (int tokey in b)
                            {
                                if (listst.NextStep1 == tokey)
                                {
                                    listst.NextStep1 = 0;
                                }
                            }
                        }
                        listst.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
                        listst.Name = nodeArray[i]["text"].ToString();
                        listst.Updated_At = DateTime.Now;
                    }
                }
            }

            db.SaveChanges();
            return Json(new { id = ps.IdGroup });
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult AddTask(int stepid)
        {
            string idUser = User.Identity.GetUserId();
            Step step = stepService.findStep(stepid);
            if (step == null) return HttpNotFound();
            Process ps = processService.findProcess(step.IdProcess);
            Group group = groupService.findGroup(ps.IdGroup);
            TaskProcess pr = new TaskProcess();
            List<Role> role = db.Roles.Where(x => x.IdProcess == step.IdProcess).ToList();
            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idStep"] = step.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View(pr);
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult ShowTask(int taskid)
        {
            string idUser = User.Identity.GetUserId();
            TaskProcess task = taskService.findTask(taskid);
            if (task == null) return HttpNotFound();
            Step step = stepService.findStep(task.IdStep);
            Group group = groupService.findGroup(step.Process.Group.Id);
            List<Role> role = db.Roles.Where(x => x.IdProcess == task.Step.Process.Id).ToList();

            //file 
            string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", group.Id, task.Step.Process.Id, task.Step.Id, task.Id);
            List<FileManager> files = fileService.findFiles(group.Id, taskPath);


            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idTask"] = task.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["Files"] = files;
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");

            if (task.ValueFormJson != null)
            {
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "ShowFormTask", taskid = taskid });
            }

            return View(task);
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult DeleteTask(int taskid)
        {
            //TODO: Check lại
            TaskProcess task = taskService.findTask(taskid);
            if (task == null) return HttpNotFound();
            Step step = stepService.findStep(task.Step.Id);
            //lấy group thuộc process
            int idGroup = task.Step.Process.Group.Id;
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(idGroup);

            //check xem có thuộc process trong group
            Participate user = participateService.findMemberInGroup(idUser, idGroup);
            if (user == null) return HttpNotFound();

            string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", task.Step.Process.Group.Id, task.Step.Process.Id, task.Step.Id, task.Id);
            fileService.removeDirectory(taskPath);

            taskService.deletetask(task);

            SetFlash(FlashType.success, "Delete Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "Process", action = "ShowStep", groupslug = group.groupSlug, groupid = group.Id, processid = step.IdProcess });
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult AddFormTask(int stepid)
        {
            string idUser = User.Identity.GetUserId();
            Step step = stepService.findStep(stepid);
            if (step == null) return HttpNotFound();
            Process ps = processService.findProcess(step.IdProcess);
            Group group = groupService.findGroup(ps.IdGroup);
            TaskProcess pr = new TaskProcess();
            List<Role> role = db.Roles.Where(x => x.IdProcess == step.IdProcess).ToList();
            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idStep"] = step.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View(pr);
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult ShowFormTask(int taskid)
        {
            string idUser = User.Identity.GetUserId();
            TaskProcess task = taskService.findTask(taskid);
            if (task == null) return HttpNotFound();
            Step step = stepService.findStep(task.IdStep);
            Group group = groupService.findGroup(step.Process.Group.Id);
            List<Role> role = db.Roles.Where(x => x.IdProcess == task.Step.Process.Id).ToList();

            //file
            string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", group.Id, task.Step.Process.Id, task.Step.Id, task.Id);
            List<FileManager> files = fileService.findFiles(group.Id, taskPath);

            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idTask"] = task.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["Files"] = files;
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            if (task.ValueFormJson == null)
            {
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "ShowTask", taskid = taskid });
            }
            return View(task);
        }
        
    }
}