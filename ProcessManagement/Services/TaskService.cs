using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;

namespace ProcessManagement.Services
{
    public class TaskService
    {
        PMSEntities db = new PMSEntities();
        StepService stepService = new StepService();

        public void addtaskprocess(int idStep, TaskProcess task)
        {
            task.idStep = idStep;
            task.Created_At = DateTime.Now;
            task.Updated_At = DateTime.Now;
            db.TaskProcesses.Add(task);
            db.SaveChanges();
        }

        public List<TaskProcess> findtaskofstep(int idStep)
        {
            List<TaskProcess> task = db.TaskProcesses.Where(x => x.idStep == idStep).ToList();
            return task;
        }

        public TaskProcess findtask(int id)
        {
            TaskProcess task = db.TaskProcesses.Find(id);
            return task;
        }

        public TaskProcess edittask(TaskProcess model)
        {
            TaskProcess task = findtask(model.id);
            task.Name = model.Name;
            task.Description = model.Description;
            task.idRole = model.idRole;
            task.Updated_At = DateTime.Now;
            db.SaveChanges();
            return task;
        }
    }
}