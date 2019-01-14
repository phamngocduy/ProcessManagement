using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Filters;
namespace ProcessManagement.Controllers
{
    [LanguageFilter]
    public class GroupController : BaseController
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        GroupService groupService = new GroupService();
        CommonService commonService = new CommonService();
        ParticipateService participateService = new ParticipateService();
        ProcessService processService = new ProcessService();
        ///=============================================================================================
        


        // GET: Group
        [Authorize]
        public ActionResult Index()
        {
            string IdUser = User.Identity.GetUserId();
            //lấy ra những group mà user đó tham gia hoặc sở hữu
            ViewData["ListGroup"] = groupService.getMyGroup(IdUser);
            return View();
        }
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Group group, HttpPostedFileBase ImageGr)
        {

            string idUser = User.Identity.GetUserId();
            //save avatar
            commonService.saveAvatarGroup(group,ImageGr, Server.MapPath("~/Content/images/workspace/"));
            //create new group
            groupService.createGroup(idUser, group);
            //create new participate
            Participate part = new Participate();
            participateService.createParticipate(idUser, group.Id, part, true);

            //set flash
            SetFlash(FlashType.Success, "Created Group Successfully");
            return RedirectToAction("index");
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult Show(int idgroup)
        {
            //Tìm group theo id
            Group group = groupService.findGroup(idgroup);
            if (group == null) return HttpNotFound();

            //Tìm tất cả member thuộc group đó
            var ListParticipant = participateService.findMembersInGroup(group.Id);
            ViewData["ListParticipant"] = ListParticipant;
            //Tìm tất cả các process thuộc group đó
            ViewData["ListProcess"] = processService.findListProcess(group.Id);
            return View(group);
        }

        [Authorize]
        [GroupAuthorize]
        //[RoleAuthorize("Admin")]
        public ActionResult Setting(int idgroup)
        {
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(idgroup);
            if (group == null) return HttpNotFound();
            //Tìm tất cả member thuộc group đó
            var ListParticipant = participateService.findMembersInGroup(group.Id);
            ViewData["ListParticipant"] = ListParticipant;
            ViewData["Group"] = group;
            //Tìm tất cả member không thuộc group đó
            ViewData["ListUser"] = participateService.findMembersNotInGroup(ListParticipant);
            ViewData["Roles"] = participateService.getRoleOfMember(idUser, group.Id);
            return View(group);
        }

        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult AddMember(List<string> adduser)
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();

            //Add members
            participateService.addMembers(group, adduser);
            SetTab(TabType.UserSetting);
            SetFlash(FlashType.Success, "Added Members Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
        }

        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult Edit(Group model, HttpPostedFileBase ImageGr)
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();
            
            //edit
            Group groupEdit = groupService.compareBeforeEdit(group, model);
            commonService.saveAvatarGroup(groupEdit, ImageGr, Server.MapPath("~/Content/images/workspace/"));
            groupService.editGroup(groupEdit);
            SetTab(TabType.GeneralSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        [HttpGet]
        public ActionResult RemoveAvatar()
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();
            groupService.removeAvatar(group);
            SetTab(TabType.GeneralSetting);
            SetFlash(FlashType.Success, "Removed Avatar Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult Remove()
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            var groupName = group.Name;
            ////////////////////////////////////////////////////////////////////////////
            //TODO
            //hỏi thầy lúc xóa group có nên xóa process luôn hông 
            //tại process này có thể là con của một process khác năng ở group khác
            ///////////////////////////////////////////////////////////////////////////
            
            //remove group (bao gồm remove participant,process,step)
            groupService.removeGroup(group);
            SetFlash(FlashType.Success, "Removed Group "+ groupName + " Successfully");
            return RedirectToAction("Index");
        }
        
        [Authorize]
        [GroupAuthorize]
        public ActionResult DeleteMember(Participate model)
        {
            Participate user = participateService.findMemberInGroup(model.Id);
            if (user == null) return HttpNotFound();
            var userName = user.AspNetUser.UserName;
            int groupId = (int)Session["groupid"];
            Group group = groupService.findGroup(groupId);
            ////////////////////////////////////////////////////////////////////////////
            //NOTE
            //Không được xóa đi owner,không được xóa thành viên khách group
            ///////////////////////////////////////////////////////////////////////////
            if (user.IsOwner)
                SetFlash(FlashType.Fail, "You cant remove owner");
            else
            {
                participateService.removeUserInGroup(user);
                SetFlash(FlashType.Success, "Removed " + userName + " Successfully");
            }

            SetTab(TabType.UserSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult EditRoleMember(int id)
        {
            
            Participate user = participateService.findMemberInGroup(id);
            if (user == null) return HttpNotFound();

            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();
           


            ////////////////////////////////////////////////////////////////////////////
            //NOTE
            //Tại owner không được sửa quyền sửa role của chính nó
            //TODO
            //Hỏi customer lại về vụ nè
            //Nếu sửa role của chính nó thì sẽ báo lỗi gì??tạm thời return 404
            ///////////////////////////////////////////////////////////////////////////
            if (user.IsOwner)
            {
                SetFlash(FlashType.Fail, "Owner cant change their role");
                SetTab(TabType.UserSetting);
                return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
            }
            return View(user);
        }

        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult EditRoleMember(Participate model)
        {
            var groupId = (int)Session["groupid"];
            Group group = groupService.findGroup(groupId);
            Participate user = participateService.findMemberInGroup(model.Id);
            if (user == null) return HttpNotFound();
            
            //chỉnh sửa role của 1 user

            if (!user.IsOwner)
            {
                participateService.editRoleUser(model);
                SetFlash(FlashType.Success, "Edited Role of "+user.AspNetUser.UserName+" Successfully");
            }
            else
                SetFlash(FlashType.Fail, "Owner cant change their role");

            SetTab(TabType.UserSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, idgroup = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult MemberLeaveGroup(Participate model)
        {
            string idUser = User.Identity.GetUserId();
            Participate user = participateService.findMemberInGroup(model.Id);
            if (user == null) return HttpNotFound();
            var groupName = user.Group.Name;

            //xóa thành viên trong group
            ////////////////////////////////////////////////////////////////////////////
            //NOTE
            //Chỉ xóa được chính bản thân mình ra khỏi group
            ///////////////////////////////////////////////////////////////////////////
            if (user.IdUser == idUser)
            {
                participateService.removeUserInGroup(user);
                SetFlash(FlashType.Success, "Left Group " + groupName + " Successfully");
                return RedirectToAction("Index");
            }
            else
            {
                SetFlash(FlashType.Fail, "Left Group " + groupName + " Failed");
                SetTab(TabType.AdvancedSetting);
                return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = user.Group.groupSlug, idgroup = user.Group.Id });
            }
            
        }
        [Authorize]
        //[GroupAuthorize]
        public ActionResult CreateProcess(int idgroup)
        {
            Group group = groupService.findGroup(idgroup);
            Process pr = new Process();
            ViewData["group"] = group;
            return View(pr);
        }
        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult CreateProcess(Process pro)
        {
            int idgroup = (int)Session["idgroup"];
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(idgroup);
            processService.createProcess(group.Id, idUser, pro);
            SetFlash(FlashType.Success, "Created Process Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "DrawProcess", groupslug = group.groupSlug, idgroup = group.Id, id = pro.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult DrawProcess(int id)
        {
            Process ps = processService.findProcess(id);
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
        public JsonResult DrawProcess(int processId, string data, string nodeData, string linkData)
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
                    foreach(var item in from)
                    {
                        var to = (int)item["to"];
                        if(j == 1)
                        {
                            step.NextStep1 = to;
                        }else if (j == 2)
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
		//[Authorize]
		//[HttpPost]
		//public ActionResult DeleteProcess(int id)
		//{

		//} 
		public ActionResult CreateRole()
		{
			return View();
		}
		public ActionResult EditRole()
		{
			return View();
		}
    }
}