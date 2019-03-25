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
        public Role findRoleOfProcess(int idRole,int idProcess)
        {
            var role = db.Roles.Where(m => m.Id == idRole && m.IdProcess == idProcess).FirstOrDefault();
            return role;
        }
        public List<Role> findListRoleOfProcess(int idProcess)
        {
            List<Role> listrole = db.Roles.Where(x => x.IdProcess == idProcess).ToList();
            return listrole;
        }
        public void addrolerun(List<Role> listrole)
        {
            if (listrole != null)
            {
                foreach (var item in listrole)
                {
                    Role role = new Role();
                    role.IdProcess = item.IdProcess;
                    role.Name = item.Name;
                    role.Description = item.Description;
                    role.IsRun = true;
                    role.Color = item.Color;
                    role.IsRun = item.IsRun;
                    role.Create_At = DateTime.Now;
                    role.Update_At = DateTime.Now;
                    db.Roles.Add(role);
                    db.SaveChanges();
                }
            }
        }
    }
}