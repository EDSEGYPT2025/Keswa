using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class WorkerPayment
    {
        public int Id { get; set; }

        [Required]
        public int WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }

        [Display(Name = "تاريخ الدفعة")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "المبلغ المدفوع")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        // علاقة لتتبع التشغيلات التي تم دفعها في هذه العملية
        public virtual ICollection<WorkerAssignment> PaidAssignments { get; set; }
    }
}