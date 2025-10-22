﻿using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic; // <-- تأكد من إضافة هذا
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class FinishingAssignment
    {
        public int Id { get; set; }

        [Display(Name = "تشغيلة التشطيب")]
        public int FinishingBatchId { get; set; }
        [ForeignKey("FinishingBatchId")]
        [ValidateNever]
        public FinishingBatch FinishingBatch { get; set; }

        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        [ValidateNever]
        public Worker Worker { get; set; }

        [Display(Name = "نوع التسليم")]
        public AssignmentType AssignmentType { get; set; }

        [Display(Name = "الكمية المسلمة")]
        public int AssignedQuantity { get; set; }

        [Display(Name = "الكمية المتبقية")]
        public int RemainingQuantity { get; set; }

        [Display(Name = "سعر القطعة")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Display(Name = "الحالة")]
        public FinishingAssignmentStatus Status { get; set; } = FinishingAssignmentStatus.InProgress;

        [Display(Name = "تاريخ التسليم")]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;

        // -- BEGIN CORRECTION --
        [ValidateNever]
        public ICollection<FinishingProductionLog> FinishingProductionLogs { get; set; }
        // -- END CORRECTION --
    }
}