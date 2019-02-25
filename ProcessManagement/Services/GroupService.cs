using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class GroupService
    {
        ///=============================================================================================
        public readonly PMSEntities db = new PMSEntities();
        ParticipateService participateService = new ParticipateService();
        ProcessService processService = new ProcessService();
        CommonService commonService = new CommonService();
        ///=============================================================================================

        


        /// <summary>
        /// Tìm Group dưa trên id
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        /// <return>Return một object group</return>
        public Group findGroup(int idGroup)
        {
            Group group = db.Groups.Find(idGroup);
            return group;
        }
        /// <summary>
        /// Tìm Group dưa trên Sluug
        /// </summary>
        /// <param name="ownerSlug">Slug Owner</param>
        /// <param name="groupSlug">Slug Group</param>
        /// <returns></returns>
        public Group findGroup(string ownerSlug, string groupSlug)
        {
            Group group = db.Groups.FirstOrDefault(m => m.ownerSlug == ownerSlug && m.groupSlug == groupSlug);
            return group;
        }
        /// <summary>
        /// Tạo một group mới
        /// </summary>
        /// <param name="idUser">Id User</param>
        /// <param name="group">Model group</param>
        public void createGroup(string idUser, Group group)
        {
            AspNetUser user = db.AspNetUsers.Find(idUser);
            group.IdOwner = user.Id;
            group.Created_At = DateTime.Now;
            group.Updated_At = DateTime.Now;
            group.ownerSlug = user.NickName;
            group.groupSlug = commonService.converToSlug(group.Name);
            db.Groups.Add(group);
            db.SaveChanges();
        }
        //public Group compareBeforeEdit(Group oldGroup, Group newGroup)
        //{
        //    newGroup.Id = oldGroup.Id;
        //    if (newGroup.Name.Substring(0, 1).ToUpper() == oldGroup.Name.Substring(0, 1).ToUpper())
        //    {
        //        newGroup.AvatarDefault = oldGroup.AvatarDefault;
        //    }
        //    //if(newGroup.Name.ToLower().Trim() != oldGroup.Name.ToLower().Trim())
        //    //{ 
  
        //    //    newGroup.groupSlug = commonService.converToSlug(newGroup.Name.Trim());
        //    //}
        //    //else
        //    //{
        //    //    newGroup.groupSlug = oldGroup.groupSlug;
        //    //}
        //    if (newGroup.Avatar == null)
        //    {
        //        newGroup.Avatar = oldGroup.Avatar;
        //    }
        //    return newGroup;
        //}
        /// <summary>
        /// Edit thông tin một group
        /// </summary>
        /// <param name="model">Group Model</param>
        public void editGroup(Group model)
        {
            Group group = findGroup(model.Id);
            group.Name = model.Name;
            group.Description = model.Description;
            group.groupSlug = commonService.converToSlug(model.Name);
            group.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        /// <summary>
        /// Xóa một group
        /// </summary>
        /// <param name="group">Model Group</param>
        public void removeGroup(Group group)
        {
            var listUser = participateService.findMembersInGroup(group.Id);
            //remove user in group
            participateService.removeUsersInGroup(listUser);
            //remove tất cả các process của group
            processService.removeProcesses(group.Id);
            db.Groups.Remove(group);
            db.SaveChanges();
        }
        public void removeAvatar(Group model)
        {
            Group group = findGroup(model.Id);
            group.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        /// <summary>
        /// Lấy ra những group mà user tham gia hoặc sỡ hữu
        /// </summary>
        /// <param name="id">Id của User</param>
        /// <return>Return danh sách các group mà user đó tham gia</return>
        public List<Group> getMyGroup(String id)
        {
            //lấy ra những participant mà user tham gia
            var ListGroupAttend = db.Participates.Where(m => m.IdUser == id).ToList();
            //tạo 1 list chứa id các group mà user tham gia
            List<int> ListGroupid = new List<int>();
            foreach (var item in ListGroupAttend)
            {
                ListGroupid.Add(item.IdGroup);
            }
            var ListGroup = db.Groups.Where(m => ListGroupid.Contains(m.Id)).OrderByDescending(m => m.Updated_At).ToList();
            return ListGroup;
        }
    }
}