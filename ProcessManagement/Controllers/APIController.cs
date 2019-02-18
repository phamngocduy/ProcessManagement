using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProcessManagement.Services;
using ProcessManagement.Models;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ProcessManagement.Controllers
{
    public class APIController : Controller
    {
        ///=============================================================================================
        GroupService groupService = new GroupService();
        CommonService commonService = new CommonService();
        ParticipateService participateService = new ParticipateService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        ///=============================================================================================
        // GET: API
        //public PartialViewResult GetGroupList()
        //{
        //    string IdUser = User.Identity.GetUserId();
        //    //lấy ra những group mà user đó tham gia hoặc sở hữu
        //    ViewData["ListGroup"] = groupService.getMyGroup(IdUser);
        //    return PartialView("GroupList");
        //}
        public JsonResult getGroupList()
        {
            string IdUser = User.Identity.GetUserId();
            List<Group> groups = groupService.getMyGroup(IdUser);
            //var groups = JsonConvert.SerializeObject(a);
            //var data = JsonConvert.SerializeObject(groups, Formatting.None,
            //    new JsonSerializerSettings
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //        PreserveReferencesHandling = PreserveReferencesHandling.None
            //    });
            //
            List<object> data = new List<object>();
            foreach (var group in groups)
            {
                JObject defaultAvatar = JObject.Parse(group.AvatarDefault);
                object tempdata = new {
                    id = group.Id,
                    owner = group.AspNetUser.UserName,
                    name = group.Name,
                    des = group.Description??"",
                    avatar = group.Avatar,
                    default_avatar = group.AvatarDefault,
                    slug = group.ownerSlug,
                    create_at = group.Created_At,
                    update_at = group.Updated_At,
                    time_ralitive = commonService.TimeAgo(group.Updated_At)
                };
                data.Add(tempdata);
            }
            var response = new { data = data, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getCurrentLang()
        {
            string lang = Session["lang"] != null ? Session["lang"].ToString() : "en";
            var response = new { data = lang, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}   