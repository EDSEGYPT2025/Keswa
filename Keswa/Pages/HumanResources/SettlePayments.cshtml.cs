using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.HumanResources
{
    public class SettlePaymentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SettlePaymentsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int WorkerId { get; set; }

        public SelectList WorkersList { get; set; }
        public Worker Worker { get; set; }
        public List<WorkerAssignment> CompletedAssignments { get; set; } // سيعرض كل التشغيلات المكتملة
        public decimal TotalEarnings { get; set; } // إجمالي المستحقات
        public decimal TotalPaid { get; set; } // إجمالي المدفوع
        public decimal BalanceDue { get; set; } // الرصيد المستحق

        [BindProperty, Required, Range(0.01, double.MaxValue)]
        [Display(Name = "المبلغ المدفوع")]
        public decimal PaymentAmount { get; set; }

        [BindProperty]
        [Display(Name = "ملاحظات")]
        public string PaymentNotes { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            WorkersList = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            if (WorkerId > 0)
            {
                Worker = await _context.Workers.FindAsync(WorkerId);

                // جلب كل التشغيلات المكتملة للعامل (للعرض فقط)
                CompletedAssignments = await _context.WorkerAssignments
                    .Where(wa => wa.WorkerId == WorkerId && wa.Status == Enums.AssignmentStatus.Completed)
                    .OrderByDescending(wa => wa.AssignedDate)
                    .ToListAsync();

                // حساب إجمالي المستحقات من كل التشغيلات المكتملة
                TotalEarnings = CompletedAssignments.Sum(a => a.Earnings);

                // حساب إجمالي المبالغ المدفوعة سابقًا
                TotalPaid = await _context.WorkerPayments
                    .Where(wp => wp.WorkerId == WorkerId)
                    .SumAsync(wp => wp.Amount);

                // حساب الرصيد النهائي المستحق للعامل
                BalanceDue = TotalEarnings - TotalPaid;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // إذا كان هناك خطأ في الإدخال، أعد تحميل بيانات الصفحة
                return await OnGetAsync();
            }

            // --- بداية المنطق الجديد والمبسط ---

            // 1. إنشاء سجل دفعة جديد فقط، بدون المساس بسجلات التشغيلات
            var newPayment = new WorkerPayment
            {
                WorkerId = WorkerId,
                Amount = PaymentAmount,
                PaymentDate = DateTime.Now,
                Notes = PaymentNotes
            };

            _context.WorkerPayments.Add(newPayment);
            await _context.SaveChangesAsync();

            // --- نهاية المنطق الجديد ---

            TempData["SuccessMessage"] = $"تم تسجيل دفعة بمبلغ {PaymentAmount.ToString("N2")} ج.م بنجاح.";
            return RedirectToPage(new { WorkerId });
        }
    }
}