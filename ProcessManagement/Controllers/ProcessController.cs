using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
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
        ///=============================================================================================
     
        [Authorize]
        public ActionResult CreateProcess(int? id)
        {
            Group gr = db.Groups.Find(id);
            Process pr = new Process();
            return View(pr);
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreateProcess(Group model, Process pro)
        {
            string idUser = User.Identity.GetUserId();
            Group gr = db.Groups.Find(model.Id);
            Process ps = new Process();
            ps.IdGroup = gr.Id;
            ps.IdOwner = idUser;
            ps.Name = pro.Name;
            ps.Description = pro.Description;
            ps.Created_At = DateTime.Now;
            ps.Updated_At = DateTime.Now;
            db.Processes.Add(ps);
            db.SaveChanges();
            SetFlash("Success", "Created Process Successfully");
            return RedirectToAction("DrawProcess", new { id = ps.Id });
        }

        public ActionResult DrawProcess(int? id)
        {
            Process ps = db.Processes.Find(id);
            return View(ps);
        }
        [HttpPost]
        public JsonResult DrawProcess(int processId, string data, string nodeData, string linkData)
        {
            Process ps = db.Processes.Find(processId);
            ps.DataJson = data.ToString();
            //JObject json = JObject.Parse(nodeData);
            JArray nodeArray = JArray.Parse(nodeData);
            JArray linkArray = JArray.Parse(linkData);
            var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
            List<int> a = new List<int>();
            for (int i = 0; i < nodeArray.Count; i++)
            {
                var key = (int)nodeArray[i]["key"];
                var from = linkArray.Where(x => (int)x["from"] == key).FirstOrDefault();
                var nextStep2 = 0;
                if (from != null)
                {
                    var to = (int)from["to"];

                    if (!a.Contains(to))
                    {
                        a.Add(to);
                    }
                    else
                    {
                        nextStep2 = to;
                    }
                }
                Step step = new Step();
                step.IdProcess = processId;
                step.Name = nodeArray[i]["text"].ToString();
                step.Key = key;
                step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
                step.NextStep1 = from == null ? 0 : (int)from["to"];
                step.NextStep2 = nextStep2;
                step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
                step.Created_At = DateTime.Now;
                step.Updated_At = DateTime.Now;
                db.Steps.Add(step);
            }
            db.Entry(ps).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return Json(new { id = ps.IdGroup });
        }
    }
}