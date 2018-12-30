using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class ParticipateService
    {
        PMSEntities db = new PMSEntities();
        /// <summary>
        /// Tạo mới một particition
        /// </summary>
        /// <param name="idUser">Id User</param>
        /// <param name="idGroup">Id Group</param>
        /// <param name="part">Model Participate</param>
        /// <param name="isOwner">Có phải owner hông??</param>
        public void createParticipate(string idUser, int idGroup, Participate part, bool isOwner)
        {

            part.IdGroup = idGroup;
            part.IdUser = idUser;
            if (isOwner)
            {
                part.IsOwner = true;
                part.IsAdmin = true;
                part.IsManager = true;
            }
            else
            {
                part.IsOwner = false;
                part.IsAdmin = false;
                part.IsManager = false;
            }
            part.Created_At = DateTime.Now;
            part.Updated_At = DateTime.Now;
            db.Participates.Add(part);
            db.SaveChanges();
        }
        public List<Participate> findMemberInGroup(int IdGroup)
        {
            var ListParticipant = db.Participates.Where(x => x.IdGroup == IdGroup).ToList();
            return ListParticipant;
        }
        /// <summary>
        /// Tìm tất cả member không thuộc group đó
        /// </summary>
        /// <param name="memberInGroup">List các Members thuộc group đó</param>
        /// <returns>Return danh sách các Members không thuộc group đó</returns>
        public List<AspNetUser> findMemberNotInGroup(List<Participate> memberInGroup)
        {
            List<string> userInGroup = new List<string>();
            foreach (var item in memberInGroup)
            {
                userInGroup.Add(item.IdUser);
            }
            //string temp = String.Join(", ", userInGroup); 
            var memberNotInGroup =  db.AspNetUsers.Where(x => !userInGroup.Contains(x.Id)).OrderByDescending(x => x.Id).ToList();
            return memberNotInGroup;
        }
        /// <summary>
        /// Lấy role của member trong một group
        /// </summary>
        /// <param name="idUser">Id Member</param>
        /// <param name="idGroup">Id Group</param>
        /// <returns>Return một object Participate của member thuộc group đó</returns>
        public Participate getRoleOfMember(string idUser, int idGroup)
        {
            var role = db.Participates.Where(x => x.IdUser == idUser && x.IdGroup == idGroup).FirstOrDefault();
            return role;
        }
    }
}