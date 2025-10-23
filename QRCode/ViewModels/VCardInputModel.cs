using Microsoft.AspNetCore.Http; // <-- !! إضافة هامة لاستخدام IFormFile
using System.ComponentModel.DataAnnotations;

namespace QRCode.ViewModels
{
    public class VCardInputModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Display(Name = "المسمى الوظيفي")]
        public string? JobTitle { get; set; }

        [Display(Name = "الصورة الشخصية (أقل من 50KB)")]
        public IFormFile? Photo { get; set; } // <-- إضافة الصورة

        [Phone]
        [Display(Name = "رقم الهاتف (الأساسي)")]
        public string? PhoneNumber { get; set; }

        [Phone]
        [Display(Name = "رقم الواتساب")]
        public string? WhatsAppNumber { get; set; }

        [Phone]
        [Display(Name = "رقم هاتف إضافي (عمل/منزل)")]
        public string? OtherPhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Url]
        [Display(Name = "الموقع الإلكتروني (Website)")]
        public string? WebsiteUrl { get; set; }

        [Url]
        [Display(Name = "حساب فيسبوك")]
        public string? FacebookUrl { get; set; }

        [Url]
        [Display(Name = "حساب انستجرام")]
        public string? InstagramUrl { get; set; }

        // --- الإضافات الجديدة ---
        [Url]
        [Display(Name = "حساب لينكدإن (LinkedIn)")]
        public string? LinkedInUrl { get; set; }

        [Url]
        [Display(Name = "حساب جيت هاب (GitHub)")]
        public string? GitHubUrl { get; set; }
        // -----------------------
    }
}