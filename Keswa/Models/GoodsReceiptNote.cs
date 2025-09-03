using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- إضافة جديدة
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class GoodsReceiptNote
    {
        public GoodsReceiptNote()
        {
            Details = new HashSet<GoodsReceiptNoteDetail>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "نوع السند مطلوب")]
        [Display(Name = "نوع السند")]
        public TransactionType TransactionType { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم السند الورقي")]
        public string? DocumentNumber { get; set; }

        [Required(ErrorMessage = "تاريخ الاستلام مطلوب")]
        [Display(Name = "تاريخ الاستلام")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; }

        [Required(ErrorMessage = "يجب اختيار المخزن")]
        [Display(Name = "المخزن المستلم")]
        public int WarehouseId { get; set; }

        [ValidateNever] // <-- تم التعديل هنا
        public Warehouse Warehouse { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public ICollection<GoodsReceiptNoteDetail> Details { get; set; }
    }
}
