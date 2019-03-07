using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;

namespace ProcessManagement.Services
{
    public class TaskService
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        StepService stepService = new StepService();
        CommonService commonService = new CommonService();
        ///=============================================================================================
        public void addtask(int idStep, string name, int ? role, string description, string inputConfig, string fileConfig)
        {
            TaskProcess task = new TaskProcess();
            task.IdStep = idStep;
            task.Name = name;
            task.IdRole = role;
            task.Description = description;
            task.ValueInputText = inputConfig;
            task.ValueInputFile = fileConfig;
            task.Color = commonService.getRandomColor();
            task.Created_At = DateTime.Now;
            task.Updated_At = DateTime.Now;
            db.TaskProcesses.Add(task);
            db.SaveChanges();
        }

        public List<TaskProcess> findtaskofstep(int idStep)
        {
            List<TaskProcess> task = db.TaskProcesses.Where(x => x.IdStep == idStep).ToList();
            return task;
        }

        public TaskProcess findTask(int idTask)
        {
            TaskProcess task = db.TaskProcesses.Find(idTask);
            return task;
        }

        public void editTask(int idTask, string name, int? role, string description, string inputConfig, string fileConfig)
        {
            TaskProcess task = findTask(idTask);
            task.Name = name;
            task.Description = description;
            task.IdRole = role;
            task.ValueInputText = inputConfig;
            task.ValueInputFile = fileConfig;
            task.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        public void deletetask(TaskProcess task)
        {
            db.TaskProcesses.Remove(task);
            db.SaveChanges();
        }
    }
}