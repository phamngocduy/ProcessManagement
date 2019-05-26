using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class RoleService
    {
        ///=============================================================================================
		PMSEntities db = new PMSEntities();
        ///=============================================================================================
        public Role findRoleOfProcess(int idRole, int idProcess)
        {
            Role role = db.Roles.Where(m => m.Id == idRole && m.IdProcess == idProcess).FirstOrDefault();
            return role;
        }
        public List<Role> findListRoleOfProcess(int idProcess)
        {
            List<Role> listrole = db.Roles.Where(x => x.IdProcess == idProcess).ToList();
            return listrole;
        }
        public List<Role> addRoleRun(List<Role> listrole, int idprocessrun)
        {
            foreach (Role item in listrole)
            {
                Role role = new Role();
                role.IdProcess = idprocessrun;
                role.Name = item.Name;
                role.Description = item.Description;
                role.IsRun = true;
                role.Color = item.Color;
                role.Create_At = DateTime.Now;
                role.Update_At = DateTime.Now;
                db.Roles.Add(role);
                db.SaveChanges();
            }
            return findListRoleOfProcess(idprocessrun);
        }

        public List<RoleRun> findlistrolerun(List<Role> listrole)
        {
            List<RoleRun> listrolerun = new List<RoleRun>();
            foreach (Role item in listrole)
            {
                List<RoleRun> role = db.RoleRuns.Where(x => x.IdRole == item.Id).ToList();
                foreach (RoleRun rolerun in role)
                {
                    listrolerun.Add(rolerun);
                }
            }
            return listrolerun;
        }

        public void assignrolerun(int roleid, string userid)
        {
            
            RoleRun rolerun = new RoleRun();
            rolerun.IdRole = roleid;
            rolerun.IdUser = userid;
            rolerun.Create_At = DateTime.Now;
            rolerun.Update_At = DateTime.Now;
            db.RoleRuns.Add(rolerun);
            db.SaveChanges();
            
        }
        public void removeRoleRun(int roleid)
        {
            List<RoleRun> role = db.RoleRuns.Where(x => x.IdRole == roleid).ToList();
            if (role.Any())
            {
                db.RoleRuns.RemoveRange(role);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Tìm Role dựa trên id
        /// </summary>
        /// <param name="idRole">Id Role</param>
        /// <return>Return một object role</return>
        public Role findRole(int idRole)
        {
            Role role = db.Roles.Find(idRole);
            return role;
        }
        /// <summary>
		/// Thay đổi role của một process
		/// </summary>
		/// <param name="model">Role model</param>
		public void editRole(Role model)
        {
            Role role = findRole(model.Id);
            role.Name = model.Name;
            role.Description = model.Description;
            role.Update_At = DateTime.Now;
            db.SaveChanges();
        }
        // <summary>
        /// Xóa role ra khỏi process
        /// </summary>
        /// <param name="role">Oarticipate Model</param>
        public void removeRole(Role role)
        {
            db.Roles.Remove(role);
            db.SaveChanges();
        }
        public void removeRoles(List<Role> listrole)
        {
            db.Roles.RemoveRange(listrole);
            db.SaveChanges();
        }
        public void removelistRoleRun(List<RoleRun> listrolerun)
        {
            db.RoleRuns.RemoveRange(listrolerun);
            db.SaveChanges();
        }
        public bool isNameExist(Role role,int processid)
        {
            Role r = db.Roles.FirstOrDefault(x => x.Name.ToLower() == role.Name.ToLower().Trim() && x.IdProcess == processid);
            return r != null ? true : false;
        }
        public bool isAssigned(int roleid, string userid)
        {
            RoleRun role = db.RoleRuns.FirstOrDefault(x => x.IdRole == roleid && x.IdUser == userid);
            return role != null ? true : false;
        }

        public List<RoleRun> findlistrolerunbyidroleprocess(int? idrole)
        {
            List<RoleRun> listrolerun = db.RoleRuns.Where(x => x.IdRole == idrole).ToList();
            return listrolerun;
        }

        public List<RoleRun> findlistrolerunbyiduser(string idUser)
        {
            List<RoleRun> listrolerun = db.RoleRuns.Where(x => x.IdUser == idUser).ToList();
            return listrolerun;
        }

        public Role findrolebyidrolerun(RoleRun rolerun)
        {
            Role role = db.Roles.Where(x => x.Id == rolerun.IdRole).FirstOrDefault();
            return role;
        }
    }
}