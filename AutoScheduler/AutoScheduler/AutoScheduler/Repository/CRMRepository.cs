using AutoScheduler.Entities;
using AutoScheduler.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Utilities;

namespace AutoScheduler.Repository
{
    public class CRMRepository
    {
        public bool AddJob(int jobId)
        {
            var db = new ApplicationDbContext();
            var job = db.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null || string.IsNullOrWhiteSpace(job.cron)) return false;
            Hangfire.RecurringJob.AddOrUpdate(job.UserId + job.JobName + job.Id, () => RunJob(jobId), job.cron);
            return true;
        }
        public bool RemoveJob(int jobId)
        {
            var db = new ApplicationDbContext();
            var job = db.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null || string.IsNullOrWhiteSpace(job.cron)) return false;
            Hangfire.RecurringJob.RemoveIfExists(job.UserId + job.JobName + job.Id);
            return true;
        }
        public bool RunJob(int jobId)
        {
            var db = new ApplicationDbContext();
            var job = db.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null) return false;
            var conn = GetConnectionById(job.ConnectionId, job.UserId);
            var service = new CrmServiceClient(conn);
            if (service == null) return false;
            var allEntities = GetEntitiesFromFetch(job.fetchXml, service, job.UserId);
            if (allEntities.Count == 0) return true;//Nothing to run
            var filteredEntities = GetUnsentEntities(allEntities, service, job.daysBetween);
            var count = 0;
            if (filteredEntities.Count > 0)
                count = runWorkflows(filteredEntities, service, job.workflowName);
            return count > 1;
        }
        public DataCollection<Entity> GetEntitiesFromView(Entity view, int id, string userId)
        {
            var connection = GetConnectionById(id, userId);
            if (connection == null) return null;
            var service = new CrmServiceClient(connection);

            var fetch = view.GetAttributeValue<string>("fetchxml");
            var request = new FetchExpression(fetch);
            var result = service.RetrieveMultiple(request);
            return result.Entities;
        }

        public DataCollection<Entity> GetEntitiesFromFetch(string fetch, int id, string userId)
        {
            var connection = GetConnectionById(id, userId);
            if (connection == null) return null;
            var service = new CrmServiceClient(connection);

            var request = new FetchExpression(fetch);
            var result = service.RetrieveMultiple(request);
            return result.Entities;
        }

        public DataCollection<Entity> GetEntitiesFromFetch(string fetch, CrmServiceClient service, string userId)
        {
            var request = new FetchExpression(fetch);
            var result = service.RetrieveMultiple(request);
            return result.Entities;
        }

        public List<Entity> GetUnsentEntities(DataCollection<Entity> unfilteredEntities, CrmServiceClient service, int daysBetweenSend)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity,
                LogicalName = unfilteredEntities[0].LogicalName
            };

            var response = (RetrieveEntityResponse)service.Execute(request);
            var idname = response.EntityMetadata.PrimaryIdAttribute;
            var returnList = new List<Entity>();
            foreach (var entity in unfilteredEntities)
            {
                var query = new QueryExpression("pw_autoschedulertracker");
                query.ColumnSet = new ColumnSet("pw_lastprocesseddate");
                query.AddOrder("pw_lastprocesseddate", OrderType.Descending);
                query.AddLink(entity.LogicalName, "regardingobjectid", idname);
                var cond = new ConditionExpression(idname, ConditionOperator.Equal, entity.Id);
                query.LinkEntities[0].LinkCriteria.AddCondition(cond);
                var result = service.RetrieveMultiple(query);
                var first = result.Entities.FirstOrDefault();
                if (first == null)
                    returnList.Add(entity);
                else if (first.Contains("pw_lastprocesseddate") && first["pw_lastprocesseddate"] != null)
                {
                    var lastSentDate = (DateTime)first["pw_lastprocesseddate"];
                    if (lastSentDate.AddDays(daysBetweenSend) < DateTime.UtcNow)
                        returnList.Add(entity);
                }
            }
            return returnList;
        }

        public int runWorkflows(List<Entity> entities, CrmServiceClient service, string workflowName)
        {
            var count = 0;

            Guid workflowId;
            if (!Guid.TryParse(workflowName, out workflowId))
                workflowId = GetWorkflowId(workflowName, entities[0].LogicalName, service);
            if (workflowId == Guid.Empty) return count;
            foreach (var entity in entities)
            {
                ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
                {
                    WorkflowId = workflowId,
                    EntityId = entity.Id,

                };

                // Execute the workflow.
                ExecuteWorkflowResponse response =
                    (ExecuteWorkflowResponse)service.Execute(request);
                var newTracker = new Entity("pw_autoschedulertracker");
                //TODO: Change from lookup to string Guid in case these cannot be joined if they don't support the activity link
                newTracker["subject"] = entity.ToEntityReference().Name + DateTime.UtcNow.ToString();
                newTracker["regardingobjectid"] = entity.ToEntityReference();
                newTracker["pw_lastprocesseddate"] = DateTime.UtcNow;
                service.Create(newTracker);
                count++;
            }
            return count;
        }

        private Guid GetWorkflowId(string workflowName, string entityName, CrmServiceClient service)
        {
            QueryExpression objQueryExpression = new QueryExpression("workflow");
            objQueryExpression.ColumnSet = new ColumnSet(false);
            objQueryExpression.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, workflowName));
            objQueryExpression.Criteria.AddCondition(new ConditionExpression("parentworkflowid", ConditionOperator.Null));
            objQueryExpression.Criteria.AddCondition(new ConditionExpression("primaryentity", ConditionOperator.Equal, entityName));
            EntityCollection entColWorkflows = service.RetrieveMultiple(objQueryExpression);
            if (entColWorkflows != null && entColWorkflows.Entities.Count > 0)
            {
                return entColWorkflows.Entities[0].Id;
            }
            return Guid.Empty;
        }

        public string GetConnectionById(int id, string userId)
        {
            var db = new ApplicationDbContext();
            CRMConnection cRMConnection = db.CrmConnections.Find(id);

            if (cRMConnection == null || cRMConnection.UserId != userId)
            {
                return null;
            }
            var connectionString = EncryptionHelper.Decrypt(cRMConnection.connectionKey);
            try
            {
                return connectionString;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return null;
            }

        }
    }
}