using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Keswa.Data;
using Keswa.Models;
using System.Threading.Tasks; // <-- تأكد من وجود هذا السطر
using System.Linq; // <-- تأكد من وجود هذا السطر
using System.Collections.Generic; // <-- تأكد من وجود هذا السطر
using System; // <-- تأكد من وجود هذا السطر

namespace Keswa.Services
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task Initialize()
        {
            // تطبيق أي Migrations معلقة
            try
            {
                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception)
            {
                // يمكنك تسجيل الخطأ هنا
                throw;
            }

            // 1. إنشاء الصلاحيات (Roles) إذا لم تكن موجودة
            string[] roleNames = { "Admin", "Manager", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء مستخدم افتراضي بصلاحية Admin إذا لم يكن موجوداً
            var adminEmail = "admin@admin.com";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true // تأكيد الإيميل مباشرة
                };

                var result = await _userManager.CreateAsync(adminUser, "Oe@123456");
                if (result.Succeeded)
                {
                    // تعيين صلاحية Admin للمستخدم الجديد
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ================== بداية الكود الجديد ==================
            // 3. استدعاء دالة إضافة الألوان الأساسية
            await SeedColorsAsync();
            // ================== نهاية الكود الجديد ===================
        }

        // --- تمت إضافة هذه الدالة بالكامل ---
        private async Task SeedColorsAsync()
        {
            // نتأكد أولاً أن جدول الألوان فارغ
            if (await _context.Colors.AnyAsync())
            {
                return; // إذا كان يحتوي على بيانات، نخرج من الدالة
            }

            // إذا كان الجدول فارغًا، نقوم بإنشاء قائمة بالألوان الأساسية
            var colors = new List<Color>
            {
                new() { Name = "أبيض" },
                new() { Name = "أسود" },
                new() { Name = "أحمر" },
                new() { Name = "أزرق" },
                new() { Name = "أخضر" },
                new() { Name = "أصفر" },
                new() { Name = "بني" },
                new() { Name = "برتقالي" },
                new() { Name = "بنفسجي" },
                new() { Name = "رمادي" }
            };

            // نضيف القائمة إلى قاعدة البيانات ونحفظ التغييرات
            await _context.Colors.AddRangeAsync(colors);
            await _context.SaveChangesAsync();
        }
    }
}