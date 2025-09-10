Skip to content
You said:
ChatGPT said:
شفت الملفات اللي رفعتها (site.css, site.js, و _Layout.cshtml). ✅

بما إن عندنا CSS و JS وملف الـ Layout الرئيسي، أقدر أساعدك دلوقتي نربطهم ونضبط الشكل والسلوك بتاع الـ Sidebar والـ Layout كله.

خليني أوضحلك:

1. ملف الـ CSS (site.css)
معمول فيه تصميم كامل للـ Sidebar (مفتوح/مغلق/موبايل).

معمول Responsive Design للشاشات الصغيرة.

فيه ستايلات إضافية للكروت (KPI cards) والجداول والـ Modal.

فيه متغيرات CSS (Variables) عشان الألوان والمسافات.

2. ملف الـ JS (site.js)
بيفعل Select2 لكل DropDown فيها class searchable-select.

بيفعل Toastr للرسائل المنبثقة.

كود كامل للتحكم في Sidebar (فتح/إغلاق – موبايل وديسكتوب – مع Backdrop).

بيفعل Tooltips على أيقونات الـ Sidebar لو القائمة متقفلة.

بيفعل مودال الحذف (Delete Confirmation Modal).

بيضيف Loading Spinner على زرار الحفظ وقت إرسال الفورم.

3. ملف _Layout.cshtml
ده هو اللي هيجمع الـ Sidebar + Header + Main Content + ربط ملفات CSS وJS.

هنا لازم يكون عندك:

عنصر بـ id="app-container" (اللي بيلف المحتوى كله).

Sidebar (.app-sidebar) ومعاه زرار للتحكم id="sidebar-toggle-btn".

Main content (.app-main).

Renderscripts عشان site.js.

Renderstyles عشان site.css.

⚡ سؤالي ليك عشان أظبطلك الـ _Layout.cshtml:

تحب أجهزهولك كده:

فيه Sidebar يمين مع أيقونات وروابط (Dashboard, Reports, Settings... الخ).

Header بسيط فيه زرار يفتح/يقفل الـ Sidebar.

مساحة فاضية في app-content للرندر بتاع الصفحات (@RenderBody()).

ولا تحب أعملهولك على Bootstrap Template جاهز فيه Navbar + Sidebar + Footer؟


Sources
You said:
@page
@model Keswa.Pages.Customers.IndexModel
@{
    ViewData["Title"] = "إدارة العملاء";
    var egyptianCulture = new System.Globalization.CultureInfo("ar-EG");
}

<!-- Page Header -->
<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h1 class="h2 fw-bold">@ViewData["Title"]</h1>
        <p class="text-muted">عرض وتتبع جميع عملاء المصنع وأرصدتهم الحالية.</p>
    </div>
    <div>
        <a asp-page="Create" class="btn btn-primary"><i class="bi bi-plus-circle me-2"></i>إضافة عميل جديد</a>
    </div>
</div>

<!-- Search Bar -->
<div class="card shadow-sm border-0 mb-4">
    <div class="card-body">
        <form method="get">
            <div class="row g-3 align-items-center">
                <div class="col">
                    <input asp-for="SearchTerm" class="form-control" placeholder="ابحث باسم العميل أو رقم الهاتف..." />
                </div>
                <div class="col-auto">
                    <button type="submit" class="btn btn-secondary"><i class="bi bi-search me-2"></i>بحث</button>
                </div>
                <div class="col-auto">
                    <a asp-page="./Index" class="btn btn-outline-secondary">عرض الكل</a>
                </div>
            </div>
        </form>
    </div>
</div>

<!-- Customers Table -->
<div class="card shadow-sm border-0">
    <div class="card-body">
        <div class="table-responsive">
            <table class="table table-hover align-middle">
                <thead class="table-light">
                    <tr>
                        <th>اسم العميل</th>
                        <th>رقم الهاتف</th>
                        <th>العنوان</th>
                        <th class="text-center">الرصيد الحالي</th>
                        <th>إجراءات</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model.Customers)
                    {
                        <tr>
                            <td><strong>@item.Name</strong></td>
                            <td>@item.PhoneNumber</td>
                            <td>@item.Address</td>
                            <td class="text-center fw-bold">
                                @if (item.Balance > 0)
                                {
                                    <span class="text-danger">@item.Balance.ToString("C", egyptianCulture) (مدين)</span>
                                }
                                else if (item.Balance < 0)
                                {
                                    <span class="text-success">@((item.Balance * -1).ToString("C", egyptianCulture)) (دائن)</span>
                                }
                                else
                                {
                                    <span>@item.Balance.ToString("C", egyptianCulture)</span>
                                }
                            </td>
                            <td>
                                <a asp-page="./Edit" asp-route-id="@item.Id" class="btn btn-sm btn-outline-primary" title="تعديل"><i class="bi bi-pencil-square"></i></a>

                                <!-- زر الحذف الجديد الذي يفتح النافذة المنبثقة -->
                                <button type="button" class="btn btn-sm btn-outline-danger"
                                        data-bs-toggle="modal"
                                        data-bs-target="#deleteConfirmationModal"
                                        data-id="@item.Id"
                                        data-name="@item.Name"
                                        data-handler="Delete"
                                        title="حذف">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <!-- Pagination Controls -->
        @if (Model.TotalPages > 1)
        {
            <nav aria-label="Page navigation">
                <ul class="pagination justify-content-center mt-4">
                    <li class="page-item @(Model.HasPreviousPage ? "" : "disabled")">
                        <a class="page-link" asp-page="./Index"
                           asp-route-SearchTerm="@Model.SearchTerm"
                           asp-route-CurrentPage="@(Model.CurrentPage - 1)">السابق</a>
                    </li>
                    @for (var i = 1; i <= Model.TotalPages; i++)
                    {
                        <li class="page-item @(i == Model.CurrentPage ? "active" : "")">
                            <a class="page-link" asp-page="./Index"
                               asp-route-SearchTerm="@Model.SearchTerm"
                               asp-route-CurrentPage="@i">@i</a>
                        </li>
                    }
                    <li class="page-item @(Model.HasNextPage ? "" : "disabled")">
                        <a class="page-link" asp-page="./Index"
                           asp-route-SearchTerm="@Model.SearchTerm"
                           asp-route-CurrentPage="@(Model.CurrentPage + 1)">التالي</a>
                    </li>
                </ul>
            </nav>
        }
    </div>
</div>
  using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Customers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CustomerViewModel> Customers { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;


        public async Task OnGetAsync()
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(SearchTerm) || (c.PhoneNumber != null && c.PhoneNumber.Contains(SearchTerm)));
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)PageSize);

            var customersFromDb = await query.OrderBy(c => c.Name)
                                 .Skip((CurrentPage - 1) * PageSize)
                                 .Take(PageSize)
                                 .AsNoTracking()
                                 .ToListAsync();

            var customerIds = customersFromDb.Select(c => c.Id).ToList();
            var balances = await _context.CustomerTransactions
                .Where(t => customerIds.Contains(t.CustomerId))
                .GroupBy(t => t.CustomerId)
                .Select(g => new {
                    CustomerId = g.Key,
                    Balance = g.Sum(t => t.Debit - t.Credit)
                })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Balance);

            Customers = customersFromDb.Select(c => new CustomerViewModel
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                Balance = balances.GetValueOrDefault(c.Id, 0)
            }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // *** تم التعديل هنا: التحقق من وجود طلبيات مرتبطة قبل الحذف ***
            var hasSalesOrders = await _context.SalesOrders.AnyAsync(so => so.CustomerId == id);
            if (hasSalesOrders)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف هذا العميل لأنه مرتبط بطلبيات مسجلة في النظام.";
                return RedirectToPage();
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                // حذف المعاملات المالية المرتبطة بالعميل أولاً
                var transactions = await _context.CustomerTransactions.Where(t => t.CustomerId == id).ToListAsync();
                _context.CustomerTransactions.RemoveRange(transactions);

                // ثم حذف العميل نفسه
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"تم حذف العميل '{customer.Name}' وجميع معاملاته بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "لم يتم العثور على العميل المطلوب.";
            }

            return RedirectToPage();
        }
    }

    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Balance { get; set; }
    }
}  TempData["ErrorMessage"] = "لا يمكن حذف هذا العميل لأنه مرتبط بطلبيات مسجلة في النظام."; لا يظهر الكلام و تظهر شفافه فقط  
ChatGPT said:
تمام ✅
المشكلة إنك عامل TempData["ErrorMessage"] و TempData["SuccessMessage"] في الكود لكن ما عندكش مكان في الـ View أو الـ Layout بيعرض الرسائل (Toastr أو Alert).

بما إنك بالفعل مفعّل Toastr في site.js
، كل اللي محتاجه إنك تضيف snippet بسيط في _Layout.cshtml أو في صفحة Index.cshtml بعد الـ @RenderBody() عشان يقرأ الرسائل من TempData ويعرضها.

✨ الحل:
في _Layout.cshtml (أو جوه الصفحة لو حابب)، ضيف الكود ده قبل </body>:

@section Scripts {
    <script>
        @if (TempData["SuccessMessage"] != null)
        {
            <text>
                toastr.success("@TempData["SuccessMessage"]");
            </text>
        }

        @if (TempData["ErrorMessage"] != null)
        {
            <text>
                toastr.error("@TempData["ErrorMessage"]");
            </text>
        }
    </script>
}
ملاحظات مهمة:
site.js جهّز Toastr بالـ Options (زي مكان الظهور: toast-top-left).

لازم تتأكد إن ملفات toastr.min.css و toastr.min.js متضافة في _Layout.cshtml.
مثال:

<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />
<script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
النتيجة:
لو فيه TempData["ErrorMessage"] → هيظهر مربع أحمر برسالة الخطأ.

لو فيه TempData["SuccessMessage"] → هيظهر مربع أخضر بالنجاح.

تحب أظبطهالك في _Layout.cshtml بالكامل (مع روابط Toastr + مكان الرسائل) وأرسملهالك نسخة جاهزة؟


Sources
You said:
موافق
ChatGPT said:
تمام 🎯
أنا هجهزلك نسخة كاملة من ملف _Layout.cshtml متظبطة بحيث:

رابط CSS و JS الخاص بـ Toastr متضافين.

كود عرض رسائل TempData (نجاح / خطأ) جاهز يشتغل مع Toastr.

الكود يشتغل مع ملفاتك اللي رفعتها (site.css, site.js).

📌 الكود المقترح لـ _Layout.cshtml:
@using Microsoft.AspNetCore.Identity
@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Keswa</title>

    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.rtl.min.css" />

    <!-- Bootstrap Icons -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />

    <!-- Toastr CSS -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />

    <!-- موقعك CSS -->
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <div id="app-container" class="app-container">
        <!-- Sidebar -->
        <div class="app-sidebar">
            <div class="sidebar-header">
                <h2 class="sidebar-title">لوحة التحكم</h2>
            </div>
            <nav class="sidebar-nav">
                <a asp-page="/Index" class="nav-link"><i class="bi bi-house"></i><span>الرئيسية</span></a>
                <a asp-page="/Customers/Index" class="nav-link"><i class="bi bi-people"></i><span>العملاء</span></a>
                <a asp-page="/Sales/Index" class="nav-link"><i class="bi bi-bag"></i><span>المبيعات</span></a>
            </nav>
        </div>

        <!-- Main Area -->
        <div class="app-main">
            <!-- Header -->
            <header class="app-header d-flex justify-content-between align-items-center">
                <button id="sidebar-toggle-btn" class="btn btn-link btn-icon">
                    <i class="bi bi-list"></i>
                </button>
                <div>
                    @if (SignInManager.IsSignedIn(User))
                    {
                        <span class="me-2">مرحباً، @User.Identity?.Name</span>
                        <form method="post" asp-area="Identity" asp-page="/Account/Logout" class="d-inline">
                            <button type="submit" class="btn btn-sm btn-outline-secondary">تسجيل الخروج</button>
                        </form>
                    }
                </div>
            </header>

            <!-- Content -->
            <main class="app-content">
                @RenderBody()
            </main>
        </div>
    </div>

    <!-- jQuery -->
    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>

    <!-- Bootstrap Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>

    <!-- Toastr JS -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>

    <!-- موقعك JS -->
    <script src="~/js/site.js" asp-append-version="true"></script>

    @await RenderSectionAsync("Scripts", required: false)

    <!-- Toastr TempData Integration -->
    <script>
        @if (TempData["SuccessMessage"] != null)
        {
            <text>
                toastr.success("@TempData["SuccessMessage"]");
            </text>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <text>
                toastr.error("@TempData["ErrorMessage"]");
            </text>
        }
    </script>
</body>
</html>
🔹 بعد الكود ده:

لما تستدعي TempData["ErrorMessage"] = "..."; → هتظهر رسالة حمراء في أعلى الشمال.

لما تستدعي TempData["SuccessMessage"] = "..."; → هتظهر رسالة خضراء في نفس المكان.

الرسائل بتتقفل أوتوماتيك بعد وقت قصير (حسب إعدادات Toastr في site.js).

تحب أخلي الرسائل تظهر في المنتصف أعلى الصفحة بدل اليسار عشان تكون أوضح للمستخدم؟

You said:
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - نظام كسوة ERP</title>

    <!-- Google Fonts: Tajawal -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.rtl.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <div class="app-container" id="app-container">
        <!-- Sidebar -->
        <aside class="app-sidebar">
            <div class="sidebar-header">
                <h2 class="sidebar-title">نظام كسوة</h2>
            </div>
            <nav class="sidebar-nav">
                <a asp-page="/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="لوحة التحكم"><i class="bi bi-speedometer2"></i><span>لوحة التحكم</span></a>

                <p class="nav-section-title">إدارة المخزون</p>
                <a asp-page="/Inventory/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="المخازن"><i class="bi bi-shop"></i><span>المخازن</span></a>
                <a asp-page="/Materials/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="المواد الخام"><i class="bi bi-rulers"></i><span>المواد الخام</span></a>
                <a asp-page="/GoodsReceipts/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="سندات الاستلام"><i class="bi bi-box-arrow-in-down"></i><span>سندات الاستلام</span></a>
                <a asp-page="/PurchaseOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="أوامر الشراء"><i class="bi bi-cart-check"></i><span>أوامر الشراء</span></a>

                <p class="nav-section-title">إدارة الإنتاج</p>
                <a asp-page="/Products/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة الموديلات"><i class="bi bi-box-seam"></i><span>إدارة الموديلات</span></a>
                <a asp-page="/WorkOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="أوامر الشغل"><i class="bi bi-clipboard-data"></i><span>أوامر الشغل</span></a>

                <p class="nav-section-title">الموارد البشرية</p>
                <a asp-page="/Workers/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة العمال"><i class="bi bi-people-fill"></i><span>إدارة العمال</span></a>

                <p class="nav-section-title">إدارة المبيعات</p>
                <a asp-page="/Customers/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة العملاء"><i class="bi bi-person-rolodex"></i><span>إدارة العملاء</span></a>
                <a asp-page="/SalesOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة الطلبيات"><i class="bi bi-receipt"></i><span>إدارة الطلبيات</span></a>

                <p class="nav-section-title">إدارة التكاليف</p>
                <a asp-page="/Costs/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تكاليف الأقسام"><i class="bi bi-cash-stack"></i><span>تكاليف الأقسام</span></a>

                <p class="nav-section-title">التقارير</p>
                <a asp-page="/Reports/WorkerProductivity" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إنتاجية العمال"><i class="bi bi-graph-up"></i><span>إنتاجية العمال</span></a>
                <a asp-page="/Reports/WorkOrderCosting" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تكلفة أمر الشغل"><i class="bi bi-calculator-fill"></i><span>تكلفة أمر الشغل</span></a>
                <a asp-page="/Reports/MaterialAnalysis" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تحليل أسعار المواد"><i class="bi bi-bar-chart-line-fill"></i><span>تحليل أسعار المواد</span></a>
                <a asp-page="/Reports/InventoryValuation" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تقييم مخزون المواد"><i class="bi bi-safe-fill"></i><span>تقييم مخزون المواد</span></a>
                <a asp-page="/Reports/FinishedGoodsValuation" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تقييم مخزون المنتجات"><i class="bi bi-archive-fill"></i><span>تقييم مخزون المنتجات</span></a>

                <p class="nav-section-title">الإدارة</p>
                <a asp-page="/Administration/AuditLog" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="سجل المراجعة"><i class="bi bi-shield-lock-fill"></i><span>سجل المراجعة</span></a>
            </nav>
        </aside>

        <div class="app-main">
            <header class="app-header">
                <button class="btn btn-icon" id="sidebar-toggle-btn">
                    <i class="bi bi-list"></i>
                </button>
                <div class="d-flex align-items-center ms-auto">
                    <partial name="_LoginPartial" />
                </div>
            </header>
            <main class="app-content">
                @RenderBody()
            </main>
        </div>
    </div>

    <div class="modal fade" id="deleteConfirmationModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="deleteModalLabel">تأكيد الحذف</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>هل أنت متأكد أنك تريد حذف العنصر: <strong id="deleteItemName"></strong>؟</p>
                    <p class="text-danger">لا يمكن التراجع عن هذا الإجراء.</p>
                </div>
                <div class="modal-footer">
                    <form method="post" id="deleteForm">
                        <input type="hidden" id="deleteItemId" name="id" />
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إلغاء</button>
                        <button type="submit" class="btn btn-danger">تأكيد الحذف</button>
                    </form>
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>

    @{
        var successMessage = TempData["SuccessMessage"] as string;
        var errorMessage = TempData["ErrorMessage"] as string;
    }

    @if (!string.IsNullOrEmpty(successMessage))
    {
        <script>toastr.success('@Html.Raw(successMessage)');</script>
    }

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <script>toastr.error('@Html.Raw(errorMessage)');</script>
    }

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
  ده كود الحالى 
ChatGPT said:
ممتاز 👌
الكود بتاعك مرتب وكامل وكل حاجة متوصلة (Sidebar, Header, Modal, Toastr…).

السبب إنك شايف الرسالة شفافة فقط ومش بتبان بوضوح راجع لحاجتين:

1. إعدادات Toastr الافتراضية
في site.js عندك الإعداد:

toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-left", // لتظهر في أعلى اليسار
};
ده بيخلي الرسالة تطلع فوق الشمال، وأحيانًا مع RTL و الـ CSS ممكن تبان في مكان مش متوقع أو تبقى شبه شفافة.

2. الحل: تخصيص مكان الرسائل
لو تحب الرسائل تبان أوضح في نص الصفحة فوق (أفضل للمستخدم العربي)، غير السطر ده في site.js:

"positionClass": "toast-top-center",
3. حل إضافي (لو عايز تزود وضوح)
ممكن تزود شوية CSS للـ Toastr في site.css:

#toast-container > .toast {
    opacity: 1 !important; /* إلغاء الشفافية */
    border-radius: 8px;
    font-size: 1rem;
    direction: rtl;
    text-align: right;
}
4. النتيجة بعد التعديل
رسالة النجاح هتظهر في منتصف الشاشة من فوق بلون أخضر واضح.

رسالة الخطأ هتظهر في نفس المكان بلون أحمر واضح.

الاتنين قابلين للإغلاق وفيهم Progress bar.

تحب أعدلّهالك في نسخة جاهزة من site.js وsite.css بحيث الرسائل تظهر وسط الصفحة بدل اليسار وتكون أوضح؟

You said:
/* --- FILENAME: wwwroot/css/site.css --- */

:root {
    --sidebar-bg: #f8f9fa;
    --sidebar-width: 260px;
    --sidebar-width-collapsed: 80px;
    --header-height: 60px;
    --primary-text: #343a40;
    --secondary-text: #6c757d;
    --border-color: #dee2e6;
    --hover-bg: #e9ecef;
    --active-bg: #e0e7ff;
    --active-color: #4f46e5;
    --sidebar-section-title: #adb5bd;
    /* مدة الحركة الاحترافية */
    --transition-speed: 0.3s;
}

body {
    font-family: 'Tajawal', sans-serif;
    background-color: #f8f9fa;
    color: var(--primary-text);
    margin: 0;
    overflow-x: hidden;
}

.app-container {
    display: flex;
    min-height: 100vh;
}

/* Sidebar Styles */
.app-sidebar {
    width: var(--sidebar-width);
    background-color: #ffffff;
    border-left: 1px solid var(--border-color);
    display: flex;
    flex-direction: column;
    position: fixed;
    height: 100%;
    right: 0;
    top: 0;
    z-index: 1020;
    /* تأثير الحركة الرئيسي للقائمة */
    transition: width var(--transition-speed) ease-in-out, transform var(--transition-speed) ease-in-out;
}

.sidebar-header {
    padding: 0 1.5rem;
    border-bottom: 1px solid var(--border-color);
    height: var(--header-height);
    display: flex;
    align-items: center;
    flex-shrink: 0;
}

.sidebar-title {
    font-size: 1.5rem;
    font-weight: 700;
    color: var(--primary-text);
    margin: 0;
    white-space: nowrap;
    /* تأثير الحركة لعنوان القائمة */
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.sidebar-nav {
    padding: 1rem 0.75rem;
    flex-grow: 1;
    overflow-y: auto;
}

.nav-section-title {
    padding: 1rem 1rem 0.5rem;
    font-size: 0.75rem;
    font-weight: 700;
    color: var(--sidebar-section-title);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    white-space: nowrap;
    /* تأثير الحركة لعناوين الأقسام */
    transition: opacity calc(var(--transition-speed) / 2) ease;
}


.nav-link {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    margin-bottom: 0.25rem;
    border-radius: 0.375rem;
    color: var(--secondary-text);
    font-weight: 500;
    text-decoration: none;
    transition: background-color 0.2s ease, color 0.2s ease;
    white-space: nowrap;
    overflow: hidden;
}

    .nav-link i {
        font-size: 1.25rem;
        margin-left: 1rem;
        width: 24px;
        text-align: center;
        flex-shrink: 0;
        /* تأثير الحركة للأيقونة */
        transition: margin var(--transition-speed) ease-in-out;
    }

    .nav-link span {
        /* تأثير الحركة لنص الرابط */
        transition: opacity calc(var(--transition-speed) / 2) ease;
    }

    .nav-link:hover {
        background-color: var(--hover-bg);
        color: var(--primary-text);
    }

    .nav-link.active {
        background-color: var(--active-bg);
        color: var(--active-color);
        font-weight: 600;
    }

/* Main Content Area */
.app-main {
    flex-grow: 1;
    margin-right: var(--sidebar-width);
    display: flex;
    flex-direction: column;
    /* تأثير الحركة للمحتوى الرئيسي */
    transition: margin-right var(--transition-speed) ease-in-out;
}

/* Header Styles */
.app-header {
    height: var(--header-height);
    background-color: #ffffff;
    border-bottom: 1px solid var(--border-color);
    padding: 0 2rem;
    display: flex;
    align-items: center;
    position: sticky;
    top: 0;
    z-index: 1010;
}

.btn-icon {
    font-size: 1.5rem;
    color: var(--secondary-text);
}

/* Page Content */
.app-content {
    padding: 2rem;
    flex-grow: 1;
}

/* Collapsed Sidebar Styles */
.app-container.sidebar-collapsed .app-sidebar {
    width: var(--sidebar-width-collapsed);
}

.app-container.sidebar-collapsed .sidebar-title,
.app-container.sidebar-collapsed .nav-link span,
.app-container.sidebar-collapsed .nav-section-title {
    opacity: 0;
    pointer-events: none;
}

.app-container.sidebar-collapsed .nav-link i {
    margin-right: auto;
    margin-left: auto;
}

.app-container.sidebar-collapsed .app-main {
    margin-right: var(--sidebar-width-collapsed);
}


/* Responsive Styles for Tablet and Mobile */
@media (max-width: 992px) {
    .app-sidebar {
        transform: translateX(100%); /* إخفاء القائمة خارج الشاشة */
    }

    .app-main {
        margin-right: 0;
    }

    .app-container.sidebar-mobile-open .app-sidebar {
        transform: translateX(0); /* إظهار القائمة على الشاشة */
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }

    .sidebar-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0,0,0,0.5);
        z-index: 1015;
        opacity: 0;
        pointer-events: none;
        transition: opacity var(--transition-speed) ease-in-out;
    }

    .app-container.sidebar-mobile-open .sidebar-backdrop {
        opacity: 1;
        pointer-events: auto;
    }
}

/* Islamic Dashboard Styles & Other Page Styles */
.kpi-card {
    position: relative;
    overflow: hidden;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}

    .kpi-card:hover {
        transform: translateY(-5px);
        box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
    }

    .kpi-card .card-body {
        position: relative;
        z-index: 2;
    }

.chart-container {
    position: relative;
    height: 300px;
    width: 100%;
}

.auth-bg {
    background-color: #f0f2f5;
}

@media (max-width: 768px) {
    .app-content {
        padding: 1rem;
    }

    .h2 {
        font-size: 1.5rem;
    }

    .kpi-card .h3 {
        font-size: 1.75rem;
    }

    .chart-container {
        height: 250px;
    }
}

/* Modal Fix */
.modal {
    z-index: 1060 !important;
}

.modal-backdrop {
    z-index: 1050 !important;
}

.modal-dialog {
    transform: translateZ(0);
}  // انتظر حتى يتم تحميل الصفحة وجميع المكتبات (مثل jQuery) بشكل كامل
window.onload = function () {

    // 1. تفعيل البحث في جميع القوائم المنسدلة التي لها كلاس "searchable-select"
    if ($('.searchable-select').length > 0) {
        $('.searchable-select').select2({
            theme: "bootstrap-5",
            dir: "rtl",
            placeholder: '-- اختر --',
            width: '100%'
        });
    }

    // 2. إعدادات رسائل التأكيد (Toastr)
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-left", // لتظهر في أعلى اليسار
    };

    // 3. الكود الخاص بالتحكم في القائمة الجانبية (Sidebar)
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const appContainer = document.getElementById('app-container');
    let activeTooltips = []; // لتخزين التلميحات الفعالة

    // وظيفة لتفعيل التلميحات (Tooltips) على أيقونات القائمة
    function initializeTooltips() {
        destroyTooltips(); // حذف أي تلميحات قديمة أولاً
        const tooltipTriggers = document.querySelectorAll('.app-sidebar [data-bs-toggle="tooltip"]');
        tooltipTriggers.forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            activeTooltips.push(tooltip);
        });
    }

    // وظيفة لإلغاء وحذف جميع التلميحات الفعالة
    function destroyTooltips() {
        activeTooltips.forEach(tooltip => {
            if (tooltip) {
                tooltip.dispose();
            }
        });
        activeTooltips = [];
    }

    // وظيفة للتحقق من حالة القائمة وتحديد ما إذا كان يجب إظهار التلميحات
    function updateTooltipState() {
        // أظهر التلميحات فقط في شاشات سطح المكتب وعندما تكون القائمة مصغرة
        if (appContainer && window.innerWidth > 992 && appContainer.classList.contains('sidebar-collapsed')) {
            initializeTooltips();
        } else {
            destroyTooltips();
        }
    }

    // تأكد من وجود زر التحكم والحاوية الرئيسية قبل تشغيل الكود
    if (sidebarToggleBtn && appContainer) {

        // الوظيفة الرئيسية لفتح وإغلاق القائمة
        function toggleSidebar() {
            // في حالة شاشات الموبايل والتابلت
            if (window.innerWidth <= 992) {
                appContainer.classList.toggle('sidebar-mobile-open');

                // إنشاء أو حذف خلفية معتمة
                let backdrop = document.querySelector('.sidebar-backdrop');
                if (appContainer.classList.contains('sidebar-mobile-open')) {
                    if (!backdrop) {
                        backdrop = document.createElement('div');
                        backdrop.className = 'sidebar-backdrop';
                        document.body.appendChild(backdrop);
                        backdrop.addEventListener('click', toggleSidebar); // لإغلاق القائمة عند الضغط على الخلفية
                    }
                } else {
                    if (backdrop) {
                        backdrop.remove();
                    }
                }
            } else {
                // في حالة شاشات سطح المكتب
                appContainer.classList.toggle('sidebar-collapsed');
            }
            // تحديث حالة التلميحات بعد كل عملية فتح أو إغلاق
            updateTooltipState();
        }

        // ربط وظيفة الفتح والإغلاق بزر القائمة
        sidebarToggleBtn.addEventListener('click', toggleSidebar);

        // التحقق من حالة التلميحات عند تحميل الصفحة لأول مرة
        updateTooltipState();
    }

    // 4. تفعيل نافذة الحذف المنبثقة (Delete Modal)
    const deleteModal = document.getElementById('deleteConfirmationModal');
    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const itemId = button.getAttribute('data-id');
            const itemName = button.getAttribute('data-name');
            const handler = button.getAttribute('data-handler');

            const modalBodyName = deleteModal.querySelector('#deleteItemName');
            const deleteForm = deleteModal.querySelector('#deleteForm');
            const deleteItemIdInput = deleteModal.querySelector('#deleteItemId');

            modalBodyName.textContent = itemName;
            deleteItemIdInput.value = itemId;
            deleteForm.action = ?handler=${handler};
        });
    }

    // 5. إضافة مؤشرات التحميل للأزرار
    $('form').submit(function () {
        const submitButton = $(this).find('button[type="submit"]');
        if (submitButton) {
            submitButton.prop('disabled', true);
            submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> جاري الحفظ...');
        }
    });
};
  
ChatGPT said:
تمام ✅
أنا شايف إنك حاطط CSS و JS في ملف واحد هنا، لكن في مشروعك الحقيقي عندك site.css و site.js منفصلين. خليني أظبطلك الجزء الخاص بالـ Toastr بحيث الرسائل تبان واضحة وفي نص الشاشة فوق، وكمان أضيف شوية CSS تمنع الشفافية.

🔧 التعديل في site.js
غير إعداد toastr.options بالشكل ده:

// 2. إعدادات رسائل التأكيد (Toastr)
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-center", // لتظهر في أعلى منتصف الصفحة
    "timeOut": "5000", // 5 ثواني وتختفي
    "extendedTimeOut": "2000" // لو المستخدم وقف عليها
};
🔧 الإضافة في site.css
ضيف في آخر الملف:

/* Toastr Customization */
#toast-container > .toast {
    opacity: 1 !important;        /* إلغاء الشفافية */
    border-radius: 8px;           /* شكل ألطف */
    font-size: 1rem;              /* خط واضح */
    direction: rtl;               /* محاذاة صحيحة للعربي */
    text-align: right;
}
✅ النتيجة
رسائل الخطأ (TempData["ErrorMessage"]) هتظهر أحمر في نص الشاشة من فوق.

رسائل النجاح (TempData["SuccessMessage"]) هتظهر أخضر بنفس المكان.

الرسائل أوضح ومش شفافة.

المستخدم يقدر يقفلها من زرار X.

تحب أجهزلك نسخة كاملة من site.js و site.css بعد التعديل بحيث تنزلها وتستبدلها على طول؟

You said:
موافق
ChatGPT said:
ممتاز 👌
أنا جهزتلك نسختين كاملتين من الملفات بعد التعديلات:

📌 site.css (مُحدّث)
/* --- FILENAME: wwwroot/css/site.css --- */

:root {
    --sidebar-bg: #f8f9fa;
    --sidebar-width: 260px;
    --sidebar-width-collapsed: 80px;
    --header-height: 60px;
    --primary-text: #343a40;
    --secondary-text: #6c757d;
    --border-color: #dee2e6;
    --hover-bg: #e9ecef;
    --active-bg: #e0e7ff;
    --active-color: #4f46e5;
    --sidebar-section-title: #adb5bd;
    --transition-speed: 0.3s;
}

body {
    font-family: 'Tajawal', sans-serif;
    background-color: #f8f9fa;
    color: var(--primary-text);
    margin: 0;
    overflow-x: hidden;
}

.app-container {
    display: flex;
    min-height: 100vh;
}

/* Sidebar Styles */
.app-sidebar {
    width: var(--sidebar-width);
    background-color: #ffffff;
    border-left: 1px solid var(--border-color);
    display: flex;
    flex-direction: column;
    position: fixed;
    height: 100%;
    right: 0;
    top: 0;
    z-index: 1020;
    transition: width var(--transition-speed) ease-in-out, transform var(--transition-speed) ease-in-out;
}

.sidebar-header {
    padding: 0 1.5rem;
    border-bottom: 1px solid var(--border-color);
    height: var(--header-height);
    display: flex;
    align-items: center;
    flex-shrink: 0;
}

.sidebar-title {
    font-size: 1.5rem;
    font-weight: 700;
    color: var(--primary-text);
    margin: 0;
    white-space: nowrap;
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.sidebar-nav {
    padding: 1rem 0.75rem;
    flex-grow: 1;
    overflow-y: auto;
}

.nav-section-title {
    padding: 1rem 1rem 0.5rem;
    font-size: 0.75rem;
    font-weight: 700;
    color: var(--sidebar-section-title);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    white-space: nowrap;
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.nav-link {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    margin-bottom: 0.25rem;
    border-radius: 0.375rem;
    color: var(--secondary-text);
    font-weight: 500;
    text-decoration: none;
    transition: background-color 0.2s ease, color 0.2s ease;
    white-space: nowrap;
    overflow: hidden;
}

.nav-link i {
    font-size: 1.25rem;
    margin-left: 1rem;
    width: 24px;
    text-align: center;
    flex-shrink: 0;
    transition: margin var(--transition-speed) ease-in-out;
}

.nav-link span {
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.nav-link:hover {
    background-color: var(--hover-bg);
    color: var(--primary-text);
}

.nav-link.active {
    background-color: var(--active-bg);
    color: var(--active-color);
    font-weight: 600;
}

/* Main Content Area */
.app-main {
    flex-grow: 1;
    margin-right: var(--sidebar-width);
    display: flex;
    flex-direction: column;
    transition: margin-right var(--transition-speed) ease-in-out;
}

/* Header Styles */
.app-header {
    height: var(--header-height);
    background-color: #ffffff;
    border-bottom: 1px solid var(--border-color);
    padding: 0 2rem;
    display: flex;
    align-items: center;
    position: sticky;
    top: 0;
    z-index: 1010;
}

.btn-icon {
    font-size: 1.5rem;
    color: var(--secondary-text);
}

/* Page Content */
.app-content {
    padding: 2rem;
    flex-grow: 1;
}

/* Collapsed Sidebar Styles */
.app-container.sidebar-collapsed .app-sidebar {
    width: var(--sidebar-width-collapsed);
}

.app-container.sidebar-collapsed .sidebar-title,
.app-container.sidebar-collapsed .nav-link span,
.app-container.sidebar-collapsed .nav-section-title {
    opacity: 0;
    pointer-events: none;
}

.app-container.sidebar-collapsed .nav-link i {
    margin-right: auto;
    margin-left: auto;
}

.app-container.sidebar-collapsed .app-main {
    margin-right: var(--sidebar-width-collapsed);
}

/* Responsive Styles */
@media (max-width: 992px) {
    .app-sidebar {
        transform: translateX(100%);
    }
    .app-main {
        margin-right: 0;
    }
    .app-container.sidebar-mobile-open .app-sidebar {
        transform: translateX(0);
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }
    .sidebar-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0,0,0,0.5);
        z-index: 1015;
        opacity: 0;
        pointer-events: none;
        transition: opacity var(--transition-speed) ease-in-out;
    }
    .app-container.sidebar-mobile-open .sidebar-backdrop {
        opacity: 1;
        pointer-events: auto;
    }
}

/* KPI Cards */
.kpi-card {
    position: relative;
    overflow: hidden;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.kpi-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
}
.kpi-card .card-body {
    position: relative;
    z-index: 2;
}

.chart-container {
    position: relative;
    height: 300px;
    width: 100%;
}

.auth-bg {
    background-color: #f0f2f5;
}

@media (max-width: 768px) {
    .app-content {
        padding: 1rem;
    }
    .h2 {
        font-size: 1.5rem;
    }
    .kpi-card .h3 {
        font-size: 1.75rem;
    }
    .chart-container {
        height: 250px;
    }
}

/* Modal Fix */
.modal {
    z-index: 1060 !important;
}
.modal-backdrop {
    z-index: 1050 !important;
}
.modal-dialog {
    transform: translateZ(0);
}

/* ✅ Toastr Customization */
#toast-container > .toast {
    opacity: 1 !important;
    border-radius: 8px;
    font-size: 1rem;
    direction: rtl;
    text-align: right;
}
📌 site.js (مُحدّث)
﻿// انتظر حتى يتم تحميل الصفحة
window.onload = function () {

    // 1. تفعيل البحث في القوائم المنسدلة
    if ($('.searchable-select').length > 0) {
        $('.searchable-select').select2({
            theme: "bootstrap-5",
            dir: "rtl",
            placeholder: '-- اختر --',
            width: '100%'
        });
    }

    // 2. إعدادات رسائل التأكيد (Toastr)
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-center", // 👈 وسط الصفحة فوق
        "timeOut": "5000",
        "extendedTimeOut": "2000"
    };

    // 3. التحكم في القائمة الجانبية
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const appContainer = document.getElementById('app-container');
    let activeTooltips = [];

    function initializeTooltips() {
        destroyTooltips();
        const tooltipTriggers = document.querySelectorAll('.app-sidebar [data-bs-toggle="tooltip"]');
        tooltipTriggers.forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            activeTooltips.push(tooltip);
        });
    }

    function destroyTooltips() {
        activeTooltips.forEach(tooltip => {
            if (tooltip) tooltip.dispose();
        });
        activeTooltips = [];
    }

    function updateTooltipState() {
        if (appContainer && window.innerWidth > 992 && appContainer.classList.contains('sidebar-collapsed')) {
            initializeTooltips();
        } else {
            destroyTooltips();
        }
    }

    if (sidebarToggleBtn && appContainer) {
        function toggleSidebar() {
            if (window.innerWidth <= 992) {
                appContainer.classList.toggle('sidebar-mobile-open');
                let backdrop = document.querySelector('.sidebar-backdrop');
                if (appContainer.classList.contains('sidebar-mobile-open')) {
                    if (!backdrop) {
                        backdrop = document.createElement('div');
                        backdrop.className = 'sidebar-backdrop';
                        document.body.appendChild(backdrop);
                        backdrop.addEventListener('click', toggleSidebar);
                    }
                } else {
                    if (backdrop) backdrop.remove();
                }
            } else {
                appContainer.classList.toggle('sidebar-collapsed');
            }
            updateTooltipState();
        }
        sidebarToggleBtn.addEventListener('click', toggleSidebar);
        updateTooltipState();
    }

    // 4. نافذة الحذف المنبثقة
    const deleteModal = document.getElementById('deleteConfirmationModal');
    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const itemId = button.getAttribute('data-id');
            const itemName = button.getAttribute('data-name');
            const handler = button.getAttribute('data-handler');

            const modalBodyName = deleteModal.querySelector('#deleteItemName');
            const deleteForm = deleteModal.querySelector('#deleteForm');
            const deleteItemIdInput = deleteModal.querySelector('#deleteItemId');

            modalBodyName.textContent = itemName;
            deleteItemIdInput.value = itemId;
            deleteForm.action = `?handler=${handler}`;
        });
    }

    // 5. مؤشرات التحميل للأزرار
    $('form').submit(function () {
        const submitButton = $(this).find('button[type="submit"]');
        if (submitButton) {
            submitButton.prop('disabled', true);
            submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> جاري الحفظ...');
        }
    });
};
✨ دلوقتي لما تستخدم TempData["ErrorMessage"] أو TempData["SuccessMessage"]:

الرسائل هتظهر في وسط الصفحة فوق.

أوضح (من غير شفافية).

فيها زر إغلاق + Progress bar.

تحب أجهزلك كمان نسخة ZIP مضغوطة فيها الملفات دي عشان تحملها مباشرة وتستبدلها؟

You said:
الكلام لا يظهر نهائي و لكن الرساله تظهر شفافه 
ChatGPT said:
تمام يا أحمد 👌 وصلت الفكرة.
الرسالة نفسها بتطلع (شفافة وفاضية) → ده معناه إن الـ Toastr container موجود لكن الـ نص الرسالة بيتقطع أو مش بيوصل صح للـ JS.

خليني أوضحلك النقاط اللي ممكن تكون السبب:

✅ 1. طريقة تمرير النص من TempData
في الـ _Layout.cshtml عندك:

@if (!string.IsNullOrEmpty(errorMessage))
{
    <script>toastr.error('@Html.Raw(errorMessage)');</script>
}
هنا لو الرسالة فيها علامات اقتباس " " أو أحرف خاصة زي '، الـ JavaScript هيكسر.
الأفضل تستخدم JavaScriptEncoder أو تعمل Json.Encode:

@using System.Text.Encodings.Web
@using System.Text.Json

@if (!string.IsNullOrEmpty(successMessage))
{
    <script>
        toastr.success(@JsonSerializer.Serialize(successMessage));
    </script>
}
@if (!string.IsNullOrEmpty(errorMessage))
{
    <script>
        toastr.error(@JsonSerializer.Serialize(errorMessage));
    </script>
}
🔹 كده أي نص (حتى بالعربي أو فيه علامات) هيطلع مظبوط.

✅ 2. مشكلة الشفافية
انت ضفت CSS لكن أظن إن Toastr بيشتغل بالـ opacity transitions. الحل:

في site.css أضف:

#toast-container > .toast {
    opacity: 1 !important;
    background-color: #fff !important;
    color: #000 !important;
}
#toast-container > .toast.toast-success {
    background-color: #28a745 !important;
    color: #fff !important;
}
#toast-container > .toast.toast-error {
    background-color: #dc3545 !important;
    color: #fff !important;
}
ده هيخلي الخلفية والألوان واضحة والنص أبيض على ألوان النجاح/الخطأ.

✅ 3. تتأكد إن toastr.min.css موجود
انت بالفعل مستدعي:

<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />
خليه فوق كل CSS عشان ما يتلغاش بالـ Overrides.

🔎 جرّب تعديلات النقطة 1 + 2، هتلاقي الرسالة تظهر كاملة وواضحة.

تحب أديك نسخة معدلة من مقطع الـ TempData + CSS كامل وأرسملهالك زي ما هي؟

You said:
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - نظام كسوة ERP</title>

    <!-- Google Fonts: Tajawal -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.rtl.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <div class="app-container" id="app-container">
        <!-- Sidebar -->
        <aside class="app-sidebar">
            <div class="sidebar-header">
                <h2 class="sidebar-title">نظام كسوة</h2>
            </div>
            <nav class="sidebar-nav">
                <a asp-page="/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="لوحة التحكم"><i class="bi bi-speedometer2"></i><span>لوحة التحكم</span></a>

                <p class="nav-section-title">إدارة المخزون</p>
                <a asp-page="/Inventory/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="المخازن"><i class="bi bi-shop"></i><span>المخازن</span></a>
                <a asp-page="/Materials/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="المواد الخام"><i class="bi bi-rulers"></i><span>المواد الخام</span></a>
                <a asp-page="/GoodsReceipts/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="سندات الاستلام"><i class="bi bi-box-arrow-in-down"></i><span>سندات الاستلام</span></a>
                <a asp-page="/PurchaseOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="أوامر الشراء"><i class="bi bi-cart-check"></i><span>أوامر الشراء</span></a>

                <p class="nav-section-title">إدارة الإنتاج</p>
                <a asp-page="/Products/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة الموديلات"><i class="bi bi-box-seam"></i><span>إدارة الموديلات</span></a>
                <a asp-page="/WorkOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="أوامر الشغل"><i class="bi bi-clipboard-data"></i><span>أوامر الشغل</span></a>

                <p class="nav-section-title">الموارد البشرية</p>
                <a asp-page="/Workers/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة العمال"><i class="bi bi-people-fill"></i><span>إدارة العمال</span></a>

                <p class="nav-section-title">إدارة المبيعات</p>
                <a asp-page="/Customers/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة العملاء"><i class="bi bi-person-rolodex"></i><span>إدارة العملاء</span></a>
                <a asp-page="/SalesOrders/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إدارة الطلبيات"><i class="bi bi-receipt"></i><span>إدارة الطلبيات</span></a>

                <p class="nav-section-title">إدارة التكاليف</p>
                <a asp-page="/Costs/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تكاليف الأقسام"><i class="bi bi-cash-stack"></i><span>تكاليف الأقسام</span></a>

                <p class="nav-section-title">التقارير</p>
                <a asp-page="/Reports/WorkerProductivity" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="إنتاجية العمال"><i class="bi bi-graph-up"></i><span>إنتاجية العمال</span></a>
                <a asp-page="/Reports/WorkOrderCosting" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تكلفة أمر الشغل"><i class="bi bi-calculator-fill"></i><span>تكلفة أمر الشغل</span></a>
                <a asp-page="/Reports/MaterialAnalysis" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تحليل أسعار المواد"><i class="bi bi-bar-chart-line-fill"></i><span>تحليل أسعار المواد</span></a>
                <a asp-page="/Reports/InventoryValuation" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تقييم مخزون المواد"><i class="bi bi-safe-fill"></i><span>تقييم مخزون المواد</span></a>
                <a asp-page="/Reports/FinishedGoodsValuation" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="تقييم مخزون المنتجات"><i class="bi bi-archive-fill"></i><span>تقييم مخزون المنتجات</span></a>

                <p class="nav-section-title">الإدارة</p>
                <a asp-page="/Administration/AuditLog" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="سجل المراجعة"><i class="bi bi-shield-lock-fill"></i><span>سجل المراجعة</span></a>
            </nav>
        </aside>

        <div class="app-main">
            <header class="app-header">
                <button class="btn btn-icon" id="sidebar-toggle-btn">
                    <i class="bi bi-list"></i>
                </button>
                <div class="d-flex align-items-center ms-auto">
                    <partial name="_LoginPartial" />
                </div>
            </header>
            <main class="app-content">
                @RenderBody()
            </main>
        </div>
    </div>

    <div class="modal fade" id="deleteConfirmationModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="deleteModalLabel">تأكيد الحذف</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>هل أنت متأكد أنك تريد حذف العنصر: <strong id="deleteItemName"></strong>؟</p>
                    <p class="text-danger">لا يمكن التراجع عن هذا الإجراء.</p>
                </div>
                <div class="modal-footer">
                    <form method="post" id="deleteForm">
                        <input type="hidden" id="deleteItemId" name="id" />
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إلغاء</button>
                        <button type="submit" class="btn btn-danger">تأكيد الحذف</button>
                    </form>
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>

    @{
        var successMessage = TempData["SuccessMessage"] as string;
        var errorMessage = TempData["ErrorMessage"] as string;
    }

    @if (!string.IsNullOrEmpty(successMessage))
    {
        <script>toastr.success('@Html.Raw(successMessage)');</script>
    }

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <script>toastr.error('@Html.Raw(errorMessage)');</script>
    }

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
  ممكن تعدله 
ChatGPT said:
تمام 👌
أنا هعدّلك الكود بحيث رسائل TempData تظهر في Toastr بشكل صحيح (من غير ما تبقى فاضية أو شفافة).

المشكلة إنك كنت مستخدم:

toastr.error('@Html.Raw(errorMessage)');
وده ممكن يكسر الـ JavaScript لو الرسالة فيها علامات اقتباس أو حروف خاصة.

✅ الحل: استخدم JsonSerializer.Serialize عشان يطلع النص آمن ومقريء في JavaScript
الكود المعدّل:

@using System.Text.Json

@{
    var successMessage = TempData["SuccessMessage"] as string;
    var errorMessage = TempData["ErrorMessage"] as string;
}

@if (!string.IsNullOrEmpty(successMessage))
{
    <script>
        toastr.success(@JsonSerializer.Serialize(successMessage));
    </script>
}

@if (!string.IsNullOrEmpty(errorMessage))
{
    <script>
        toastr.error(@JsonSerializer.Serialize(errorMessage));
    </script>
}
✅ تحسين إضافي للألوان والوضوح (في site.css)
ضيف في آخر ملف site.css:

#toast-container > .toast {
    opacity: 1 !important;       /* إلغاء الشفافية */
    border-radius: 8px;
    font-size: 1rem;
    direction: rtl;
    text-align: right;
}
#toast-container > .toast.toast-success {
    background-color: #28a745 !important;
    color: #fff !important;
}
#toast-container > .toast.toast-error {
    background-color: #dc3545 !important;
    color: #fff !important;
}
🔎 النتيجة بعد التعديل
الرسائل هتظهر في Toastr بنص واضح.

ملهاش شفافية زيادة.

بالعربي من اليمين لليسار.

الألوان (أخضر = نجاح / أحمر = خطأ).

تحب أجهزلك نسخة كاملة من _Layout.cshtml فيها التعديلات دي مضبوطة وأرسلها لك كملف جاهز تستخدمه مباشرة؟

You said:
موافق
ChatGPT said:
تمام يا أحمد ✅
أنا ظبطتلك نسخة كاملة من _Layout.cshtml بالتعديلات المطلوبة:

الرسائل بتتعرض في Toastr باستخدام JsonSerializer.Serialize عشان النص يظهر مضبوط حتى لو فيه علامات اقتباس أو حروف خاصة.

كود CSS اتظبط (هتضيفه في site.css زي ما وريتك قبل كده) عشان يمنع الشفافية ويظهر الألوان واضحة.

📌 الكود المعدل: _Layout.cshtml
@using System.Text.Json
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - نظام كسوة ERP</title>

    <!-- Google Fonts: Tajawal -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700&display=swap" rel="stylesheet">

    <!-- Bootstrap & Icons -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.rtl.min.css" />

    <!-- Select2 -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" />

    <!-- Toastr -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" />

    <!-- موقعك CSS -->
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <div class="app-container" id="app-container">
        <!-- Sidebar -->
        <aside class="app-sidebar">
            <div class="sidebar-header">
                <h2 class="sidebar-title">نظام كسوة</h2>
            </div>
            <nav class="sidebar-nav">
                <a asp-page="/Index" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="left" title="لوحة التحكم"><i class="bi bi-speedometer2"></i><span>لوحة التحكم</span></a>

                <p class="nav-section-title">إدارة المخزون</p>
                <a asp-page="/Inventory/Index" class="nav-link" data-bs-toggle="tooltip" title="المخازن"><i class="bi bi-shop"></i><span>المخازن</span></a>
                <a asp-page="/Materials/Index" class="nav-link" data-bs-toggle="tooltip" title="المواد الخام"><i class="bi bi-rulers"></i><span>المواد الخام</span></a>
                <a asp-page="/GoodsReceipts/Index" class="nav-link" data-bs-toggle="tooltip" title="سندات الاستلام"><i class="bi bi-box-arrow-in-down"></i><span>سندات الاستلام</span></a>
                <a asp-page="/PurchaseOrders/Index" class="nav-link" data-bs-toggle="tooltip" title="أوامر الشراء"><i class="bi bi-cart-check"></i><span>أوامر الشراء</span></a>

                <p class="nav-section-title">إدارة الإنتاج</p>
                <a asp-page="/Products/Index" class="nav-link" data-bs-toggle="tooltip" title="إدارة الموديلات"><i class="bi bi-box-seam"></i><span>إدارة الموديلات</span></a>
                <a asp-page="/WorkOrders/Index" class="nav-link" data-bs-toggle="tooltip" title="أوامر الشغل"><i class="bi bi-clipboard-data"></i><span>أوامر الشغل</span></a>

                <p class="nav-section-title">الموارد البشرية</p>
                <a asp-page="/Workers/Index" class="nav-link" data-bs-toggle="tooltip" title="إدارة العمال"><i class="bi bi-people-fill"></i><span>إدارة العمال</span></a>

                <p class="nav-section-title">إدارة المبيعات</p>
                <a asp-page="/Customers/Index" class="nav-link" data-bs-toggle="tooltip" title="إدارة العملاء"><i class="bi bi-person-rolodex"></i><span>إدارة العملاء</span></a>
                <a asp-page="/SalesOrders/Index" class="nav-link" data-bs-toggle="tooltip" title="إدارة الطلبيات"><i class="bi bi-receipt"></i><span>إدارة الطلبيات</span></a>

                <p class="nav-section-title">إدارة التكاليف</p>
                <a asp-page="/Costs/Index" class="nav-link" data-bs-toggle="tooltip" title="تكاليف الأقسام"><i class="bi bi-cash-stack"></i><span>تكاليف الأقسام</span></a>

                <p class="nav-section-title">التقارير</p>
                <a asp-page="/Reports/WorkerProductivity" class="nav-link" data-bs-toggle="tooltip" title="إنتاجية العمال"><i class="bi bi-graph-up"></i><span>إنتاجية العمال</span></a>
                <a asp-page="/Reports/WorkOrderCosting" class="nav-link" data-bs-toggle="tooltip" title="تكلفة أمر الشغل"><i class="bi bi-calculator-fill"></i><span>تكلفة أمر الشغل</span></a>
                <a asp-page="/Reports/MaterialAnalysis" class="nav-link" data-bs-toggle="tooltip" title="تحليل أسعار المواد"><i class="bi bi-bar-chart-line-fill"></i><span>تحليل أسعار المواد</span></a>
                <a asp-page="/Reports/InventoryValuation" class="nav-link" data-bs-toggle="tooltip" title="تقييم مخزون المواد"><i class="bi bi-safe-fill"></i><span>تقييم مخزون المواد</span></a>
                <a asp-page="/Reports/FinishedGoodsValuation" class="nav-link" data-bs-toggle="tooltip" title="تقييم مخزون المنتجات"><i class="bi bi-archive-fill"></i><span>تقييم مخزون المنتجات</span></a>

                <p class="nav-section-title">الإدارة</p>
                <a asp-page="/Administration/AuditLog" class="nav-link" data-bs-toggle="tooltip" title="سجل المراجعة"><i class="bi bi-shield-lock-fill"></i><span>سجل المراجعة</span></a>
            </nav>
        </aside>

        <!-- Main -->
        <div class="app-main">
            <header class="app-header">
                <button class="btn btn-icon" id="sidebar-toggle-btn">
                    <i class="bi bi-list"></i>
                </button>
                <div class="d-flex align-items-center ms-auto">
                    <partial name="_LoginPartial" />
                </div>
            </header>
            <main class="app-content">
                @RenderBody()
            </main>
        </div>
    </div>

    <!-- Delete Modal -->
    <div class="modal fade" id="deleteConfirmationModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="deleteModalLabel">تأكيد الحذف</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>هل أنت متأكد أنك تريد حذف العنصر: <strong id="deleteItemName"></strong>؟</p>
                    <p class="text-danger">لا يمكن التراجع عن هذا الإجراء.</p>
                </div>
                <div class="modal-footer">
                    <form method="post" id="deleteForm">
                        <input type="hidden" id="deleteItemId" name="id" />
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إلغاء</button>
                        <button type="submit" class="btn btn-danger">تأكيد الحذف</button>
                    </form>
                </div>
            </div>
        </div>
    </div>

    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>

    @{
        var successMessage = TempData["SuccessMessage"] as string;
        var errorMessage = TempData["ErrorMessage"] as string;
    }

    @if (!string.IsNullOrEmpty(successMessage))
    {
        <script>
            toastr.success(@JsonSerializer.Serialize(successMessage));
        </script>
    }

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <script>
            toastr.error(@JsonSerializer.Serialize(errorMessage));
        </script>
    }

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
👨‍💻 مع ده + تعديل الـ CSS اللي قلتلك عليه، الرسائل هتظهر مظبوطة وواضحة في Toastr.

تحب أجهزلك كمان ملف site.css بعد التعديلات وأبعتهولك نسخة كاملة؟

You said:
موافق
ChatGPT said:
ممتاز 👌
دلوقتي أديك نسخة كاملة من site.css بعد التعديلات الخاصة بالـ Toastr (علشان الرسائل متبقاش شفافة وتظهر ألوان واضحة):

📌 site.css (مُحدث)
/* --- FILENAME: wwwroot/css/site.css --- */

:root {
    --sidebar-bg: #f8f9fa;
    --sidebar-width: 260px;
    --sidebar-width-collapsed: 80px;
    --header-height: 60px;
    --primary-text: #343a40;
    --secondary-text: #6c757d;
    --border-color: #dee2e6;
    --hover-bg: #e9ecef;
    --active-bg: #e0e7ff;
    --active-color: #4f46e5;
    --sidebar-section-title: #adb5bd;
    --transition-speed: 0.3s;
}

body {
    font-family: 'Tajawal', sans-serif;
    background-color: #f8f9fa;
    color: var(--primary-text);
    margin: 0;
    overflow-x: hidden;
}

.app-container {
    display: flex;
    min-height: 100vh;
}

/* Sidebar */
.app-sidebar {
    width: var(--sidebar-width);
    background-color: #ffffff;
    border-left: 1px solid var(--border-color);
    display: flex;
    flex-direction: column;
    position: fixed;
    height: 100%;
    right: 0;
    top: 0;
    z-index: 1020;
    transition: width var(--transition-speed) ease-in-out, transform var(--transition-speed) ease-in-out;
}

.sidebar-header {
    padding: 0 1.5rem;
    border-bottom: 1px solid var(--border-color);
    height: var(--header-height);
    display: flex;
    align-items: center;
    flex-shrink: 0;
}

.sidebar-title {
    font-size: 1.5rem;
    font-weight: 700;
    color: var(--primary-text);
    margin: 0;
    white-space: nowrap;
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.sidebar-nav {
    padding: 1rem 0.75rem;
    flex-grow: 1;
    overflow-y: auto;
}

.nav-section-title {
    padding: 1rem 1rem 0.5rem;
    font-size: 0.75rem;
    font-weight: 700;
    color: var(--sidebar-section-title);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    white-space: nowrap;
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.nav-link {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    margin-bottom: 0.25rem;
    border-radius: 0.375rem;
    color: var(--secondary-text);
    font-weight: 500;
    text-decoration: none;
    transition: background-color 0.2s ease, color 0.2s ease;
    white-space: nowrap;
    overflow: hidden;
}

.nav-link i {
    font-size: 1.25rem;
    margin-left: 1rem;
    width: 24px;
    text-align: center;
    flex-shrink: 0;
    transition: margin var(--transition-speed) ease-in-out;
}

.nav-link span {
    transition: opacity calc(var(--transition-speed) / 2) ease;
}

.nav-link:hover {
    background-color: var(--hover-bg);
    color: var(--primary-text);
}

.nav-link.active {
    background-color: var(--active-bg);
    color: var(--active-color);
    font-weight: 600;
}

/* Main Content */
.app-main {
    flex-grow: 1;
    margin-right: var(--sidebar-width);
    display: flex;
    flex-direction: column;
    transition: margin-right var(--transition-speed) ease-in-out;
}

/* Header */
.app-header {
    height: var(--header-height);
    background-color: #ffffff;
    border-bottom: 1px solid var(--border-color);
    padding: 0 2rem;
    display: flex;
    align-items: center;
    position: sticky;
    top: 0;
    z-index: 1010;
}

.btn-icon {
    font-size: 1.5rem;
    color: var(--secondary-text);
}

/* Page Content */
.app-content {
    padding: 2rem;
    flex-grow: 1;
}

/* Collapsed Sidebar */
.app-container.sidebar-collapsed .app-sidebar {
    width: var(--sidebar-width-collapsed);
}

.app-container.sidebar-collapsed .sidebar-title,
.app-container.sidebar-collapsed .nav-link span,
.app-container.sidebar-collapsed .nav-section-title {
    opacity: 0;
    pointer-events: none;
}

.app-container.sidebar-collapsed .nav-link i {
    margin-right: auto;
    margin-left: auto;
}

.app-container.sidebar-collapsed .app-main {
    margin-right: var(--sidebar-width-collapsed);
}

/* Responsive */
@media (max-width: 992px) {
    .app-sidebar {
        transform: translateX(100%);
    }
    .app-main {
        margin-right: 0;
    }
    .app-container.sidebar-mobile-open .app-sidebar {
        transform: translateX(0);
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }
    .sidebar-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0,0,0,0.5);
        z-index: 1015;
        opacity: 0;
        pointer-events: none;
        transition: opacity var(--transition-speed) ease-in-out;
    }
    .app-container.sidebar-mobile-open .sidebar-backdrop {
        opacity: 1;
        pointer-events: auto;
    }
}

/* KPI Cards */
.kpi-card {
    position: relative;
    overflow: hidden;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.kpi-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
}
.kpi-card .card-body {
    position: relative;
    z-index: 2;
}

.chart-container {
    position: relative;
    height: 300px;
    width: 100%;
}

.auth-bg {
    background-color: #f0f2f5;
}

@media (max-width: 768px) {
    .app-content {
        padding: 1rem;
    }
    .h2 {
        font-size: 1.5rem;
    }
    .kpi-card .h3 {
        font-size: 1.75rem;
    }
    .chart-container {
        height: 250px;
    }
}

/* Modal Fix */
.modal {
    z-index: 1060 !important;
}
.modal-backdrop {
    z-index: 1050 !important;
}
.modal-dialog {
    transform: translateZ(0);
}

/* ✅ Toastr Customization */
#toast-container > .toast {
    opacity: 1 !important;
    border-radius: 8px;
    font-size: 1rem;
    direction: rtl;
    text-align: right;
}
#toast-container > .toast.toast-success {
    background-color: #28a745 !important;
    color: #fff !important;
}
#toast-container > .toast.toast-error {
    background-color: #dc3545 !important;
    color: #fff !important;
}
✨ النتيجة
أي TempData["SuccessMessage"] → أخضر بخط أبيض واضح.

أي TempData["ErrorMessage"] → أحمر بخط أبيض واضح.

مفيش شفافية غير مرغوبة.

النص كله RTL ومحاذاته يمين.

تحب أجهزلك كمان نسخة site.js مضبوطة (بالمكان toast-top-center بدل الشمال) بحيث تكمل المنظومة كلها؟

You said:
موافق
ChatGPT said:
تمام 🎯
دي نسخة كاملة من site.js بعد التعديل علشان:

رسائل Toastr تبان في نص الشاشة فوق (Top Center).

التحكم في القائمة الجانبية زي ما هو.

باقي الأكواد زي ما كانت عندك بس متظبطة.

📌 site.js (مُحدث)
// انتظر حتى يتم تحميل الصفحة وجميع المكتبات (مثل jQuery) بشكل كامل
window.onload = function () {

    // 1. تفعيل البحث في جميع القوائم المنسدلة التي لها كلاس "searchable-select"
    if ($('.searchable-select').length > 0) {
        $('.searchable-select').select2({
            theme: "bootstrap-5",
            dir: "rtl",
            placeholder: '-- اختر --',
            width: '100%'
        });
    }

    // 2. إعدادات رسائل التأكيد (Toastr)
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-center", // ✅ وسط الشاشة فوق
        "timeOut": "5000", // 5 ثواني
        "extendedTimeOut": "2000"
    };

    // 3. الكود الخاص بالتحكم في القائمة الجانبية (Sidebar)
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const appContainer = document.getElementById('app-container');
    let activeTooltips = []; // لتخزين التلميحات الفعالة

    function initializeTooltips() {
        destroyTooltips();
        const tooltipTriggers = document.querySelectorAll('.app-sidebar [data-bs-toggle="tooltip"]');
        tooltipTriggers.forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            activeTooltips.push(tooltip);
        });
    }

    function destroyTooltips() {
        activeTooltips.forEach(tooltip => {
            if (tooltip) {
                tooltip.dispose();
            }
        });
        activeTooltips = [];
    }

    function updateTooltipState() {
        if (appContainer && window.innerWidth > 992 && appContainer.classList.contains('sidebar-collapsed')) {
            initializeTooltips();
        } else {
            destroyTooltips();
        }
    }

    if (sidebarToggleBtn && appContainer) {
        function toggleSidebar() {
            if (window.innerWidth <= 992) {
                appContainer.classList.toggle('sidebar-mobile-open');

                let backdrop = document.querySelector('.sidebar-backdrop');
                if (appContainer.classList.contains('sidebar-mobile-open')) {
                    if (!backdrop) {
                        backdrop = document.createElement('div');
                        backdrop.className = 'sidebar-backdrop';
                        document.body.appendChild(backdrop);
                        backdrop.addEventListener('click', toggleSidebar);
                    }
                } else {
                    if (backdrop) {
                        backdrop.remove();
                    }
                }
            } else {
                appContainer.classList.toggle('sidebar-collapsed');
            }
            updateTooltipState();
        }

        sidebarToggleBtn.addEventListener('click', toggleSidebar);
        updateTooltipState();
    }

    // 4. تفعيل نافذة الحذف المنبثقة (Delete Modal)
    const deleteModal = document.getElementById('deleteConfirmationModal');
    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const itemId = button.getAttribute('data-id');
            const itemName = button.getAttribute('data-name');
            const handler = button.getAttribute('data-handler');

            const modalBodyName = deleteModal.querySelector('#deleteItemName');
            const deleteForm = deleteModal.querySelector('#deleteForm');
            const deleteItemIdInput = deleteModal.querySelector('#deleteItemId');

            modalBodyName.textContent = itemName;
            deleteItemIdInput.value = itemId;
            deleteForm.action = `?handler=${handler}`;
        });
    }

    // 5. إضافة مؤشرات التحميل للأزرار
    $('form').submit(function () {
        const submitButton = $(this).find('button[type="submit"]');
        if (submitButton) {
            submitButton.prop('disabled', true);
            submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> جاري الحفظ...');
        }
    });
};
✨ النتيجة
أي رسالة نجاح أو خطأ من TempData هتظهر في Toastr فوق في النص.

شكلها أخضر/أحمر بخط أبيض (زي ما عدلنا في site.css).

الـ Sidebar والـ Modal شغالين زي ما كانوا.

تحب أجمعلك كمان نسخة كاملة من _Layout.cshtml بعد ما أضفنا تعديلات JsonSerializer بحيث تبقى الملفات التلاتة (_Layout.cshtml, site.css, site.js) متكاملين وجاهزين للنسخ واللصق؟



No file chosenNo file chosen
ChatGPT can make mistakes. Check important info.
