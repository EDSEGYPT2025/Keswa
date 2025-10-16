# نظام كسوة

تطبيق ويب لإدارة عمليات المحلات وسلاسل التأجير (فساتين – منتجات) بطريقة ذكية ومنظمة.

---

## 💡 نظرة سريعة

نظام "كسوة" هو **نظام ERP مبسط** مبني باستخدام **ASP.NET Core Razor Pages** و**Entity Framework Code First** لإدارة:

* المنتجات والمخزون
* الموردين والعملاء
* عمليات البيع والتأجير
* الفواتير والتقارير

---

## 🚀 المميزات

* إدارة كاملة للمنتجات مع الصور والتصنيفات.
* نظام بيع وتأجير (POS) سهل الاستخدام.
* تتبع حركات المخزون (صرف، استلام، تحويل، رصيد افتتاحي).
* صلاحيات مستخدمين متعددة المستويات.
* تقارير جاهزة وقابلة للطباعة (مبيعات – مخزون – موردين).

---

## 🛠️ التقنيات المستخدمة

* **Backend:** ASP.NET Core 9 (Razor Pages)
* **Database:** SQL Server + Entity Framework Core
* **Frontend:** HTML5, CSS3, Bootstrap
* **Language:** C#

---

## ⚙️ خطوات التشغيل السريعة

```bash
git clone https://github.com/<your-username>/kswha.git
cd kswha
dotnet restore
dotnet ef database update
dotnet run
```

ثم افتح المتصفح على:
👉 `https://localhost:5001`

---

## 📂 بنية المشروع

```
Kswa.Web       → واجهة المستخدم (Razor Pages)
Kswa.Data      → قاعدة البيانات وDbContext
Kswa.Core      → الكيانات والنماذج
Kswa.Services  → منطق الأعمال والخدمات
```

---

## 📞 تواصل معي

**Ahmed**
📧 [edsegypt@outlook.com](mailto:edsegypt@outlook.com)

