﻿using System;
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
            processService.insertDataJson(ps, data);
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
                step.Name = nodeArray[i]["text"].ToString();
                step.Key = key;
                step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;

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
            var process = processService.findProcess(processid);
            var group = groupService.findGroup(process.IdGroup);
            ViewData["group"] = group;
            ViewData["process"] = process;

            return View(stepService.findStepsOfProcess(processid));
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
        public ActionResult CreateRole(int processid)
        {
            Process process = processService.findProcess(processid);
            //Tìm tất cả các role thuộc quy trình đó
            ViewData["ListRole"] = processService.findListRole(process.Id);
            return View(process);
        }
        [GroupAuthorize]
        [HttpPost]
        public ActionResult CreateRole(int IdProcess, Role role)
        {
            role.IdProcess = IdProcess;
            processService.createRole(role);
            Process process = processService.findProcess(IdProcess);
            Group group = groupService.findGroup(process.IdGroup);
            //set flash
            SetFlash(FlashType.Success, "Created Role Successfully");
            return View();
            //return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "createrole", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
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
            SetFlash(FlashType.Success, "Removed " + roleId + " Successfully");

            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "createrole", groupslug = group.groupSlug, groupid = group.Id, processid = process.Id });
        }
        public ActionResult EditRole(int roleid)
        {
            Role role = participateService.findRoleInProcess(roleid);
            if (role == null) return HttpNotFound();
            return View(role);
        }

        [HttpPost]
        [GroupAuthorize]
        public ActionResult EditRole(Role model)
        {
            Process process = processService.findProcess(model.IdProcess);
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            Role role = participateService.findRole(model.Id);
            if (role == null) return HttpNotFound();
            participateService.editRole(model);
            SetFlash(FlashType.Success, "Edited Role of " + role.Name + " Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { controller = "process", action = "createrole", groupslug = group.groupSlug, groupid = group.Id, processid = role.IdProcess });

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
            processService.insertDataJson(ps, data);
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
                    Step ste = stepService.findStepByKey(processId, item);
                    if (ste != null)
                    {
                        stepService.removeStep(ste);
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
    }
}