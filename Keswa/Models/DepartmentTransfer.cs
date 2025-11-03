using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class DepartmentTransfer
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        [Display(Name = "من قسم")]
        public string FromDepartment { get; set; }

        [Required]
        [Display(Name = "إلى قسم")]
        public string ToDepartment { get; set; }

        [Required]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "تاريخ التحويل")]
        public DateTime TransferDate { get; set; } = DateTime.Now;

        [Display(Name = "المستخدم")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
