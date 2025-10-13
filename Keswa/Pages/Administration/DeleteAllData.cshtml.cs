using Keswa.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.Administration
{
    public class DeleteAllDataModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteAllDataModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [TempData]
        public string Message { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(bool confirmation)
        {
            if (!confirmation)
            {
                ModelState.AddModelError(string.Empty, "يجب عليك تأكيد رغبتك في حذف البيانات.");
                return Page();
            }

            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // قائمة الجداول التي سيتم تجاهلها
                var tablesToIgnore = new[]
                {
            "AspNetRoles", "AspNetUsers", "AspNetUserClaims",
            "AspNetUserLogins", "AspNetUserRoles", "AspNetUserTokens",
            "AspNetRoleClaims", "__EFMigrationsHistory"
        };
                var tablesToIgnoreFormatted = string.Join(",", tablesToIgnore.Select(t => $"'{t}'"));

                // 1. الحصول على قائمة بكل الجداول ما عدا جداول الهوية
                var tableNames = await _context.Database.SqlQueryRaw<string>(
                     $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT IN ({tablesToIgnoreFormatted})")
                    .ToListAsync();

                // 2. تعطيل كافة القيود
                foreach (var tableName in tableNames)
                {
                    await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] NOCHECK CONSTRAINT ALL");
                }

                // 3. حذف البيانات من الجداول
                foreach (var tableName in tableNames)
                {
                    await _context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");
                }

                // 4. إعادة تعيين عدادات الهوية (Reseed Identity)
                foreach (var tableName in tableNames)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('[{tableName}]', RESEED, 0)");
                    }
                    catch (Exception)
                    {
                        // تجاهل الأخطاء في حال كان الجدول لا يحتوي على Identity column
                    }
                }

                // 5. إعادة تفعيل كافة القيود
                foreach (var tableName in tableNames)
                {
                    await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] WITH CHECK CHECK CONSTRAINT ALL");
                }

                await transaction.CommitAsync();
                Message = "تم حذف جميع بيانات التطبيق بنجاح، مع الإبقاء على جداول المستخدمين.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Message = "حدث خطأ أثناء حذف البيانات: " + ex.Message;
            }

            return Page();
        }
    }
}