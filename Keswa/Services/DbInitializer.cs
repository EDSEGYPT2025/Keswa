// --- FILENAME: Services/DbInitializer.cs ---
// التنفيذ الفعلي لخدمة تهيئة قاعدة البيانات (إنشاء الصلاحيات والمستخدم الافتراضي)

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Keswa.Data;
using Keswa.Models;

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
            var adminEmail = "admin@yourapp.com";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true // تأكيد الإيميل مباشرة
                };

                var result = await _userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    // تعيين صلاحية Admin للمستخدم الجديد
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
