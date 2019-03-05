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
    }
}