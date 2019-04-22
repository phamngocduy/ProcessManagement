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
using System.Dynamic;

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
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult NewGroup(Group group, HttpPostedFileBase FileUpload)
        {

            string idUser = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(group.Name))
            {
                SetFlash(FlashType.error, "Group Name is required");
                return View();
            }
            ConfigRule fileSizeRule = db.ConfigRules.Find("filesize");
            bool isFileOverSize = fileService.checkFileOverSize(FileUpload);
            if (isFileOverSize)
            {
                SetFlash(FlashType.error, string.Format("This file is too big ({0} {1} maximum)",fileSizeRule.Value,fileSizeRule.Unit));
                return View();
            }
            //create new group
            groupService.createGroup(idUser, group);

            //create directory
            string directoryPath = String.Format("Upload/{0}/run", group.Id);
            fileService.createDirectory(directoryPath);
            //save file 
            //string savePath = Server.MapPath(String.Format("~/App_Data/{0}", group.Id));
            string filePath = String.Format("Upload/{0}", group.Id);
            fileService.saveFile(group.Id, FileUpload, filePath, Direction.Group);


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
            string idUser = User.Identity.GetUserId();
            if (group == null) return HttpNotFound();

            dynamic expando = new ExpandoObject();
            var groupStatisticModel = expando as IDictionary<string, object>;
            groupStatisticModel.Add("totalmember", participateService.countMemberInGroup(group.Id));
            groupStatisticModel.Add("totalprocess", processService.countProcessOfGroup(group.Id));

            //tìm file group
            string groupPath = string.Format("Upload/{0}",group.Id);
            List<FileManager> files = fileService.findFiles(group.Id,groupPath);

            ////Tìm tất cả member thuộc group đó
            //var ListParticipant = participateService.findMembersInGroup(group.Id);
            //ViewData["ListParticipant"] = ListParticipant;
            //Tìm tất cả các process thuộc group đó
            ViewData["ListProcess"] = processService.findListProcess(group.Id);
            //thống kê
            ViewData["Statistic"] = groupStatisticModel;
            //lấy role của user hiện tại
            ViewData["UserRoles"] = participateService.getRoleOfMember(idUser, group.Id);
            ViewData["Files"] = files;
            //get maximum file config
            ViewData["FileMaxSize"] = db.ConfigRules.Find("filesize");
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
                SetFlash(FlashType.error, "You cant remove owner");
            else
            {
                participateService.removeUserInGroup(user);
                SetFlash(FlashType.success, "Removed " + userName + " Successfully");
            }

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
                SetFlash(FlashType.error, "Owner cant change their role");
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
                SetFlash(FlashType.error, "Owner cant change their role");

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
                SetFlash(FlashType.error, "Left Group " + groupName + " Failed");
                return RedirectToRoute("GroupControlLocalizedDefault", new { action = "setting", groupslug = user.Group.groupSlug, groupid = user.Group.Id });
            }

        }
        public ActionResult CreateTask()
        {
            return View();
        }
        public ActionResult ProcessManagement()
        {
            return View();
        }
       

    }
}