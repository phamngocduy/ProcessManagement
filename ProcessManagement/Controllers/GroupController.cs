using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using System;

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
            var ListGroup = db.Groups.Where(m=>m.IdOwner== IdUser).OrderByDescending(m => m.Updated_At).ToList();
            ViewData["ListGroup"] = ListGroup;
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Group group, HttpPostedFileBase ImageWS)
        {

            string userid = User.Identity.GetUserId();
            if (ImageWS != null)
            {
                string avatar = "";
                if (ImageWS.ContentLength > 0)
                {
                    var filename = Path.GetFileName(ImageWS.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/workspace/"), filename);
                    ImageWS.SaveAs(path);
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
    }
}