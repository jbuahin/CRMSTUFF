using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoScheduler.Models;
using AutoScheduler.Repository;
using System.Linq;
using Microsoft.Xrm.Client.Services;

namespace UnitTestScheduler
{
    [TestClass]
    public class UnitTestJobs
    {
        [TestMethod]
        public void TestGetEntities()
        {
            var repo = new CRMRepository();
            var result = repo.RunJob(4);
            //var db = new ApplicationDbContext();
            //var jobId = 185;
            //var job = db.Jobs.FirstOrDefault(j => j.Id == jobId);
            //Assert.IsNotNull(job);
            //var conn = repo.GetConnectionById(job.ConnectionId, job.UserId);
            //var service = new OrganizationService(conn);
            //Assert.IsNotNull(service);
            //var allEntities = repo.GetEntitiesFromFetch(job.fetchXml, service, job.UserId);
            //if (allEntities.Count == 0) { }
            //var filteredEntities = repo.GetUnsentEntities(allEntities, service, 30);
            //var count = 0;
            //if (filteredEntities.Count > 0)
            //    count = repo.runWorkflows(filteredEntities, service, job.workflowName);
            //Assert.AreNotEqual(0, count);
        }
    }
}
