using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoScheduler.Entities;
using AutoScheduler.Models;
using Microsoft.AspNet.Identity;
using AutoScheduler.Repository;
namespace AutoScheduler.Controllers
{
    [Authorize]
    [RequireHttps]
    public class JobsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        [Authorize]
        // GET: Jobs
        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(db.Jobs.ToList());
            }
            var userId = User.Identity.GetUserId();
            var jobs = db.Jobs.Where(j => j.UserId == userId);
            return View(jobs.ToList());
        }

        // GET: Jobs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            Job job = db.Jobs.Find(id);
            if (job == null || job.UserId != userId)
            {
                return HttpNotFound();
            }

            var jobVM = new JobViewModel
            {
                id = job.Id,
                userId = job.UserId,
                connectionId = job.ConnectionId,
                cron = job.cron,
                enabled = job.Enabled,
                fetchXml = job.fetchXml,
                jobName = job.JobName,
                workflowName = job.workflowName,
                connections = db.CrmConnections.Where(c => c.UserId == userId)
            };
            return View(jobVM);
        }
        [Authorize]
        // GET: Jobs/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var viewModel = new JobViewModel
            {
                connections = db.CrmConnections.Where(c => c.UserId == userId)
            };
            return View(viewModel);
        }
        [Authorize]
        // POST: Jobs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobViewModel jobVM)
        {
            var userId = User.Identity.GetUserId();
            jobVM.userId = userId;
            ModelState.Remove("UserId");
            var conn = db.CrmConnections.Find(jobVM.connectionId);
            if (conn == null || conn.UserId != userId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (ModelState.IsValid)
            {
                var job = new Job
                {
                    ConnectionId = jobVM.connectionId,
                    cron = jobVM.cron,
                    Enabled = jobVM.enabled,
                    fetchXml = jobVM.fetchXml,
                    JobName = jobVM.jobName,
                    UserId = jobVM.userId,
                    workflowName = jobVM.workflowName,
                    daysBetween = jobVM.daysBetween,
                    startHour = jobVM.startHour
                };
                if (job.Enabled)
                    db.Jobs.Add(job);
                db.SaveChanges();
                var repo = new CRMRepository();
                repo.AddJob(job.Id);
                return RedirectToAction("Index");
            }

            var viewModel = new JobViewModel
            {
                connections = db.CrmConnections.Where(c => c.UserId == userId)
            };
            return View(viewModel);
        }
        [Authorize]
        // GET: Jobs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();

            Job job = db.Jobs.Find(id);
            if (job == null || job.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var jobVM = new JobViewModel
            {
                id = job.Id,
                userId = job.UserId,
                connectionId = job.ConnectionId,
                cron = job.cron,
                enabled = job.Enabled,
                fetchXml = job.fetchXml,
                jobName = job.JobName,
                workflowName = job.workflowName,
                daysBetween = job.daysBetween,
                startHour = job.startHour,
                connections = db.CrmConnections.Where(c => c.UserId == userId)
            };
            return View(jobVM);
        }
        [Authorize]
        // POST: Jobs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,jobName,userId,connectionId,enabled,fetchXml,workflowName,cron")] JobViewModel jobVM)
        {
            var userId = User.Identity.GetUserId();
            Job checkValue = db.Jobs.Find(jobVM.id);
            if (checkValue == null || checkValue.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModelState.Remove("UserId");
            jobVM.userId = userId;
            if (jobVM.fetchXml == null)
                jobVM.fetchXml = checkValue.fetchXml;
            var job = new Job
            {
                Id = jobVM.id,
                UserId = jobVM.userId,
                ConnectionId = jobVM.connectionId,
                cron = jobVM.cron,
                Enabled = jobVM.enabled,
                fetchXml = jobVM.fetchXml,
                JobName = jobVM.jobName,
                workflowName = jobVM.workflowName,
                daysBetween = jobVM.daysBetween,
                startHour = jobVM.startHour
            };
            if (ModelState.IsValid)
            {
                db.Entry(checkValue).State = System.Data.Entity.EntityState.Detached;
                db.Entry(job).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                var repo = new CRMRepository();

                if (job.Enabled)
                    repo.AddJob(job.Id);
                else
                    repo.RemoveJob(job.Id);
                   
                return RedirectToAction("Index");
            }
            jobVM.connections = db.CrmConnections.Where(c => c.UserId == userId);
            return View(jobVM);
        }
        [Authorize]
        // GET: Jobs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            Job job = db.Jobs.Find(id);
            if (job == null || job.UserId != userId)
            {
                return HttpNotFound();
            }
            var jobVM = new JobViewModel
            {
                id = job.Id,
                userId = job.UserId,
                connectionId = job.ConnectionId,
                cron = job.cron,
                enabled = job.Enabled,
                fetchXml = job.fetchXml,
                jobName = job.JobName,
                workflowName = job.workflowName,
                connections = db.CrmConnections.Where(c => c.UserId == userId),
                daysBetween = job.daysBetween,
                startHour = job.startHour
            };
            return View(jobVM);
        }
        [Authorize]
        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Job job = db.Jobs.Find(id);
            var userId = User.Identity.GetUserId();
            if (job.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var repo = new CRMRepository();
            repo.RemoveJob(job.Id);
            db.Jobs.Remove(job);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
