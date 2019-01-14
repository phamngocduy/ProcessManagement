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
        //[GroupAuthorize]
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
        public JsonResult Draw(int processId, string data, string nodeData, string linkData)
        {
            Process ps = processService.findProcess(processId);
            processService.insertDataJson(ps, data);
            //JObject json = JObject.Parse(nodeData);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            List<int> a = new List<int>();
            for (int i = 0; i < nodeArray.Count; i++)
            {
                var key = (int)nodeArray[i]["key"];
                var from = linkArray.Where(x => (int)x["from"] == key).ToList();
                var nextStep2 = 0;
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
                        //if (!a.Contains(to))
                        //{
                        //    a.Add(to);
                        //}
                        //else
                        //{
                        //    nextStep2 = to;
                        //    a.Add(to);
                        //}
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
            }
            //db.Entry(ps).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
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
    }
}