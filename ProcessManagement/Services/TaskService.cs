using Newtonsoft.Json.Linq;
using ProcessManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessManagement.Services
{
    public class TaskService
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        StepService stepService = new StepService();
        CommonService commonService = new CommonService();
        FileService fileService = new FileService();
        ///=============================================================================================
        public List<TaskProcess> findTaskOfStep(int idStep)
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
        public TaskProcess addtask(int idStep, string name, int? role, string description, string inputConfig, string fileConfig)
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
            return task;
        }

        public TaskProcess AddFormTask(int idStep, string name, int? role, string description, string formBuilder)
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
        public void deletelisttask(List<TaskProcess> listtask)
        {
            db.TaskProcesses.RemoveRange(listtask);
            db.SaveChanges();
        }
        public void deletelisttaskrun(List<TaskProcessRun> listtaskrun)
        {
            db.TaskProcessRuns.RemoveRange(listtaskrun);
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
                TaskProcess task = db.TaskProcesses.Find(tId);
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

        public void addListTaskRun(List<TaskProcess> listtask, List<Role> rolerun, List<Step> liststep)
        {

            if (listtask != null)
            {
                TaskProcess taskrun = new TaskProcess();
                foreach (TaskProcess task in listtask)
                {
                    if (task.IdRole != null)
                    {
                        foreach (Role role in rolerun)
                        {
                            if (task.Role.Name == role.Name)
                            {
                                taskrun.IdRole = role.Id;
                                break;
                            }
                        }
                        
                    }
                    else
                    {
                        taskrun.IdRole = null;
                    }
                    Process processrun = new Process();
                    foreach (Step step in liststep)
                    {
                        if (task.Step.Key == step.Key)
                        {
                            taskrun.IdStep = step.Id;
                            processrun = step.Process;
                            break;
                        }
                    }
                    taskrun.Name = task.Name;
                    taskrun.Description = task.Description;
                    taskrun.ValueInputText = task.ValueInputText;
                    taskrun.ValueInputFile = task.ValueInputFile;
                    taskrun.ValueFormJson = task.ValueFormJson;
                    taskrun.Color = task.Color;
                    taskrun.Position = task.Position;
                    taskrun.IsRun = true;
                    taskrun.Created_At = DateTime.Now;
                    taskrun.Updated_At = DateTime.Now;
                    db.TaskProcesses.Add(taskrun);
                    db.SaveChanges();

                    
                    //copy folder task
                    string taskPath = string.Format("Upload/{0}/{1}/{2}/{3}", task.Step.Process.IdGroup, task.Step.IdProcess, task.IdStep, task.Id);
                    string taskRunPath = string.Format("Upload/{0}/{1}/{2}/{3}", processrun.IdGroup, processrun.Id, taskrun.IdStep, taskrun.Id);
                    fileService.copyDirectory(taskPath, taskRunPath);
                }
            }
        }

        public void addlistruntask(List<TaskProcess> listtask, StepRun runstep)
        {
            Status status = db.Status.Where(y => y.Name == "Open").FirstOrDefault();
            foreach (TaskProcess task in listtask)
            {
                TaskProcessRun runtask = new TaskProcessRun();
                runtask.IdStep = runstep.Id;
                runtask.IdRole = task.IdRole;
                runtask.Name = task.Name;
                runtask.Description = task.Description;
                runtask.Status = status.Id;
                runtask.ValueInputText = task.ValueInputText;
                runtask.ValueInputFile = task.ValueInputFile;
                runtask.ValueFormJson = task.ValueFormJson;
                runtask.Color = task.Color;
                runtask.Position = task.Position;
                runtask.CloneForm = task.Id;
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
        public void submitclosetask(int idtaskrun, string iduserby)
        {
            Status status = db.Status.Where(y => y.Name == "Finish").FirstOrDefault();
            TaskProcessRun taskrun = findTaskRun(idtaskrun);
            taskrun.Status = status.Id;
            taskrun.ApproveBy = iduserby;
            taskrun.Approve_At = DateTime.Now;
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
        
        public void submitTask(string userid,string valuetext, string valuefile, int idtaskrun,bool isDone = false)
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

            //TODO: Gộp 2 cột file với text lại thành một
            if (inputConfig["value"] == null)
            {
                inputConfig.Add("value", valuetext);
            }
            else
            {
                inputConfig["value"] = valuetext;
            }

            if (fileConfig["value"] == null)
            {
                fileConfig.Add("value", valuefile);
            }
            else
            {
                fileConfig["value"] = valuefile;
            }
            
            taskrun.ValueInputText = inputConfig.ToString(Newtonsoft.Json.Formatting.None);
            taskrun.ValueInputFile = fileConfig.ToString(Newtonsoft.Json.Formatting.None);
            if (isDone)
            {
                Status status = db.Status.Where(y => y.Name == "Done").FirstOrDefault();
                taskrun.Status = status.Id;
                taskrun.DoneBy = userid;
                taskrun.Done_At = DateTime.Now;
            }
            taskrun.Updated_At = DateTime.Now;
            db.SaveChanges();
        }

        public void submitTaskForm(string iduserby, int idtaskrun, string formrender, bool isDone = false)
        {
            TaskProcessRun taskform = findTaskRun(idtaskrun);
            taskform.ValueFormJson = formrender;
            if (isDone)
            {
                Status status = db.Status.Where(y => y.Name == "Done").FirstOrDefault();
                taskform.Status = status.Id;
                taskform.DoneBy = iduserby;
                taskform.Done_At = DateTime.Now;
            }
            taskform.Updated_At = DateTime.Now;
            db.SaveChanges();
        }
    }
}