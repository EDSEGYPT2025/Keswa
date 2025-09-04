using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [Display(Name = "اسم العميل")]
        public string Name { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        // *** تمت إضافة هذه الحقول ***
        [NotMapped] // هذا الحقل لن يتم حفظه في قاعدة البيانات مباشرة
        [Display(Name = "الرصيد الافتتاحي")]
        public decimal OpeningBalance { get; set; } = 0;

        [NotMapped]
        [Display(Name = "نوع الرصيد")]
        public BalanceType BalanceType { get; set; }

        [ValidateNever]
        public List<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
    }
}
