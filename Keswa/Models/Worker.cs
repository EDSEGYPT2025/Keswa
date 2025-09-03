using Keswa.Enums;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class Worker
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "كود العامل مطلوب")]
        [Display(Name = "كود العامل")]
        public string WorkerCode { get; set; }

        [Required(ErrorMessage = "اسم العامل مطلوب")]
        [Display(Name = "اسم العامل بالكامل")]
        public string Name { get; set; }

        [Required(ErrorMessage = "يجب تحديد القسم")]
        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Display(Name = "تاريخ التعيين")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; } = DateTime.Today;
    }
}
