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
            var role = db.Roles.Where(m => m.Id == idRole && m.IdProcess == idProcess).FirstOrDefault();
            return role;
        }
        public List<Role> findListRoleOfProcess(int idProcess)
        {
            List<Role> listrole = db.Roles.Where(x => x.IdProcess == idProcess).ToList();
            return listrole;
        }
        public List<Role> addrolerun(List<Role> listrole, int idprocessrun)
        {
            foreach (var item in listrole)
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
            foreach (var item in listrole)
            {
                List<RoleRun> role = db.RoleRuns.Where(x => x.IdRole == item.Id).ToList();
                foreach (var rolerun in role)
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
        public bool isAssigned(int roleid, string userid)
        {
            var role = db.RoleRuns.FirstOrDefault(x => x.IdRole == roleid && x.IdUser == userid);
            return role != null ? true : false;
        }
    }
}