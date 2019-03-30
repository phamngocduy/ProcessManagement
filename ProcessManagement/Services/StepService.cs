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
        public List<Step> addliststeprun(List<Step> liststep, int idprocessrun)
        {
            foreach (var item in liststep)
            {
                Step step = new Step();
                step.IdProcess = idprocessrun;
                step.Name = item.Name;
                step.Description = item.Description;
                step.StartStep = item.StartStep;
                step.NextStep1 = item.NextStep1;
                step.NextStep2 = item.NextStep2;
                step.Figure = item.Figure;
                step.Key = item.Key;
                step.Color = item.Color;
                step.IsRun = true;
                step.Created_At = DateTime.Now;
                step.Updated_At = DateTime.Now;
                db.Steps.Add(step);
                db.SaveChanges();
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
                steprun.Created_at = DateTime.Now;
                steprun.Updated_At = DateTime.Now;
                db.StepRuns.Add(steprun);
                db.SaveChanges();
            }
            return steprun;
        }

        public void changestatustep(int idstep)
        {
            Status status = db.Status.Where(y => y.Name == "Done").FirstOrDefault();
            StepRun steprun = findsteprun(idstep);
            steprun.Status = status.Id;
            db.SaveChanges();
        }

        public StepRun addrunnextstep(int idrunprocess, List<Step> liststeprun)
        {
            Status status = db.Status.Where(y => y.Name == "Running").FirstOrDefault();
            StepRun steprun = new StepRun();
            foreach (var item in liststeprun)
            {
                steprun.idProcess = idrunprocess;
                steprun.Name = item.Name;
                steprun.StartStep = item.StartStep;
                steprun.NextStep1 = item.NextStep1;
                steprun.NextStep2 = item.NextStep2;
                steprun.Key = item.Key;
                steprun.Figure = item.Figure;
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