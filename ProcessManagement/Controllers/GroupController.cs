using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using System;
using System.Collections.Generic;

namespace ProcessManagement.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        PMSEntities db = new PMSEntities();
        // GET: Group
        public ActionResult Index()
        {
            string IdUser = User.Identity.GetUserId();
            //lấy ra những group mà user tham da
            var ListGroupAttend = db.Participates.Where(m => m.IdUser == IdUser).ToList();
            //tạo 1 list chứa id các group mà user tham gia
            List<int> ListGroupid = new List<int>();
            foreach (var item in ListGroupAttend)
            {
                ListGroupid.Add(item.IdGroup);
            }
            var ListGroup = db.Groups.Where(m=>ListGroupid.Contains(m.Id)).OrderByDescending(m => m.Updated_At).ToList();
            ViewData["ListGroup"] = ListGroup;
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Group group, HttpPostedFileBase ImageGr)
        {

            string userid = User.Identity.GetUserId();
            if (ImageGr != null)
            {
                string avatar = "";
                if (ImageGr.ContentLength > 0)
                {
                    var filename = Path.GetFileName(ImageGr.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/workspace/"), filename);
                    ImageGr.SaveAs(path);
                    avatar = filename;
                }
                group.Avatar = avatar;
            }
            else
            {
                group.Avatar = "default.png";
            }
            group.IdOwner = userid;
            group.Created_At = DateTime.Now;
            group.Updated_At = DateTime.Now;


            db.Groups.Add(group);
            db.SaveChanges();

            Participate role = new Participate();
            role.IdGroup = group.Id;
            role.IdUser = userid;
            role.IsAdmin = true;
            role.Created_At = DateTime.Now;
            role.Updated_At = DateTime.Now;
            db.Participates.Add(role);
            db.SaveChanges();
            return RedirectToAction("Index", "Group");
            //return RedirectToAction("InviteMember", "Group", new { id = ws.ID });
        }
        public ActionResult Show(int? id)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }
        public ActionResult AddMember(int? id)
        {
            string userid = User.Identity.GetUserId();
            Group group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();
           
            var ListParticipant = db.Participates.Where(x => x.IdGroup == id).ToList();
            ViewData["ListParticipant"] = ListParticipant;
            List<string> userInGroup = new List<string>();
            foreach (var item in ListParticipant)
            {
                userInGroup.Add(item.IdUser);
            }

            //string temp = String.Join(", ", userInGroup); 
            ViewData["ListUser"] = db.AspNetUsers.Where(x=>!userInGroup.Contains(x.Id)).OrderByDescending(x => x.Id).ToList();
            return View(group);
        }
        [HttpPost]
        public ActionResult AddMember(Group model, List<string> adduser)
        {
            Group group = db.Groups.Find(model.Id);
            if (group == null) return HttpNotFound();
            foreach (var user in adduser)
            {
                Participate role = new Participate();
                role.IdUser = db.AspNetUsers.SingleOrDefault(x => x.Email == user).Id;
                role.IdGroup = group.Id;
                role.Created_At = DateTime.Now;
                role.Updated_At = DateTime.Now;

                db.Participates.Add(role);
            }
            db.SaveChanges();
            TempData["UserSetting"] = "ABC";
            return RedirectToAction("settings", new { id = model.Id });
        }
        public ActionResult Settings(int? id)
        {
            string userid = User.Identity.GetUserId();
            Group group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();
            var ListParticipant = db.Participates.Where(x => x.IdGroup == id).ToList();
            ViewData["ListParticipant"] = ListParticipant;
            List<string> userInGroup = new List<string>();
            foreach (var item in ListParticipant)
            {
                userInGroup.Add(item.IdUser);
            }

            //string temp = String.Join(", ", userInGroup); 
            ViewData["ListUser"] = db.AspNetUsers.Where(x => !userInGroup.Contains(x.Id)).OrderByDescending(x => x.Id).ToList();
            ViewData["ListVisibility"] = db.Visibilities.ToList();
            return View(group);
        }
        [HttpPost]
        public ActionResult Edit(Group model, HttpPostedFileBase ImageGr)
        {
            Group group = db.Groups.Find(model.Id);
            if (group == null) return HttpNotFound();
           
            if (ImageGr != null)
            {
                string avatar = "";
                if (ImageGr.ContentLength > 0)
                {
                    var filename = Path.GetFileName(ImageGr.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/workspace/"), filename);
                    ImageGr.SaveAs(path);
                    avatar = filename;
                }
                group.Avatar = avatar;
            }
            else
            {
                group.Avatar = group.Avatar != "default.png" ? group.Avatar : "default.png";
            }
            group.Description = model.Description;
            group.Name = model.Name;
            group.Updated_At = DateTime.Now;
            db.SaveChanges();
            TempData["GaneralSetting"] = "ABC";
            return RedirectToAction("Settings", new { id = model.Id });
        }
        public ActionResult Remove(int id)
        {
            Group group = db.Groups.Find(id);
            var iduser = db.Participates.Where(x => x.IdGroup == id);
            db.Participates.RemoveRange(iduser);
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult EditVisibility(Group model)
        {
            Group group = db.Groups.Find(model.Id);
            group.Visibility = model.Visibility;
            group.Updated_At = DateTime.Now;
            db.SaveChanges();
            TempData["PermissionSetting"] = "ABC";
            return RedirectToAction("Settings", new { id = model.Id });
        }
        public ActionResult DeleteMember(Participate model)
        {
            Participate user = db.Participates.SingleOrDefault(x=>x.Id == model.Id);
            var groupId = user.IdGroup;
            db.Participates.Remove(user);
            db.SaveChanges();
            TempData["UserSetting"] = "ABC";
            return RedirectToAction("Settings", new { id = groupId });
        }
        public ActionResult EditRoleMember(int? id)
        {
            Participate ws = db.Participates.SingleOrDefault(x => x.Id == id);
            return View(ws);
        }

        [HttpPost]
        public ActionResult EditRoleMember(Participate model)
        {
            Participate user = db.Participates.SingleOrDefault(x => x.Id == model.Id);
            var groupId = user.IdGroup;
            user.IsAdmin = model.IsAdmin;
            user.IsManager = model.IsManager;
            user.Updated_At = DateTime.Now;
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            TempData["UserSetting"] = "ABC";
            return RedirectToAction("Settings", new { id = groupId });
        }
        public ActionResult MemberLeaveGroup(Participate model)
        {
            Participate user = db.Participates.SingleOrDefault(x => x.Id == model.Id);
            db.Participates.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}