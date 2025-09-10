// --- FILENAME: Program.cs ---
// هذا هو الملف الرئيسي الذي يقوم بضبط وإعداد وتشغيل التطبيق.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Keswa.Data;
using Keswa.Models;
using Keswa.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد الاتصال بقاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//------------------------------------------------------------------------------------------------
// 2. إضافة خدمات Identity وتخصيصها لاستخدام كلاس ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // يمكنك تفعيلها لاحقاً لإرسال إيميل تأكيد
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); // لإضافة واجهات المستخدم الجاهزة من Identity

// 3. إضافة Razor Pages
builder.Services.AddRazorPages();

// 4. تسجيل خدمة تهيئة قاعدة البيانات (لإنشاء الصلاحيات والمستخدم الافتراضي)
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// استدعاء دالة تهيئة قاعدة البيانات
await SeedDatabaseAsync(app);

app.UseRouting();

app.UseAuthentication(); // تفعيل المصادقة
app.UseAuthorization(); // تفعيل الصلاحيات

app.MapRazorPages();

app.Run();


// دالة مساعدة لاستدعاء خدمة تهيئة قاعدة البيانات
async Task SeedDatabaseAsync(IApplicationBuilder app)
{
    using (var scope = app.ApplicationServices.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initialize();
    }
}
