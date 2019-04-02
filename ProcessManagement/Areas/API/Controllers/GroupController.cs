using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Controllers;
using ProcessManagement.Filters;

namespace ProcessManagement.Areas.API.Controllers
{
    [AjaxAuthorize]
    public class GroupController : ProcessManagement.Controllers.BaseController
    {
        ///=============================================================================================
        CommonService commonService = new CommonService();
        GroupService groupService = new GroupService();
        ParticipateService participateService = new ParticipateService();
        UserService userService = new UserService();
        FileService fileService = new FileService();
        TaskService taskService = new TaskService();
        ///=============================================================================================

        [GroupAuthorize]
        [HttpPost]
        public JsonResult editGroup(int groupid, string name, string description)
        {
            var status = HttpStatusCode.OK;
            object response;
            string message;
            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Process Name is required";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            Group newGroup = new Group
            {
                Id = groupid,
                Name = name,
                Description = description
            };
            groupService.editGroup(newGroup);
            message = "Edit Group Successfully";
            SetFlash(FlashType.success, "Edit Group Successfully");
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
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
                    slug = group.groupSlug,
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
        public JsonResult getUserNotInGroup(string key, int idGroup)
        {
            List<object> jMember = new List<object>();
            List<Participate> ListParticipant = participateService.findMembersInGroup(idGroup);
            List<AspNetUser> memberNotInGroup = participateService.searchMembersNotInGroup(ListParticipant, key, 5);
            if (memberNotInGroup.Any())
            {
                foreach (var member in memberNotInGroup)
                {
                    object tempData = new
                    {
                        id = member.Id,
                        text = member.NickName
                    };
                    jMember.Add(tempData);
                }
            }
            var response = new { results = jMember, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getMemberList()
        {
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            List<Participate> participates = participateService.findMembersInGroup(group.Id);
            List<object> jMember = new List<object>();
            foreach (var user in participates)
            {
                object tempdata = new
                {
                    id = user.Id,
                    name = user.AspNetUser.UserName,
                    avatar = new
                    {
                        avatar = user.AspNetUser.Avatar,
                        avatardefault = user.AspNetUser.AvatarDefault
                    },
                    isOwner = user.IsOwner,
                    isAdmin = user.IsAdmin,
                    isManager = user.IsManager
                };
                jMember.Add(tempdata);
            }
            var response = new { data = jMember, status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult addMember(string id)
        {
            var status = HttpStatusCode.OK;
            string message;
            int groupid = (int)Session["groupid"];
            Group group = groupService.findGroup(groupid);
            AspNetUser user = userService.findUser(id);
            if (user == null)
            {
                status = HttpStatusCode.InternalServerError;
                message = "User not found";
            }
            else
            {
                var checkExist = participateService.checkMemberExist(user.Id, group.Id);
                if (checkExist != null)
                {
                    status = HttpStatusCode.InternalServerError;
                    message = "User exist in group";
                }
                else
                {
                    participateService.addMember(user.Id, group.Id);
                    message = "Added member sucessfully";
                }
            }
            var response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult editRole(int id, string roles)
        {
            var status = HttpStatusCode.OK;
            string message;
            JObject jRoles = JObject.Parse(roles);
            Participate user = participateService.findMemberInGroup(id);
            if (user == null)
            {
                status = HttpStatusCode.InternalServerError;
                message = "User not in group";
            }
            else
            {
                user.IsAdmin = bool.Parse(jRoles["isAdmin"].ToString());
                user.IsManager = bool.Parse(jRoles["isManager"].ToString());
                participateService.editRoleUser(user);

                status = HttpStatusCode.OK;
                message = "Updated role sucessfully";

            }
            var response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
























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
    }
}