using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
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

        public List<TaskProcessRun> findruntaskofstep(int idStep)
        {
            List<TaskProcessRun> task = db.TaskProcessRuns.Where(x => x.IdStep == idStep).ToList();
            return task;
        }

        public TaskProcess findTask(int idTask)
        {
            TaskProcess task = db.TaskProcesses.Find(idTask);
            return task;
        }

        public TaskProcessRun findTaskRun(int idTask)
        {
            TaskProcessRun task = db.TaskProcessRuns.Find(idTask);
            return task;
        }
        public void addtask(int idStep, string name, int? role, string description, string inputConfig, string fileConfig)
        {
            TaskProcess task = new TaskProcess();
            task.IdStep = idStep;
            task.Name = name;
            task.IdRole = role;
            task.Description = description;
            task.IsRun = false;
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
            task.IsRun = false;
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

        public void addlisttaskrun(List<TaskProcess> listtaskrun, List<Role> rolerun, List<Step> steprun)
        {
            if (listtaskrun != null)
            {
                TaskProcess task = new TaskProcess();
                foreach (var item in listtaskrun)
                {
                    foreach (var role in rolerun)
                    {
                        if (item.IdRole != null)
                        {
                            if (item.Role.Name == role.Name)
                            {
                                task.IdRole = role.Id;
                            }
                        }
                        else
                        {
                            task.IdRole = null;
                        }
                    }
                    foreach (var step in steprun)
                    {
                        if (item.Step.Key == step.Key)
                        {
                            task.IdStep = step.Id;
                        }
                    }
                    task.Name = item.Name;
                    task.Description = item.Description;
                    task.ValueInputText = item.ValueInputText;
                    task.ValueInputFile = item.ValueInputFile;
                    task.ValueFormJson = item.ValueFormJson;
                    task.Color = item.Color;
                    task.Position = item.Position;
                    task.IsRun = true;
                    task.Created_At = DateTime.Now;
                    task.Updated_At = DateTime.Now;
                    db.TaskProcesses.Add(task);
                    db.SaveChanges();
                }
            }
        }

        public void addlistruntask(List<TaskProcess> listtaskrun, StepRun runstep)
        {
            Status status = db.Status.Where(y => y.Name == "Open").FirstOrDefault();
            foreach (var item in listtaskrun)
            {
                TaskProcessRun runtask = new TaskProcessRun();
                runtask.IdStep = runstep.Id;
                runtask.IdRole = item.IdRole;
                runtask.Name = item.Name;
                runtask.Description = item.Description;
                runtask.Status = status.Id;
                runtask.ValueInputText = item.ValueInputText;
                runtask.ValueInputFile = item.ValueInputFile;
                runtask.ValueFormJson = item.ValueFormJson;
                runtask.Color = item.Color;
                runtask.Position = item.Position;
                runtask.Created_At = DateTime.Now;
                runtask.Updated_At = DateTime.Now;
                db.TaskProcessRuns.Add(runtask);
                db.SaveChanges();
            }
        }

        public void submitdonetask(int idtaskrun)
        {
            Status status = db.Status.Where(y => y.Name == "Done").FirstOrDefault();
            TaskProcessRun taskrun = findTaskRun(idtaskrun);
            taskrun.Status = status.Id;
            taskrun.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        public void submitclosetask(int idtaskrun)
        {
            Status status = db.Status.Where(y => y.Name == "Close").FirstOrDefault();
            TaskProcessRun taskrun = findTaskRun(idtaskrun);
            taskrun.Status = status.Id;
            taskrun.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        public void submitopentask(int idtaskrun)
        {
            Status status = db.Status.Where(y => y.Name == "Open").FirstOrDefault();
            TaskProcessRun taskrun = findTaskRun(idtaskrun);
            taskrun.Status = status.Id;
            taskrun.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
        
        public void submitvaluetask(string valuetext, string valuefile, int idtaskrun)
        {
            TaskProcessRun taskrun = findTaskRun(idtaskrun);
            JObject inputConfig = new JObject();
            if (taskrun.ValueInputText != null)
            {
                inputConfig = JObject.Parse(taskrun.ValueInputText);
            }
            JObject fileConfig = new JObject();
            if (taskrun.ValueInputFile != null)
            {
                fileConfig = JObject.Parse(taskrun.ValueInputFile);
            }
            if (inputConfig["value"] == null)
            {
                inputConfig.Add("value", valuetext);
            }

            if (fileConfig["value"] == null)
            {
                fileConfig.Add("value", valuefile);
            }
            
            taskrun.ValueInputText = inputConfig.ToString(Newtonsoft.Json.Formatting.None);
            taskrun.ValueInputFile = fileConfig.ToString(Newtonsoft.Json.Formatting.None);
            db.SaveChanges();
        }

        public void savevaluetaskform(int idtaskrun, string formrender)
        {
            TaskProcessRun taskform = findTaskRun(idtaskrun);
            taskform.ValueFormJson = formrender;
            db.SaveChanges();
        }
    }
}