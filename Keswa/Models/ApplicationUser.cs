// --- FILENAME: Models/ApplicationUser.cs ---
// هذا هو الكلاس المخصص للمستخدمين. يرث كل خصائص IdentityUser ويضيف خصائص جديدة.

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "الاسم بالكامل")]
        public string FullName { get; set; }
    }
}
