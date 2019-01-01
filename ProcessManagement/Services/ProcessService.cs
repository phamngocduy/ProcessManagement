using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class ProcessService
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        StepService stepService = new StepService();
        ///=============================================================================================
       

        public List<Process> getProcess(int idGroup)
        {
            var processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Created_At).ToList();
            return processes;
        }
        /// <summary>
        /// Xóa tất cả các process thuộc một group
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        public void removeProcesses(int idGroup)
        {
            var processes = getProcess(idGroup);
            stepService.removeSteps(processes);
            db.Processes.RemoveRange(processes);
            db.SaveChanges();
        }
    }
}