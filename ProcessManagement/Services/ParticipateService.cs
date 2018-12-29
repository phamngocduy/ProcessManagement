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
        public List<Participate> findMemberOfGroup(int IdGroup)
        {
            var ListParticipant = db.Participates.Where(x => x.IdGroup == IdGroup).ToList();
            return ListParticipant;
        }
    }
}