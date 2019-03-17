using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
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
        public void addtask(int idStep, string name, int? role, string description, string inputConfig, string fileConfig)
        {
            TaskProcess task = new TaskProcess();
            task.IdStep = idStep;
            task.Name = name;
            task.IdRole = role;
            task.Description = description;
            task.ValueInputText = inputConfig;
            task.ValueInputFile = fileConfig;
            task.Color = commonService.getRandomColor();
            task.Position = getLastPosition(idStep) + 1;
            task.Created_At = DateTime.Now;
            task.Updated_At = DateTime.Now;
            db.TaskProcesses.Add(task);
            db.SaveChanges();
        }

        public void AddFormTask(int idStep, string name, int? role, string description, string formBuilder)
        {
            TaskProcess task = new TaskProcess();
            task.IdStep = idStep;
            task.Name = name;
            task.IdRole = role;
            task.Description = description;
            task.ValueFormJson = formBuilder;
            task.Color = commonService.getRandomColor();
            task.Position = getLastPosition(idStep) + 1;
            task.Created_At = DateTime.Now;
            task.Updated_At = DateTime.Now;
            db.TaskProcesses.Add(task);
            db.SaveChanges();
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
        public int getLastPosition(int idStep)
        {
            TaskProcess task = db.TaskProcesses.Where(x => x.IdStep == idStep).OrderByDescending(x => x.Position).FirstOrDefault();
            if (task == null) return 0;
            else return task.Position;

        }
        public void changePosition(string position)
        {
            JArray jPos = JArray.Parse(position);
            foreach (JObject taskPos in jPos)
            {
                int tId = taskPos["idTask"].ToObject<int>();
                int tPos = taskPos["position"].ToObject<int>();
                var task = db.TaskProcesses.Find(tId);
                task.Position = tPos;
                task.Updated_At = DateTime.Now;
            }
            db.SaveChanges();
        }

        public void editFromTask(int idTask, string name, int? role, string description, string formBuilder)
        {
            TaskProcess task = findTask(idTask);
            task.Name = name;
            task.Description = description;
            task.ValueFormJson = formBuilder;
            task.IdRole = role;
            task.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
    }
}