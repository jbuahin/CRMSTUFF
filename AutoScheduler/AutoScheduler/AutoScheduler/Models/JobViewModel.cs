using AutoScheduler.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace AutoScheduler.Models
{
    public class JobViewModel
    {
        public int id { get; set; }

        [DisplayName("Job Name")]
        public string jobName { get; set; }

        [DisplayName("Connection")]
        public int connectionId { get; set; }

        [DisplayName("Enabled?")]
        public bool enabled { get; set;}
        [AllowHtml]
        [DisplayName("View to Query")]
        public string fetchXml { get; set; }
        [DisplayName("Workflow Name")]
        public string workflowName { get; set; }
        //Uses NCronTab expressions: https://code.google.com/archive/p/ncrontab/wikis/CrontabExpression.wiki
        [DisplayName("How Often")]
        public string cron { get; set; }
        //Hour of the day to start the job
        [DisplayName("Hour to start (0-23)")]
        public int startHour { get; set; }
        //How many days before running again
        [DisplayName("Days Between")]
        public int daysBetween { get; set; }

        [DisplayName("Entity")]
        public string objectTypeCode { get; set; }
        public string userId { get; set; }
        public IEnumerable<CRMConnection> connections { get; set; }

    }
}