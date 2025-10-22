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

        // -- BEGIN MODIFICATION --

        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }

        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Display(Name = "الكمية الهالكة")]
        public int Quantity { get; set; } // تم تغيير الاسم من ScrappedQuantity

        [Display(Name = "سبب الهالك")]
        public string? Reason { get; set; }

        [Display(Name = "تاريخ تسجيل الهالك")]
        public DateTime LogDate { get; set; } = DateTime.Now;

        // -- تم حذف الحقول القديمة --
        // public int WorkerAssignmentId { get; set; }
        // public WorkerAssignment WorkerAssignment { get; set; }

        // -- END MODIFICATION --
    }
}