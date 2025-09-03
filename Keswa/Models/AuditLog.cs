using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Display(Name = "التاريخ والوقت")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "الشاشة")]
        public string ScreenName { get; set; }

        [Display(Name = "العملية")]
        public string Action { get; set; }

        [Display(Name = "التفاصيل")]
        public string Details { get; set; } // e.g., "Changed cost from X to Y for WO-123"
    }
}