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
            return View(pr);
        }
        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult NewProcess(Process pro, HttpPostedFileBase FileUpload)
        {
            int idgroup = (int)Session["idgroup"];
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(idgroup);
            processService.createProcess(group.Id, idUser, pro);

            //create directory
            string directoryPath = String.Format("{0}/{1}", group.Id,pro.Id);
            fileService.CreateDirectory(directoryPath);
            //save file 
            string savePath = Server.MapPath(String.Format("~/App_Data/{0}/{1}", group.Id,pro.Id));
            string filePath = String.Format("{0}/{1}", group.Id, pro.Id);
            fileService.saveFile(FileUpload, filePath);

            SetFlash(FlashType.success, "Created Process Successfully");
            return RedirectToAction("Draw", new { groupslug = group.groupSlug, groupid = group.Id, processid = pro.Id });
            //return RedirectToRoute("GroupControlLocalizedDefault", new { action = "DrawProcess", groupslug = group.groupSlug, groupid = group.Id, id = pro.Id });
        }
        public ActionResult NewProcessRun(int groupid)
        {
            var group = groupService.findGroup(groupid);
            return View(group);
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult Draw(int processid)
        {
            Process ps = processService.findProcess(processid);
            if (ps == null) return HttpNotFound();
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
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            List<int> b = new List<int>();
            string circle = "Circle";
            for (int i = 0; i < nodeArray.Count; i++)
            {
                if (nodeArray[i]["figure"] != null)
                {
                    if (nodeArray[i]["figure"].ToString() == circle && nodeArray[i]["fill"].ToString() == "#CE0620")
                    {
                        b.Add((int)nodeArray[i]["key"]);
                    }
                }
            }
            for (int i = 0; i < nodeArray.Count; i++)
            {
                var key = (int)nodeArray[i]["key"];
                var from = linkArray.Where(x => (int)x["from"] == key).ToList();
                Step step = new Step();
                int j = 1;
                if (from != null)
                {
                    foreach (var item in from)
                    {
                        var to = (int)item["to"];
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
                    foreach (var tokey in b)
                    {
                        if (step.NextStep1 == tokey)
                        {
                            step.NextStep1 = 0;
                        }
                    }
                }

                step.IdProcess = processId;
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
                }
            }
            db.SaveChanges();
            return Json(new { id = ps.Id });
        }
        [GroupAuthorize]
        public ActionResult ShowStep(int processid)
        {
            string idUser = User.Identity.GetUserId();
            var process = processService.findProcess(processid);
            var group = groupService.findGroup(process.IdGroup);
            var listStep = stepService.findStepsOfProcess(processid);
            var listRole = processService.findListRole(process.Id);
            //statics
            dynamic expando = new ExpandoObject();
            var processStatisticModel = expando as IDictionary<string, object>;
            processStatisticModel.Add("totalstep", listStep.Count);
            processStatisticModel.Add("totalrole", listRole.Count);

            List<Step> listnextstep1 = new List<Step>();
            List<Step> listnextstep2 = new List<Step>();
            Step start = listStep.Where(x => x.StartStep == true).FirstOrDefault();
            listnextstep1.Add(start);
            int z = 0;
            int t = 0;
            for (int j = 0; j < listStep.Count; j++)
            {
                if (listnextstep1[j].NextStep1 == 0)
                {
                    break;
                }
                do
                {
                    if (listStep[z].Key == listnextstep1[j].NextStep2 && listStep[z].StartStep == false)
                    {
                        listnextstep2.Add(listStep[z]);
                        z = 0;
                        break;
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
            if (listnextstep2.Count() > 0)
            {
                for (int j = 0; j < listStep.Count; j++)
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
            foreach (var item in listnextstep2)
            {
                listnextstep1.Add(item);
            }
                

            ViewData["Group"] = group;
            ViewData["Process"] = process;
            ViewData["ListRole"] = listRole;
            ViewData["Statistic"] = processStatisticModel;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            return View(listnextstep1);
        }
        [GroupAuthorize]
        public ActionResult EditStep(int stepid)
        {
            var step = stepService.findStep(stepid);
            return View(step);
        }
        [GroupAuthorize]
        [HttpPost]
        public ActionResult EditStep(int groupid, Step model)
        {
            var group = groupService.findGroup(groupid);
            var step = db.Steps.Find(model.Id);
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

                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "createrole", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
            }
            var check = db.Roles.Where(x => x.Name.ToLower() == role.Name.ToLower().Trim() && x.IdProcess == processId).FirstOrDefault();
            if (check != null)
            {
                SetFlash(FlashType.error, "Name is exist in db");
                return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "createrole", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
            }

            role.IdProcess = process.Id;
            processService.createRole(role);
			SetFlash(FlashType.success, "Created Role Successfully");
			return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = process.Group.groupSlug, groupid = process.Group.Id, processid = process.Id });
		}
        [GroupAuthorize]
        public ActionResult DeleteRole(int roleid)
        {
            Role role = participateService.findRoleInProcess(roleid);
            if (role == null) return HttpNotFound();
            var roleId = role.Id;
            Process process = processService.findProcess(role.IdProcess);
            Group group = groupService.findGroup(process.IdGroup);
            participateService.removeRoleInProcess(role);
            SetFlash(FlashType.success, "Removed " + roleId + " Successfully");

            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "showstep", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
        }
        public ActionResult EditRole(int roleid)
        {
            Role role = participateService.findRoleInProcess(roleid);
            if (role == null) return HttpNotFound();
            var group = groupService.findGroup(role.Process.Group.Id);
            ViewData["Group"] = group;
            Session["roleid"] = role.Id;
            Session["processid"] = role.Process.Id;
            return View(role);
        }

        [GroupAuthorize]
        [HttpPost]
        public ActionResult EditRole(Role model)
        {
            model.Id = (int)Session["roleid"];
            var processid = (int)Session["processid"];
            Process process = processService.findProcess(processid);
            Group group = groupService.findGroup(process.Group.Id);
            participateService.editRole(model);
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
                var loadprocess = ps.DataJson.ToString();
                JObject load = JObject.Parse(loadprocess);
                ViewData["load"] = load;
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
			processService.EditProcess(model);
			SetFlash(FlashType.success, "Edited Information of " + process.Name + " Successfully");
			return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "EditInforProcess", processid = process.Id });

		}
		[Authorize]
        [HttpPost]
        public JsonResult EditProcess(int processId, string data, string nodeData, string linkData, string imageprocess)
        {
            Process ps = processService.findProcess(processId);
            processService.insertDataJson(ps, data, imageprocess);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            var liststep = db.Steps.Where(z => z.IdProcess == processId).ToList();
            List<int> keystep = new List<int>();
            List<int> keynode = new List<int>();
            List<int> b = new List<int>();
            string circle = "Circle";
            for (int i = 0; i < nodeArray.Count; i++)
            {
                if (nodeArray[i]["figure"] != null)
                {
                    if (nodeArray[i]["figure"].ToString() == circle && nodeArray[i]["fill"].ToString() == "#CE0620")
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
            foreach (var ls in liststep)
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
            foreach (var item in same)
            {
                Step s = db.Steps.Where(p => p.Key == item && p.IdProcess == processId).FirstOrDefault();
                keystepgiong.Add(s);
            }

            for (int i = 0; i < nodeArray.Count; i++)
            {
                var key = (int)nodeArray[i]["key"];
                var from = linkArray.Where(x => (int)x["from"] == key).ToList();
                Step step = new Step();
                int j = 1;
                if (from != null)
                {
                    foreach (var item1 in from)
                    {
                        var to = (int)item1["to"];
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
                    foreach (var tokey in b)
                    {
                        if (step.NextStep1 == tokey)
                        {
                            step.NextStep1 = 0;
                        }
                    }
                }

                step.IdProcess = processId;
                step.Name = nodeArray[i]["text"].ToString();
                step.Key = key;
                step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;

                step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
                step.Created_At = DateTime.Now;
                step.Updated_At = DateTime.Now;
                foreach (var item in diff)
                {
                    if (item == key)
                    {
                        keystepkhac.Add(step);
                    }
                    else
                    {
                        Step ste = stepService.findStepByKey(processId, item);
                        if (ste != null)
                        {
                            var listtaskofstep = db.TaskProcesses.Where(s => s.IdStep == ste.Id).ToList();
                            for (int s = 0; s < listtaskofstep.Count; s++)
                            {
                                db.TaskProcesses.Remove(listtaskofstep[s]);
                                db.SaveChanges();
                            }
                            stepService.removeStep(ste);
                        }
                    }
                }
            }
            foreach (var item3 in keystepkhac)
            {
                Step step = new Step();
                step.IdProcess = processId;
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
                }
            }

            foreach (var listst in keystepgiong)
            {
                for (int i = 0; i < nodeArray.Count; i++)
                {
                    var key = (int)nodeArray[i]["key"];
                    if (key == listst.Key)
                    {
                        var from = linkArray.Where(x => (int)x["from"] == key).ToList();
                        int j = 1;//if (from != null)
                        {
                            foreach (var item2 in from)
                            {
                                var to = (int)item2["to"];
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
                            foreach (var tokey in b)
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
            
            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idTask"] = task.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            return View(task);
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult DeleteTask(int taskid)
        {
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

            ViewData["Step"] = step;
            ViewData["ListRole"] = role;
            ViewData["Group"] = group;
            Session["idTask"] = task.Id;
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            return View(task);
        }

        [GroupAuthorize]
        public ActionResult AssignRole(int idprocess)
        {
            var processrun = processService.findProcess(idprocess);
            var listrole = roleService.findListRoleOfProcess(idprocess);
            var listuseringroup = participateService.findMembersInGroup(processrun.IdGroup);
            ViewData["ProcessRun"] = processrun;
            ViewBag.ListRole = listrole;
            ViewBag.ListUser = listuseringroup;
            return View();
        }

        [GroupAuthorize]
        public ActionResult Detail(int idprocess)
        {
            var processrun = processService.findProcess(idprocess);
            var listrole = roleService.findListRoleOfProcess(idprocess);
            var listroleruns = roleService.findlistrolerun(listrole);
            var listStep = stepService.findStepsOfProcess(idprocess);

            List<Step> listnextstep1 = new List<Step>();
            List<Step> listnextstep2 = new List<Step>();
            Step start = listStep.Where(x => x.StartStep == true).FirstOrDefault();
            listnextstep1.Add(start);
            int z = 0;
            int t = 0;
            for (int j = 0; j < listStep.Count; j++)
            {
                if (listnextstep1[j].NextStep1 == 0)
                {
                    break;
                }
                do
                {
                    if (listStep[z].Key == listnextstep1[j].NextStep2 && listStep[z].StartStep == false)
                    {
                        listnextstep2.Add(listStep[z]);
                        z = 0;
                        break;
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
            if (listnextstep2.Count() > 0)
            {
                for (int j = 0; j < listStep.Count; j++)
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
            foreach (var item in listnextstep2)
            {
                listnextstep1.Add(item);
            }
            
            ViewBag.ListRole = listrole;
            ViewData["ProcessRun"] = processrun;
            ViewBag.ListRoleRun = listroleruns;
            return View(listnextstep1);
        }

    }
}