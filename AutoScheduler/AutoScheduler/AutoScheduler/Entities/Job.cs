using AutoScheduler.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace AutoScheduler.Entities
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [Display(Name ="Job Name")]
        public string JobName { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ConnectionId { get; set; }

        public bool Enabled { get; set; }

        [AllowHtml]
        [Display(Name = "Fetch Query")]
        public string fetchXml { get; set; }

        [Display(Name = "Workflow Name")]
        public string workflowName { get; set; }


        [Display(Name = "How Often")]
        public string cron { get; set; }

        //Hour of the day to start the job
        [Display(Name = "Hour to start (0-23)")]
        [System.ComponentModel.DefaultValue(0)]
        public int startHour { get; set; }
        //How many days before running again
        [Display(Name = "Days Between")]
        [System.ComponentModel.DefaultValue(30)]
        public int daysBetween { get; set; }

        [ForeignKey("ConnectionId")]
        public CRMConnection Connection { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}