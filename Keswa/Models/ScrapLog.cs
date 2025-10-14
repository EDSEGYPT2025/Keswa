using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class ScrapLog
    {
        public int Id { get; set; }

        [Display(Name = "عهدة العامل")]
        public int WorkerAssignmentId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkerAssignmentId")]
        public WorkerAssignment WorkerAssignment { get; set; }

        [Display(Name = "الكمية الهالكة")]
        public int ScrappedQuantity { get; set; }

        [Display(Name = "سبب الهالك")]
        public string? Reason { get; set; }

        [Display(Name = "تاريخ تسجيل الهالك")]
        public DateTime LogDate { get; set; }
    }
}