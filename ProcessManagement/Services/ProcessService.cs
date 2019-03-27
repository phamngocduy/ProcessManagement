using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
using ProcessManagement.Filters;
using System.Dynamic;
using System.Web;
using System.IO;
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

        public ProcessRun findRunProcess(int idProcess)
        {
            ProcessRun process = db.ProcessRuns.Find(idProcess);
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
		public Role findRole(int idRole)
		{
			Role role = db.Roles.Find(idRole);
			return role;
		}
		/// <summary>
		///Tìm tất cả các role thuộc process đó
		/// </summary>
		/// <param name="idProcess"></param>
		/// <returns></returns>
		public List<Role> findListRole(int idProcess)
		{
			var roles = db.Roles.Where(x => x.IdProcess == idProcess).ToList();
			return roles;
		}
		public void createProcess(int idGroup, string idUser, Process process)
		{
			process.IdGroup = idGroup;
			process.IdOwner = idUser;
            process.IsRun = false;
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
		public void insertDataJson(Process ps, string data, string imageprocess)
		{
			ps.DataJson = data;
            ps.Avatar = imageprocess;
            db.SaveChanges();
		}
		public void createRole(Role role)
		{
            Role roles = new Role();
            roles.IdProcess = role.IdProcess;
            roles.Name = role.Name;
            roles.Description = role.Description;
            roles.Color = role.Color;
            roles.IsRun = false;
            roles.Create_At = DateTime.Now;
            roles.Update_At = DateTime.Now;
			db.Roles.Add(role);
			db.SaveChanges();
		}
        public int countProcessOfGroup(int idGroup)
        {
            var count = db.Processes.Where(m => m.IdGroup == idGroup).Count();
            return count;
        }
        public List<Process> getProcess(int idGroup)
        {
            List<Process> processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Updated_At).ToList();
            return processes;
        }
		/// <summary>
		/// Edit thông tin một process
		/// </summary>
		/// <param name="model">Process Model</param>
		public void EditProcess(Process model)
		{
			Process ps = findProcess(model.Id);
			ps.Name = model.Name;
			ps.Description = model.Description;
			db.SaveChanges();
		}

        public int createProcessRun(Process process, string des)
        {
            Process procrun = new Process();
            procrun.IdGroup = process.IdGroup;
            procrun.IdOwner = process.IdOwner;
            procrun.Name = process.Name;
            procrun.Description = des;
            procrun.DataJson = process.DataJson;
            procrun.Avatar = process.Avatar;
            procrun.IsRun = true;
            procrun.Created_At = DateTime.Now;
            procrun.Updated_At = DateTime.Now;
            db.Processes.Add(procrun);
            db.SaveChanges();
            return procrun.Id;
        }

        public void addrunprocess(Process process)
        {
            ProcessRun runpro = new ProcessRun();
            runpro.IdProcess = process.Id;
            runpro.Name = process.Name;
            runpro.Description = process.Description;
            runpro.Start_At = DateTime.Now;
            runpro.Create_At = DateTime.Now;
            runpro.Update_At = DateTime.Now;
            db.ProcessRuns.Add(runpro);
            db.SaveChanges();
        }
    }
}