using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Filters;
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
        public ActionResult NewProcess(Process pro)
        {
            int idgroup = (int)Session["idgroup"];
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(idgroup);
            processService.createProcess(group.Id, idUser, pro);
            SetFlash(FlashType.Success, "Created Process Successfully");
            return RedirectToAction("Draw", new { groupslug = group.groupSlug, groupid = group.Id, processid = pro.Id });
            //return RedirectToRoute("GroupControlLocalizedDefault", new { action = "DrawProcess", groupslug = group.groupSlug, groupid = group.Id, id = pro.Id });
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
        public JsonResult Draw(int processId, string data, string nodeData, string linkData)
        {
            Process ps = processService.findProcess(processId);
            //processService.insertDataJson(ps, data);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            for (int i = 0; i < nodeArray.Count; i++)
            {
                //if ((string)nodeArray[i]["fiqure"] != "Circle")
                //{
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
                    }

                    step.IdProcess = processId;
                    step.Name = nodeArray[i]["text"].ToString();
                    step.Key = key;
                    step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;

                    step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
                    step.Created_At = DateTime.Now;
                    step.Updated_At = DateTime.Now;
                    db.Steps.Add(step);
                    
                //}
            }
            //db.SaveChanges();
            return Json(new { id = ps.IdGroup });
        }
        public ActionResult ShowStep(int groupid, int id)
        {
            var group = groupService.findGroup(groupid);
            ViewData["step"] = stepService.findStepsOfProcess(id);
            return View(group);
        }

        public ActionResult EditStep(int groupid, int id)
        {
            var group = groupService.findGroup(groupid);
            var step = stepService.findStep(id);
            return View(step);
        }
        [HttpPost]
        public ActionResult EditStep(int groupid, Step model)
        {
            var group = groupService.findGroup(groupid);
            var step = db.Steps.Find(model.Id);
            step.Description = model.Description;
            db.SaveChanges();
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "ShowStep", groupslug = group.groupSlug, groupid = group.Id, id = step.Process.Id });
        }
		[GroupAuthorize(Role = new UserRole[] { UserRole.Manager })]
		public ActionResult CreateRole(int processid)
		{
			Process process = processService.findProcess(processid);
			return View(process);
		}
		[HttpPost]
		public ActionResult CreateRole(int IdProcess, Role role)
		{
			role.IdProcess = IdProcess;
			processService.createRole(role);
			Process process = processService.findProcess(IdProcess);
			Group group = groupService.findGroup(process.IdGroup);
			//set flash
			SetFlash(FlashType.Success, "Created Role Successfully");
			return RedirectToRoute("GroupControlLocalizedDefault", new {controller = "process", action = "createrole", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
		}
		public ActionResult EditRole()
		{
			return View();
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

        [Authorize]
        [HttpPost]
        public JsonResult EditProcess(int processId, string data, string nodeData, string linkData)
        {
            Process ps = processService.findProcess(processId);
            //processService.insertDataJson(ps, data);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            var liststep = db.Steps.Where(z => z.IdProcess == processId).ToList();
            List<int> keystep = new List<int>();
            List<int> keynode = new List<int>();

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
                    if (key == item)
                    {
                        keystepkhac.Add(step);
                    }
                    else
                    {
                            Step st = db.Steps.Where(p => p.Key == item && p.IdProcess == processId).FirstOrDefault();
                            db.Steps.Remove(st);
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
                db.Steps.Add(step);
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
                        }
                        listst.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
                        listst.Name = nodeArray[i]["text"].ToString();
                        listst.Updated_At = DateTime.Now;
                    }
                }
            }

            //db.SaveChanges();
            return Json(new { id = ps.IdGroup });
        }
    }
}