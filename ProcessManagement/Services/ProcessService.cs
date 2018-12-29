using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class ProcessService
    {
        PMSEntities db = new PMSEntities();

        public List<Process> getProcess(int idGroup)
        {
            var processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Created_At).ToList();
            return processes;
        }
    }
}