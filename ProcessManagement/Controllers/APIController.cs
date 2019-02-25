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
using System.Dynamic;

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
        UserService userService = new UserService();
        ///=============================================================================================
        // GET: API
        //public PartialViewResult GetGroupList()
        //{
        //    string IdUser = User.Identity.GetUserId();
        //    //lấy ra những group mà user đó tham gia hoặc sở hữu
        //    ViewData["ListGroup"] = groupService.getMyGroup(IdUser);
        //    return PartialView("GroupList");
        //}
        //public JsonResult getGroupList()
        //{
        //    string IdUser = User.Identity.GetUserId();
        //    List<Group> groups = groupService.getMyGroup(IdUser);
        //    //var groups = JsonConvert.SerializeObject(a);
        //    //var data = JsonConvert.SerializeObject(groups, Formatting.None,
        //    //    new JsonSerializerSettings
        //    //    {
        //    //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //    //        PreserveReferencesHandling = PreserveReferencesHandling.None
        //    //    });
        //    //
        //    List<object> data = new List<object>();
        //    foreach (var group in groups)
        //    {
        //        JObject defaultAvatar = JObject.Parse(group.AvatarDefault);
        //        object tempdata = new
        //        {
        //            id = group.Id,
        //            owner = group.AspNetUser.UserName,
        //            name = group.Name,
        //            des = group.Description ?? "",
        //            avatar = group.Avatar,
        //            default_avatar = group.AvatarDefault,
        //            slug = group.ownerSlug,
        //            create_at = group.Created_At,
        //            update_at = group.Updated_At,
        //            time_ralitive = commonService.TimeAgo(group.Updated_At)
        //        };
        //        data.Add(tempdata);
        //    }
        //    var response = new { data = data, status = HttpStatusCode.OK };
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult getGroupList()
        {
            string IdUser = User.Identity.GetUserId();
            List<Group> groups = groupService.getMyGroup(IdUser);

            List<object> groupList = new List<object>();
            foreach (var group in groups)
            {
                object jOwner = new
                {
                    name = group.AspNetUser.UserName,
                    avatar = new 
                    {
                        avatar = group.AspNetUser.Avatar,
                        avatardefault = group.AspNetUser.AvatarDefault
                    },
                };
                


                List<Participate> ListParticipate = participateService.findMembersNotOwnerInGroup(group.Id, 4);
                int memberTotalInGroup = participateService.countMemberInGroup(group.Id);
                List<object> memberList = new List<object>();

                foreach (var member in ListParticipate)
                {
                    if (member.IdUser != group.IdOwner)
                    {
                        object jMember = new
                        {
                            name = member.AspNetUser.UserName,
                            avatar = new
                            {
                                avatar = member.AspNetUser.Avatar,
                                avatardefault = member.AspNetUser.AvatarDefault
                            },
                        };
                        memberList.Add(jMember);
                    }
                }

                object jMembers = new
                {
                    list = memberList,
                    total = memberTotalInGroup
                };

                object tempData = new
                {
                    id = group.Id,
                    owner = jOwner,
                    name = group.Name,
                    des = group.Description ?? "",
                    slug = group.ownerSlug,
                    create_at = new
                    {
                        display = group.Created_At,
                        sort = group.Created_At.ToFileTimeUtc()
                    },
                    update_at = new
                    {
                        display = group.Updated_At,
                        sort = group.Updated_At.ToFileTime()
                    },
                    time_ralitive = commonService.TimeAgo(group.Updated_At),
                    members = jMembers
                };
                groupList.Add(tempData);

            }

            var response = new { data = groupList, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}   