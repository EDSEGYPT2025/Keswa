using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // <-- أضف هذا السطر

namespace Keswa.Models
{
    public class WorkerAssignment
    {
        public int Id { get; set; }

        [Display(Name = "تشغيلة الخياطة")]
        public int SewingBatchId { get; set; }
        [ForeignKey("SewingBatchId")]
        [ValidateNever]
        public SewingBatch SewingBatch { get; set; }

        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        [ValidateNever]
        public Worker Worker { get; set; }

        [Display(Name = "رقم التشغيلة الداخلية")]
        public string AssignmentNumber { get; set; }

        [Display(Name = "الكمية المسلمة")]
        public int AssignedQuantity { get; set; }

        [Display(Name = "الكمية المستلمة (سليمة)")]
        public int ReceivedQuantity { get; set; } = 0;

        [Display(Name = "إجمالي الهالك")]
        public int TotalScrapped { get; set; } = 0;

        [NotMapped]
        [Display(Name = "الكمية المتبقية")]
        public int RemainingQuantity => AssignedQuantity - (ReceivedQuantity + TotalScrapped);

        [Display(Name = "الحالة")]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.InProgress;

        // -- BEGIN CORRECTION 1: Rename Property --
        [Display(Name = "تاريخ التسليم")]
        public DateTime AssignedDate { get; set; } = DateTime.Now; // تم تغيير الاسم
        // -- END CORRECTION 1 --

        [ValidateNever]
        public ICollection<SewingProductionLog> SewingProductionLogs { get; set; }

        // -- BEGIN CORRECTION 2: Add Calculated Earnings --
        [NotMapped]
        [Display(Name = "إجمالي المستحقات")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Earnings => SewingProductionLogs?.Sum(log => log.TotalPay) ?? 0;
        // -- END CORRECTION 2 --
    }
}