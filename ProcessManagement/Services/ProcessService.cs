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

        public Process findProcess(int idProcess)
        {
            Process process = db.Processes.Find(idProcess);
            return process;
        }
        /// <summary>
        /// Tìm tất cả các process thuộc một group
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        public List<Process> findListProcess(int idGroup)
        {
            var processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Created_At).ToList();
            return processes;
        }
        public void createProcess(int idGroup,string idUser, Process process)
        {
            Process ps = new Process();
            ps.IdGroup = idGroup;
            ps.IdOwner = idUser;
            ps.Name = process.Name;
            ps.Description = process.Description;
            ps.Created_At = DateTime.Now;
            ps.Updated_At = DateTime.Now;
            db.Processes.Add(ps);
            db.SaveChanges();
        }
        /// <summary>
        /// Xóa tất cả các process thuộc một group
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        public void removeProcesses(int idGroup)
        {
            var processes = findListProcess(idGroup);
            stepService.removeSteps(processes);
            db.Processes.RemoveRange(processes);
            db.SaveChanges();
        }
    }
}