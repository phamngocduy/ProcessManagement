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
            process.IdGroup = idGroup;
            process.IdOwner = idUser;
            process.Created_At = DateTime.Now;
            process.Updated_At = DateTime.Now;
            db.Processes.Add(process);
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
        public void insertDataJson(Process ps,string data)
        {
            ps.DataJson = data;
            db.SaveChanges();
        }
    }
}