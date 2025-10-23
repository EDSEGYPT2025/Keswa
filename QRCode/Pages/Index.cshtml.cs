using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCode.ViewModels; 
using System.Text;
using System.IO;



// --- المكتبات المطلوبة ---
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using QRCoder.ImageSharp; // <-- !! هذا هو السطر الذي يحل الخطأ !!

namespace QRCode.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public VCardInputModel Input { get; set; } = new VCardInputModel();

        public string QrCodeBase64 { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string? photoBase64 = null;

            // --- 1. معالجة الصورة المرفوعة بـ ImageSharp ---
            if (Input.Photo != null && Input.Photo.Length > 0)
            {
                try
                {
                    using (var image = Image.Load(Input.Photo.OpenReadStream()))
                    {
                        // 1. تغيير الحجم والقص لتصبح مربعة 150x150
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Crop,
                            Size = new Size(150, 150) // حجم مناسب جداً لجهة الاتصال
                        }));

                        using (var ms = new MemoryStream())
                        {
                            // 2. ضغط الصورة كـ PNG (يمكن استخدام Jpeg لضغط أعلى)
                            image.Save(ms, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression });

                            var fileBytes = ms.ToArray();

                            // 3. التحقق من الحجم النهائي (بعد الضغط)
                            if (fileBytes.Length > 5 * 1024) // 5KB
                            {
                                ModelState.AddModelError("Input.Photo", "الصورة كبيرة جداً حتى بعد الضغط. حاول رفع صورة أبسط.");
                                return Page();
                            }

                            photoBase64 = Convert.ToBase64String(fileBytes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // إذا لم يكن الملف صورة، سيحدث خطأ هنا
                    ModelState.AddModelError("Input.Photo", "ملف الصورة غير صالح. " + ex.Message);
                    return Page();
                }
            }
            // ---------------------------------

            // 2. بناء نص vCard
            string vCardString = BuildVCardString(Input, photoBase64);

            // 3. توليد الكود
            QrCodeBase64 = GenerateQrCode(vCardString);
            return Page();
        }

        // --- (دالة BuildVCardString ... لا يوجد تغيير فيها) ---
        private string BuildVCardString(VCardInputModel data, string? photoBase64)
        {
            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCARD");
            builder.AppendLine("VERSION:3.0");

            builder.AppendLine($"N:;{data.FullName};;;");
            builder.AppendLine($"FN:{data.FullName}");

            if (!string.IsNullOrEmpty(data.JobTitle))
                builder.AppendLine($"TITLE:{data.JobTitle}");

            if (!string.IsNullOrEmpty(photoBase64))
            {
                builder.AppendLine($"PHOTO;ENCODING=BASE64;TYPE=PNG:{photoBase64}");
            }

            if (!string.IsNullOrEmpty(data.PhoneNumber))
                builder.AppendLine($"TEL;TYPE=CELL;TYPE=VOICE;TYPE=PREF:{data.PhoneNumber}");
            if (!string.IsNullOrEmpty(data.WhatsAppNumber))
                builder.AppendLine($"TEL;TYPE=CELL;TYPE=WHATSAPP:{data.WhatsAppNumber}");
            if (!string.IsNullOrEmpty(data.OtherPhoneNumber))
                builder.AppendLine($"TEL;TYPE=WORK;TYPE=VOICE:{data.OtherPhoneNumber}");
            if (!string.IsNullOrEmpty(data.Email))
                builder.AppendLine($"EMAIL:{data.Email}");
            if (!string.IsNullOrEmpty(data.Address))
                builder.AppendLine($"ADR;TYPE=HOME:;;{data.Address};;;;");
            if (!string.IsNullOrEmpty(data.WebsiteUrl))
                builder.AppendLine($"URL;TYPE=Website:{data.WebsiteUrl}");
            if (!string.IsNullOrEmpty(data.FacebookUrl))
                builder.AppendLine($"URL;TYPE=Facebook:{data.FacebookUrl}");
            if (!string.IsNullOrEmpty(data.InstagramUrl))
                builder.AppendLine($"URL;TYPE=Instagram:{data.InstagramUrl}");
            if (!string.IsNullOrEmpty(data.LinkedInUrl))
                builder.AppendLine($"URL;TYPE=LinkedIn:{data.LinkedInUrl}");
            if (!string.IsNullOrEmpty(data.GitHubUrl))
                builder.AppendLine($"URL;TYPE=GitHub:{data.GitHubUrl}");


            builder.AppendLine("END:VCARD");
            return builder.ToString();
        }

        // --- 4. تحديث دالة توليد الكود ---
        private string GenerateQrCode(string vCardData)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(vCardData, QRCodeGenerator.ECCLevel.Q);

                // --- التغيير هنا: نستخدم المكتبة الجديدة ---
                // هذا السطر سيعمل الآن لأننا أضفنا "using QRCoder.ImageSharp;"
                using (ImageSharpQRCode qrCode = new ImageSharpQRCode(qrCodeData))
                {
                    // نرسم الصورة بـ ImageSharp
                    using (var image = qrCode.GetGraphic(20))
                    {
                        // نحولها إلى Base64
                        return image.ToDataUri(); // هذه الدالة أسهل من التحويل اليدوي
                    }
                }
            }
        }
    }
}