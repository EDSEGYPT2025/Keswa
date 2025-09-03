using Keswa.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class DepartmentCost
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Required(ErrorMessage = "يجب إدخال التكلفة")]
        [Display(Name = "تكلفة القطعة الواحدة")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن تكون التكلفة أكبر من صفر")]
        public decimal CostPerUnit { get; set; }
    }
}
