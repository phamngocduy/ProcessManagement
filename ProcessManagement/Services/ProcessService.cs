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
        RoleService roleService = new RoleService();
        TaskService taskService = new TaskService();
        FileService fileService = new FileService();
        ///=============================================================================================

        public Process findProcess(int idProcess)
		{
			Process process = db.Processes.Find(idProcess);
			return process;
		}
        public Process findProcess(int idProcess,bool isRun)
        {
            Process process = db.Processes.FirstOrDefault(x =>x.Id == idProcess && x.IsRun == isRun);
            return process;
        }
        public ProcessRun findProcessRun(int idProcess)
        {
            ProcessRun process = db.ProcessRuns.Find(idProcess);
            return process;
        }
        public ProcessRun findProcessRunByProcessId(int idProcess)
        {
            ProcessRun process = db.ProcessRuns.Where(x => x.IdProcess == idProcess).FirstOrDefault();
            return process;
        }
        /// <summary>
        /// Tìm tất cả các process thuộc một group
        /// </summary>
        /// <param name="idGroup">Id Group</param>
        public List<Process> findListProcess(int idGroup)
		{
            List<Process> processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Created_At).ToList();
			return processes;
		}
        public List<Process> findListProcess(int idGroup, bool isRun)
        {
            List<Process> processes = db.Processes.Where(x => x.IdGroup == idGroup && x.IsRun == isRun).OrderByDescending(x => x.Created_At).ToList();
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
            List<Role> roles = db.Roles.Where(x => x.IdProcess == idProcess).ToList();
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
            List<Process> processes = findListProcess(idGroup);
			stepService.removeSteps(processes);
			db.Processes.RemoveRange(processes);
			db.SaveChanges();
		}
        public void removeProcess(Process process)
        {
            string processPath = string.Format("Upload/{0}/{1}", process.IdGroup, process.Id);
            fileService.removeDirectory(processPath);
            db.Processes.Remove(process);
            db.SaveChanges();
        }
        public void removeProcessRun(ProcessRun processrun)
        {
            string processRunPath = string.Format("Upload/{0}/run/{1}", processrun.Process.IdGroup, processrun.Id);
            fileService.removeDirectory(processRunPath);
            db.ProcessRuns.Remove(processrun);
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
			db.Roles.Add(roles);
			db.SaveChanges();
		}
        public int countProcessOfGroup(int idGroup)
        {
            int count = db.Processes.Where(m => m.IdGroup == idGroup).Count();
            return count;
        }
        public List<Process> getProcesses(int idGroup)
        {
            List<Process> processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Updated_At).ToList();
            return processes;
        }
        public List<Process> searchProcesses(int idGroup, string key = null, int quantity = 5 )
        {
            List<Process> processes;
            
            if (key == null)
                processes = db.Processes.Where(x => x.IdGroup == idGroup).OrderByDescending(x => x.Updated_At).Take(quantity).ToList();
            else    
                processes = db.Processes.Where(x => x.IdGroup == idGroup && x.Name.ToLower().Contains(key)).OrderByDescending(x => x.Updated_At).Take(quantity).ToList();
            return processes;
        }
        /// <summary>
        /// Edit thông tin một process
        /// </summary>
        /// <param name="model">Process Model</param>
        public void EditProcess(int processId, Process model)
		{
			Process ps = findProcess(processId);
			ps.Name = model.Name;
			ps.Description = model.Description;
            ps.Updated_At = DateTime.Now;
			db.SaveChanges();
		}

        public Process createProcessRun(Process process, string name, string des)
        {
            Process processrun = new Process();
            processrun.Name = name.Trim();
            processrun.IdGroup = process.IdGroup;
            processrun.IdOwner = process.IdOwner;
            processrun.Description = des.Trim();
            processrun.DataJson = process.DataJson;
            processrun.Avatar = process.Avatar;
            processrun.IsRun = true;
            processrun.Created_At = DateTime.Now;
            processrun.Updated_At = DateTime.Now;
            db.Processes.Add(processrun);
            db.SaveChanges();

            //copy folder process
            string processPath = string.Format("Upload/{0}/{1}", process.IdGroup, process.Id);
            string processRunPath = string.Format("Upload/{0}/{1}", processrun.IdGroup, processrun.Id);
            fileService.copyDirectory(processPath, processRunPath);

            return processrun;
        }

        public void addrunprocess(Process process)
        {
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            ProcessRun runpro = new ProcessRun();
            runpro.IdProcess = process.Id;
            runpro.Name = process.Name;
            runpro.Status = status.Id;
            runpro.Description = process.Description;
            runpro.Start_At = DateTime.Now;
            runpro.Create_At = DateTime.Now;
            runpro.Update_At = DateTime.Now;
            db.ProcessRuns.Add(runpro);
            db.SaveChanges();
        }

        public void removeProcess(int idprocess)
        {
            Process process = findProcess(idprocess);
            if (process != null)
            {
                List<Role> roles = roleService.findListRoleOfProcess(idprocess);
                List<Step> steps = stepService.findStepsOfProcess(idprocess);
                foreach (Step step in steps)
                {
                    List<TaskProcess> tasks = taskService.findTaskOfStep(step.Id);
                    if (tasks.Any())
                    {
                        taskService.removeTasks(tasks);
                        
                    } 
                }
                if (steps.Any()) stepService.removeSteps(steps);
                if (roles.Any()) roleService.removeRoles(roles);

                
                removeProcess(process);
            }
        }
        public void removeprocessrun(int idprocess)
        {
            ProcessRun processrun = findProcessRun(idprocess);
            if (processrun != null)
            {
                Process process = findProcess(processrun.Id);
                List<Role> listrole = roleService.findListRoleOfProcess(idprocess);
                List<RoleRun> listrolerun = roleService.findlistrolerun(listrole);
                List<StepRun> liststeprun = stepService.findStepsOfRunProcess(processrun.Id);
                foreach (StepRun steprun in liststeprun)
                {
                    List<TaskProcessRun> listtaskrun = taskService.findruntaskofstep(steprun.Id);
                    if (listtaskrun.Any()) taskService.deletelisttaskrun(listtaskrun);

                }
                if (liststeprun.Any()) stepService.removelistStepRun(liststeprun);
                if (listrolerun.Any()) roleService.removelistRoleRun(listrolerun);
                removeProcessRun(processrun);

                removeProcess(process);
            }  
        }
    }
}