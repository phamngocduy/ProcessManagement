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
    public class APIController : BaseController
    {
        ///=============================================================================================
        GroupService groupService = new GroupService();
        CommonService commonService = new CommonService();
        ParticipateService participateService = new ParticipateService();
        ProcessService processService = new ProcessService();
        StepService stepService = new StepService();
        UserService userService = new UserService();
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
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
        public JsonResult getUserNotInGroup(string key, int idGroup)
        {
            List<object> jMember = new List<object>();
            List<Participate> ListParticipant = participateService.findMembersInGroup(idGroup);
            List<AspNetUser> memberNotInGroup = participateService.searchMembersNotInGroup(ListParticipant,key,5);
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
            var response = new { message = message , status = status };
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
        public JsonResult editRole(int id,string roles)
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

        [Authorize]
        [HttpPost]
        public JsonResult AddTask(string name, int? idRole, string description, string inputConfig, string fileConfig)
        {
            var status = HttpStatusCode.OK;
            string message;
            object response;
            int idstep = (int)Session["idStep"];
            Step step = stepService.findStep(idstep);


            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Created Task Successfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            if (idRole != null)
            {
                int idR = idRole.GetValueOrDefault();
                var role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }
                
            }

            taskService.addtask(step.Id, name, idRole, description, inputConfig, fileConfig);
            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult editTask(string name, int? idRole, string description, string inputConfig, string fileConfig)
        {
            ///////////////////////////
            /// chỉ được edit task thuộc process mà mình quản lý
            ///////////////////////////
            var status = HttpStatusCode.OK;
            string message;
            object response;
            int idTask = int.Parse(Session["idTask"].ToString());
            var task = taskService.findTask(idTask);
            

            if (task == null)
            {
                message = "Task not exit";
                status = HttpStatusCode.InternalServerError;
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            Step step = stepService.findStep(task.Step.Id);
            //lấy group thuộc process
            int idGroup = task.Step.Process.Group.Id;
            string idUser = User.Identity.GetUserId();

            //check xem có thuộc process trong group
            Participate user = participateService.findMemberInGroup(idUser, idGroup);
            if (user == null)
            {
                message = "Task not found";
                status = HttpStatusCode.Forbidden;
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (name == "")
            {
                status = HttpStatusCode.InternalServerError;
                message = "Created Task Successfully";
                response = new { message = message, status = status };
                return Json(response, JsonRequestBehavior.AllowGet);
            }




            if (idRole != null)
            {
                int idR = idRole.GetValueOrDefault();
                var role = roleService.findRoleOfProcess(idR, step.Process.Id);
                if (role == null)
                {
                    //role not exist
                    status = HttpStatusCode.InternalServerError;
                    message = "Role not exist";
                    response = new { message = message, status = status };
                    return Json(response, JsonRequestBehavior.AllowGet);

                }

            }

            taskService.editTask(task.Id, name, idRole, description, inputConfig, fileConfig);
            SetFlash(FlashType.success, "Created Task Successfully");
            message = "Created Task Successfully";
            response = new { message = message, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);


        }
        public JsonResult changeTaskPosition(string position)
        {
            taskService.changePosition(position);
            var response = new { message = "Change position sucess", status = HttpStatusCode.OK };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}   