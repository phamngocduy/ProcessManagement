using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class GroupService
    {
        public readonly PMSEntities db = new PMSEntities();



        /// <summary>
        /// Tạo group mới
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        /// <return>Return một object group</return>
        public Group findGroup(int idGroup)
        {
            Group group = db.Groups.Find(idGroup);
            return group;
        }
        /// <summary>
        /// Tạo một group mới
        /// </summary>
        /// <param name="idUser">Id User</param>
        /// <param name="group">Model group</param>
        public void createGroup(string idUser, Group group)
        {
            group.IdOwner = idUser;
            group.Created_At = DateTime.Now;
            group.Updated_At = DateTime.Now;

            db.Groups.Add(group);
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