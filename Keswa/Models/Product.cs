using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "كود الموديل")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "اسم الموديل مطلوب")]
        [Display(Name = "اسم الموديل")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [ValidateNever]
        // *** تم التعديل هنا: تغيير النوع من ICollection إلى List ***
        public List<BillOfMaterialItem> BillOfMaterialItems { get; set; } = new List<BillOfMaterialItem>();


        [Display(Name = "سعر خياطة القطعة (فيا)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SewingRate { get; set; } = 0;

    }
}
