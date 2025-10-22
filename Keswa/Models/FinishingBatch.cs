using Keswa.Enums;
using Keswa.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic; // <-- تأكد من إضافة هذا السطر
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class FinishingBatch
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم تشغيلة التشطيب")]
        public string FinishingBatchNumber { get; set; }

        [Display(Name = "تشغيلة الخياطة الأصلية")]
        public int SewingBatchId { get; set; }
        [ValidateNever]
        [ForeignKey("SewingBatchId")]
        public SewingBatch SewingBatch { get; set; }

        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "الكمية المستلمة")]
        public int Quantity { get; set; }

        [Display(Name = "الحالة")]
        public FinishingBatchStatus Status { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTimeHelper.EgyptNow;

        // -- BEGIN CORRECTION --
        [ValidateNever]
        public ICollection<FinishingAssignment> FinishingAssignments { get; set; }
        // -- END CORRECTION --
    }
}