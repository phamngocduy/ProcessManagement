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
            task.idStep = idStep;
            task.Name = name;
            task.idRole = role;
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
            List<TaskProcess> task = db.TaskProcesses.Where(x => x.idStep == idStep).ToList();
            return task;
        }

        public TaskProcess findtask(int id)
        {
            TaskProcess task = db.TaskProcesses.Find(id);
            return task;
        }

        public TaskProcess edittask(int taskid, string valueinputtext, string valueinputfile, string nametask, int roletask, string editor)
        {
            TaskProcess task = findtask(taskid);
            task.Name = nametask;
            task.Description = editor;
            task.idRole = roletask;
            task.ValueInputText = valueinputtext;
            task.ValueInputFile = valueinputfile;
            task.Updated_At = DateTime.Now;
            db.SaveChanges();
            return task;
        }
        public void deletetask(int idtask)
        {
            TaskProcess task = findtask(idtask);
            db.TaskProcesses.Remove(task);
            db.SaveChanges();
        }
    }
}