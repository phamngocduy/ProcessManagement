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
        StepService stepService = new StepService();
        FileService fileService = new FileService();
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
        public ActionResult NewGroup()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult NewGroup(Group group, HttpPostedFileBase FileUpload)
        {

            string idUser = User.Identity.GetUserId();
            //create new group
            groupService.createGroup(idUser, group);

            //save file 
            fileService.CreateDirectory(group.Id);
            string savePath = Server.MapPath(String.Format("~/App_Data/Files/group/{0}/intro", group.Id));
            fileService.saveFile(FileUpload, savePath);
            
            
            //create new participate
            Participate part = new Participate();
            participateService.createParticipate(idUser, group.Id, part, true);

            //set flash
            SetFlash(FlashType.success, "Created Group Successfully");
            return RedirectToAction("index");
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult FileManager(int groupid)
        {
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();
            return View(group);
        }

        [Authorize]
        [GroupAuthorize]
        [HttpPost]
        public ActionResult FileManager(HttpPostedFileBase[] files)
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            if (group == null) return HttpNotFound();
            var folderName = String.Format("~/App_Data/Files/Groups/{0}-{1}/Intros", group.Name, group.Id);
            foreach (var file in files)
            {
                if (file != null)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var serverSavePath = Path.Combine(Server.MapPath(folderName),fileName);
                    file.SaveAs(serverSavePath);
                }
            }
            return View(group);
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult Show(int groupid)
        {
            //Tìm group theo id
            Group group = groupService.findGroup(groupid);
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
        public ActionResult Setting(int groupid)
        {
            string idUser = User.Identity.GetUserId();
            Group group = groupService.findGroup(groupid);
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
            SetFlash(FlashType.success, "Added Members Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
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
            
            groupService.editGroup(group);
            SetTab(TabType.GeneralSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
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
            SetFlash(FlashType.success, "Removed Avatar Successfully");
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
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
            SetFlash(FlashType.success, "Removed Group " + groupName + " Successfully");
            return RedirectToAction("Index");
        }

        [Authorize]
        [GroupAuthorize]
        public ActionResult DeleteMember(int participateid)
        {
            Participate user = participateService.findMemberInGroup(participateid);
            if (user == null) return HttpNotFound();
            var userName = user.AspNetUser.UserName;
            int groupId = (int)Session["groupid"];
            Group group = groupService.findGroup(groupId);
            ////////////////////////////////////////////////////////////////////////////
            //NOTE
            //Không được xóa đi owner,không được xóa thành viên khác group
            ///////////////////////////////////////////////////////////////////////////
            if (user.IsOwner)
                SetFlash(FlashType.fail, "You cant remove owner");
            else
            {
                participateService.removeUserInGroup(user);
                SetFlash(FlashType.success, "Removed " + userName + " Successfully");
            }

            SetTab(TabType.UserSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult EditRoleMember(int participateid)
        {
            
            Participate user = participateService.findMemberInGroup(participateid);
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
                SetFlash(FlashType.fail, "Owner cant change their role");
                SetTab(TabType.UserSetting);
                return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
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
                SetFlash(FlashType.success, "Edited Role of " + user.AspNetUser.UserName + " Successfully");
            }
            else
                SetFlash(FlashType.fail, "Owner cant change their role");

            SetTab(TabType.UserSetting);
            return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = group.groupSlug, groupid = group.Id });
        }
        [Authorize]
        [GroupAuthorize]
        public ActionResult MemberLeaveGroup(int participateid)
        {
            string idUser = User.Identity.GetUserId();
            Participate user = participateService.findMemberInGroup(participateid);
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
                SetFlash(FlashType.success, "Left Group " + groupName + " Successfully");
                return RedirectToAction("Index");
            }
            else
            {
                SetFlash(FlashType.fail, "Left Group " + groupName + " Failed");
                SetTab(TabType.AdvancedSetting);
                return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = user.Group.groupSlug, groupid = user.Group.Id });
            }

        }
        public ActionResult CreateTask()
        {
            return View();
        }
        //[Authorize]
        ////[GroupAuthorize]
        //public ActionResult CreateProcess(int groupid)
        //{
        //    Group group = groupService.findGroup(groupid);
        //    Process pr = new Process();
        //    ViewData["group"] = group;
        //    return View(pr);
        //}
        //[Authorize]
        //[GroupAuthorize]
        //[HttpPost]
        //public ActionResult CreateProcess(Process pro)
        //{
        //    int idgroup = (int)Session["idgroup"];
        //    string idUser = User.Identity.GetUserId();
        //    Group group = groupService.findGroup(idgroup);
        //    processService.createProcess(group.Id, idUser, pro);
        //    SetFlash(FlashType.success, "Created Process Successfully");
        //    return RedirectToRoute("GroupControlLocalizedDefault", new { action = "DrawProcess", groupslug = group.groupSlug, idgroup = group.Id, id = pro.Id });
        //}
        //[Authorize]
        //[GroupAuthorize]
        //public ActionResult DrawProcess(int id)
        //{
        //    Process ps = processService.findProcess(id);
        //    if (ps == null) return HttpNotFound();
        //    if (ps.DataJson != null)
        //    {
        //        var loadprocess = ps.DataJson.ToString();
        //        JObject load = JObject.Parse(loadprocess);
        //        ViewData["load"] = load;
        //    }
        //    return View(ps);
        //}
        //[Authorize]
        //[HttpPost]
        //public JsonResult DrawProcess(int processId, string data, string nodeData, string linkData)
        //{
        //    Process ps = processService.findProcess(processId);
        //    processService.insertDataJson(ps, data);
        //    //JObject json = JObject.Parse(nodeData);
        //    JArray nodeArray = JArray.Parse(nodeData);
        //    JArray linkArray = JArray.Parse(linkData);
        //    var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
        //    for (int i = 0; i < nodeArray.Count; i++)
        //    {
        //        var key = (int)nodeArray[i]["key"];
        //        var from = linkArray.Where(x => (int)x["from"] == key).ToList();
        //        Step step = new Step();
        //        int j = 1;
        //        if (from != null)
        //        {
        //            foreach (var item in from)
        //            {
        //                var to = (int)item["to"];
        //                if (j == 1)
        //                {
        //                    step.NextStep1 = to;
        //                }
        //                else if (j == 2)
        //                {
        //                    step.NextStep2 = to;
        //                }
        //                j++;
        //            }
        //            if (step.NextStep2 == null)
        //            {
        //                step.NextStep2 = 0;
        //            }
        //            if (step.NextStep1 == null)
        //            {
        //                step.NextStep1 = 0;
        //            }
        //        }

        //        step.IdProcess = processId;
        //        step.Name = nodeArray[i]["text"].ToString();
        //        step.Key = key;
        //        step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;

        //        step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
        //        step.Created_At = DateTime.Now;
        //        step.Updated_At = DateTime.Now;
        //        db.Steps.Add(step);
        //    }
        //    //db.Entry(ps).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();
        //    return Json(new { id = ps.IdGroup });
        //}

        //[Authorize]
        //[GroupAuthorize]
        //public ActionResult EditProcess(int id)
        //{
        //    Process ps = processService.findProcess(id);
        //    if (ps == null) return HttpNotFound();
        //    if (ps.DataJson != null)
        //    {
        //        var loadprocess = ps.DataJson.ToString();
        //        JObject load = JObject.Parse(loadprocess);
        //        ViewData["load"] = load;
        //    }
        //    return View(ps);
        //}

        //[Authorize]
        //[HttpPost]
        //public JsonResult EditProcess(int processId, string data, string nodeData, string linkData)
        //{
        //    Process ps = processService.findProcess(processId);
        //    processService.insertDataJson(ps, data);
        //    JArray nodeArray = JArray.Parse(nodeData);
        //    JArray linkArray = JArray.Parse(linkData);
        //    var idfirstStep = linkArray.Where(x => (int)x["from"] == -1).FirstOrDefault();
        //    var liststep = db.Steps.Where(z => z.IdProcess == processId).ToList();
        //    List<int> keystep = new List<int>();
        //    List<int> keynode = new List<int>();

        //    //chuyển linkstep về mặc định
        //    //add new step nếu step chưa có trong liststep
        //    //so sánh 2 list tìm giá trị giống nhau và khác nhau
        //    //nếu 2 list có các phần tử giống nhau sẽ lưu vào 1 mảng khác để sử giá trị của step đó
        //    //nếu 2 list có các phần tử khác nhau sẽ lưu vào 1 mảng khác để tạo mới step đó
        //    //sau đó kt lại nếu step nào không có nextstep1 and nextstep2,step đó sẽ đc xóa
        //    foreach (var ls in liststep)
        //    {
        //        ls.StartStep = false;
        //        ls.NextStep1 = 0;
        //        ls.NextStep2 = 0;
        //        keystep.Add(ls.Key);
        //    }

        //    for (int i = 0; i < nodeArray.Count; i++)
        //    {
        //        keynode.Add((int)nodeArray[i]["key"]);
        //    }

        //    int[] same = keynode.Intersect(keystep).ToArray();
        //    int[] diff = keynode.Union(keystep).Except(same).ToArray();

        //    List<Step> keystepgiong = new List<Step>();
        //    List<Step> keystepkhac = new List<Step>();
        //    foreach (var item in same)
        //    {
        //        Step s = db.Steps.Where(p => p.Key == item && p.IdProcess == processId).FirstOrDefault();
        //        keystepgiong.Add(s);
        //    }

        //    for (int i = 0; i < nodeArray.Count; i++)
        //    {
        //        var key = (int)nodeArray[i]["key"];
        //        var from = linkArray.Where(x => (int)x["from"] == key).ToList();
        //        Step step = new Step();
        //        int j = 1;
        //        if (from != null)
        //        {
        //            foreach (var item1 in from)
        //            {
        //                var to = (int)item1["to"];
        //                if (j == 1)
        //                {
        //                    step.NextStep1 = to;
        //                }
        //                else if (j == 2)
        //                {
        //                    step.NextStep2 = to;
        //                }
        //                j++;
        //            }
        //            if (step.NextStep2 == null)
        //            {
        //                step.NextStep2 = 0;
        //            }
        //            if (step.NextStep1 == null)
        //            {
        //                step.NextStep1 = 0;
        //            }
        //        }

        //        step.IdProcess = processId;
        //        step.Name = nodeArray[i]["text"].ToString();
        //        step.Key = key;
        //        step.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;

        //        step.Figure = nodeArray[i]["figure"] == null ? "Step" : nodeArray[i]["figure"].ToString();
        //        step.Created_At = DateTime.Now;
        //        step.Updated_At = DateTime.Now;
        //        foreach (var item in diff)
        //        {
        //            if (key == item)
        //            {
        //                keystepkhac.Add(step);
        //            }
        //            else 
        //            {
        //                if (step.Figure == "Step" && step.NextStep1 == 0 && step.NextStep2 ==0)
        //                {
        //                    Step st = db.Steps.Where(p => p.Key == item && p.IdProcess == processId).FirstOrDefault();
        //                    db.Steps.Remove(st);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //    }
        //    foreach (var item3 in keystepkhac)
        //    {
        //        Step step = new Step();
        //        step.IdProcess = processId;
        //        step.Name = item3.Name;
        //        step.Key = item3.Key;
        //        step.StartStep = item3.StartStep;
        //        step.Figure = item3.Figure;
        //        step.Created_At = item3.Created_At;
        //        step.Updated_At = item3.Updated_At;
        //        step.NextStep1 = item3.NextStep1;
        //        step.NextStep2 = item3.NextStep2;
        //        db.Steps.Add(step);
        //    }

        //    foreach (var listst in keystepgiong)
        //    {
        //        for (int i = 0; i < nodeArray.Count; i++)
        //        {
        //            var key = (int)nodeArray[i]["key"];
        //            if (key == listst.Key)
        //            {
        //                var from = linkArray.Where(x => (int)x["from"] == key).ToList();
        //                int j = 1;//if (from != null)
        //                {
        //                    foreach (var item2 in from)
        //                    {
        //                        var to = (int)item2["to"];
        //                        if (j == 1)
        //                        {
        //                            listst.NextStep1 = to;
        //                        }
        //                        else if (j == 2)
        //                        {
        //                            listst.NextStep2 = to;
        //                        }
        //                        j++;
        //                    }
        //                    if (listst.NextStep2 == null)
        //                    {
        //                        listst.NextStep2 = 0;
        //                    }
        //                    if (listst.NextStep1 == null)
        //                    {
        //                        listst.NextStep1 = 0;
        //                    }
        //                }
        //                listst.StartStep = (int)idfirstStep["to"] == (int)nodeArray[i]["key"] ? true : false;
        //                listst.Name = nodeArray[i]["text"].ToString();
        //                listst.Updated_At = DateTime.Now;
        //            }
        //        }
        //    }

        //    db.SaveChanges();
        //    return Json(new { id = ps.IdGroup });
        //}

    }
}