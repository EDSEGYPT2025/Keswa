using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class WorkerAssignment
    {
        public int Id { get; set; }

        // ================== بداية الإضافة ==================
        [Display(Name = "رقم التشغيلة الداخلية")]
        public string AssignmentNumber { get; set; }
        // ================== نهاية الإضافة ===================

        [Display(Name = "تشغيلة الخياطة")]
        public int SewingBatchId { get; set; }
        [ValidateNever]
        [ForeignKey("SewingBatchId")]
        public SewingBatch SewingBatch { get; set; }

        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }

        [Display(Name = "تاريخ التسليم للعامل")]
        public DateTime AssignedDate { get; set; }

        [Display(Name = "الكمية المسلمة للعامل")]
        public int AssignedQuantity { get; set; }

        [Display(Name = "الكمية المستلمة (سليم)")]
        public int ReceivedQuantity { get; set; } = 0;

        [Display(Name = "الكمية الهالكة")]
        public int ScrappedQuantity { get; set; } = 0;

        [NotMapped]
        public int RemainingQuantity => AssignedQuantity - (ReceivedQuantity + ScrappedQuantity);

        [Display(Name = "الحالة")]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.InProgress;

        [Display(Name = "المبلغ المستحق")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Earnings { get; set; } = 0;


        [Display(Name = "هل تم الدفع؟")]
        public bool IsPaid { get; set; } = false; // القيمة الافتراضية "لم يتم الدفع"

        // لربط هذا المستحق بسجل الدفع الخاص به
        public int? WorkerPaymentId { get; set; }
        [ForeignKey("WorkerPaymentId")]
        public WorkerPayment WorkerPayment { get; set; }
    }
}