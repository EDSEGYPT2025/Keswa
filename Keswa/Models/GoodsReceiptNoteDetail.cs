using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- إضافة جديدة
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class GoodsReceiptNoteDetail
    {
        public int Id { get; set; }

        public int GoodsReceiptNoteId { get; set; }

        [ValidateNever] // <-- تم التعديل هنا
        public GoodsReceiptNote GoodsReceiptNote { get; set; }

        [Required(ErrorMessage = "يجب اختيار المادة")]
        [Display(Name = "المادة الخام")]
        public int MaterialId { get; set; }

        [ValidateNever] // <-- تم التعديل هنا
        public Material Material { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية المستلمة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
        public double Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Display(Name = "سعر الوحدة")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        public decimal UnitPrice { get; set; }
    }
}
