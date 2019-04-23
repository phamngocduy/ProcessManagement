using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class StepService
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        FileService fileService = new FileService();
        ///=============================================================================================


        /// <summary>
        /// Tìm tất cả các step cúa 1 process
        /// </summary>
        /// <param name="idProcess">Id Process</param>
        public List<Step> findStepsOfProcess(int idProcess)
        {
            List<Step> step = db.Steps.Where(x => x.IdProcess == idProcess).ToList();
            return step;
        }

        public List<StepRun> findStepsOfRunProcess(int idProcess)
        {
            List<StepRun> runstep = db.StepRuns.Where(x => x.idProcess == idProcess).ToList();
            return runstep;
        }

        public StepRun findsteprun(int idstep)
        {
            StepRun steprun = db.StepRuns.Find(idstep);
            return steprun;
        }
        /// <summary>
        /// Xóa tất cả các step thuộc nhiều procses
        /// </summary>
        /// <param name="process">Process Model</param>
        public void removeSteps(List<Process> processes)
        {
            foreach (var process in processes)
            {
                var steps = findStepsOfProcess(process.Id);
                db.Steps.RemoveRange(steps);
            }
            db.SaveChanges();
        }
        public Step findStep(int id)
        {
            Step step = db.Steps.Find(id);
            return step;
        }
        public Step findStepByKey(int processid, int key)
        {
            Step step = db.Steps.Where(x => x.IdProcess == processid && x.Key == key).FirstOrDefault();
            return step;
        }
        public void removeStep(Step step)
        {
            db.Steps.Remove(step);
            db.SaveChanges();
        }
        public void removeStepRun(StepRun step)
        {
            db.StepRuns.Remove(step);
            db.SaveChanges();
        }
        public List<Step> addStepRun(List<Step> liststep, int idprocessrun)
        {

            Process process = db.Processes.Find(liststep.First().Process.Id);
            Process processrun = db.Processes.Find(idprocessrun); 
            foreach (var step in liststep)
            {
                Step stepRun = new Step();
                stepRun.IdProcess = processrun.Id;
                stepRun.Name = step.Name;
                stepRun.Description = step.Description;
                stepRun.StartStep = step.StartStep;
                stepRun.NextStep1 = step.NextStep1;
                stepRun.NextStep2 = step.NextStep2;
                stepRun.Figure = step.Figure;
                stepRun.Key = step.Key;
                stepRun.Color = step.Color;
                stepRun.IsRun = true;
                stepRun.Created_At = DateTime.Now;
                stepRun.Updated_At = DateTime.Now;
                db.Steps.Add(stepRun);
                db.SaveChanges();

                //copy folder step
                string stepPath = string.Format("Upload/{0}/{1}/{2}", process.IdGroup, process.Id, step.Id);
                string stepRunPath = string.Format("Upload/{0}/{1}/{2}", processrun.IdGroup, processrun.Id, stepRun.Id);
                fileService.copyDirectory(stepPath, stepRunPath);
            }
            return findStepsOfProcess(idprocessrun);
        }

        public StepRun addstartstep(int idprocess)
        {
            List<Step> step = findStepsOfProcess(idprocess);
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            ProcessRun idrun = db.ProcessRuns.Where(x => x.IdProcess == idprocess).FirstOrDefault();
            StepRun steprun = new StepRun();
            foreach (var item in step.Where(x => x.StartStep == true))
            {
                steprun.idProcess = idrun.Id;
                steprun.Name = item.Name;
                steprun.StartStep = item.StartStep;
                steprun.NextStep1 = item.NextStep1;
                steprun.NextStep2 = item.NextStep2;
                steprun.Key = item.Key;
                steprun.Figure = item.Figure;
                steprun.Status = status.Id;
                steprun.CloneFrom = item.Id;
                steprun.Created_at = DateTime.Now;
                steprun.Updated_At = DateTime.Now;
                db.StepRuns.Add(steprun);
                db.SaveChanges();
            }
            return steprun;
        }

        public void changestatustep(int idstep, string IdUser)
        {
            Status status = db.Status.Where(y => y.Name == "Done").FirstOrDefault();
            StepRun steprun = findsteprun(idstep);
            steprun.ApproveBy = IdUser;
            steprun.Approve_At = DateTime.Now;
            steprun.Status = status.Id;
            db.SaveChanges();
        }

        public StepRun addrunnextstep(int idrunprocess, Step step)
        {
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            StepRun steprun = new StepRun();
            //foreach (var item in liststeprun)
            //{
                steprun.idProcess = idrunprocess;
                steprun.Name = step.Name;
                steprun.StartStep = step.StartStep;
                steprun.NextStep1 = step.NextStep1;
                steprun.NextStep2 = step.NextStep2;
                steprun.Key = step.Key;
                steprun.Figure = step.Figure;
                steprun.Status = status.Id;
                steprun.CloneFrom = step.Id;
                steprun.Created_at = DateTime.Now;
                steprun.Updated_At = DateTime.Now;
                db.StepRuns.Add(steprun);
                db.SaveChanges();
            //}
            return steprun;
        }

        public void deletenextsteprun(StepRun runstep, StepRun stepback)
        {
            List<TaskProcessRun> taskrun = db.TaskProcessRuns.Where(x =>x.IdStep == runstep.Id).ToList();
            foreach (var item in taskrun)
            {
                List<Comment> comment = db.Comments.Where(x => x.IdDirection == item.Id).ToList();
                db.Comments.RemoveRange(comment);
            }
            db.TaskProcessRuns.RemoveRange(taskrun);
            db.StepRuns.Remove(runstep);
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            stepback.Status = status.Id;
            db.SaveChanges();
        }

        public StepRun completestepinrunprocess(int idrunprocess, List<Step> liststep)
        {
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            StepRun steprun = new StepRun();
            foreach (var step in liststep)
            {
                steprun.idProcess = idrunprocess;
                steprun.Name = step.Name;
                steprun.StartStep = step.StartStep;
                steprun.NextStep1 = step.NextStep1;
                steprun.NextStep2 = step.NextStep2;
                steprun.Key = step.Key;
                steprun.Figure = step.Figure;
                steprun.Status = status.Id;
                steprun.Created_at = DateTime.Now;
                steprun.Updated_At = DateTime.Now;
                db.StepRuns.Add(steprun);
                db.SaveChanges();
            }
            return steprun;
        }
    }
}