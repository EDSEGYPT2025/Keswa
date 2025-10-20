using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Reports
{
    public class WorkerStatementModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkerStatementModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // خصائص لفلترة البحث
        [BindProperty(SupportsGet = true)]
        public int? WorkerId { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        // خصائص لعرض البيانات
        public SelectList WorkersList { get; set; }
        public Worker Worker { get; set; }
        public List<StatementTransaction> Transactions { get; set; } = new();
        public decimal StartingBalance { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal FinalBalance { get; set; }

        public async Task OnGetAsync()
        {
            WorkersList = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            if (!WorkerId.HasValue)
            {
                return; // إذا لم يتم اختيار عامل، لا تعرض أي بيانات
            }

            Worker = await _context.Workers.FindAsync(WorkerId.Value);

            // 1. جلب كل المستحقات (له)
            var earnings = await _context.WorkerAssignments
                .Where(wa => wa.WorkerId == WorkerId.Value && wa.Status == Enums.AssignmentStatus.Completed)
                .Select(wa => new StatementTransaction
                {
                    Date = wa.AssignedDate,
                    Description = $"إنتاج تشغيلة: {wa.AssignmentNumber}",
                    Credit = wa.Earnings, // له
                    Debit = 0 // عليه
                }).ToListAsync();

            // 2. جلب كل المدفوعات (عليه)
            var payments = await _context.WorkerPayments
                .Where(wp => wp.WorkerId == WorkerId.Value)
                .Select(wp => new StatementTransaction
                {
                    Date = wp.PaymentDate,
                    Description = $"دفعة نقدية",
                    Credit = 0, // له
                    Debit = wp.Amount // عليه
                }).ToListAsync();

            // 3. دمج الحركات وترتيبها زمنيًا
            var allTransactions = earnings.Concat(payments).OrderBy(t => t.Date).ToList();

            // 4. فلترة الحركات حسب التاريخ وحساب الأرصدة
            if (FromDate.HasValue)
            {
                // حساب الرصيد الافتتاحي (كل الحركات قبل تاريخ البداية)
                StartingBalance = allTransactions
                    .Where(t => t.Date.Date < FromDate.Value.Date)
                    .Sum(t => t.Credit - t.Debit);
            }

            // فلترة الحركات لتكون ضمن النطاق الزمني المحدد
            Transactions = allTransactions
                .Where(t => (!FromDate.HasValue || t.Date.Date >= FromDate.Value.Date) &&
                            (!ToDate.HasValue || t.Date.Date <= ToDate.Value.Date))
                .ToList();

            // 5. حساب الرصيد المرحل لكل حركة
            decimal runningBalance = StartingBalance;
            foreach (var transaction in Transactions)
            {
                runningBalance += transaction.Credit - transaction.Debit;
                transaction.Balance = runningBalance;
            }

            // 6. حساب الإجماليات النهائية
            TotalEarnings = Transactions.Sum(t => t.Credit);
            TotalPayments = Transactions.Sum(t => t.Debit);
            FinalBalance = StartingBalance + TotalEarnings - TotalPayments;
        }
    }

    // نموذج مساعد لتوحيد شكل الحركات
    public class StatementTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Credit { get; set; } // له
        public decimal Debit { get; set; }  // عليه
        public decimal Balance { get; set; } // الرصيد
    }
}