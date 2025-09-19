using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic; // <-- إضافة مهمة
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class GoodsReceiptNote
    {
        public int Id { get; set; }

        [Display(Name = "نوع السند")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "يجب تحديد المخزن")]
        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }
        [ValidateNever]
        public Warehouse Warehouse { get; set; }

        [Display(Name = "رقم السند الورقي")]
        public string? DocumentNumber { get; set; }

        [Display(Name = "تاريخ الاستلام")]
        [DataType(DataType.Date)]
        public System.DateTime ReceiptDate { get; set; } = System.DateTime.Today;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [ValidateNever]
        // *** تم التعديل هنا: تغيير النوع من ICollection إلى List ***
        public List<GoodsReceiptNoteDetail> Details { get; set; } = new List<GoodsReceiptNoteDetail>();
    }
}
