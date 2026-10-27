# Entity Framework Migrations

## 🚀 إعداد Migrations لأول مرة

### الخطوة 1: فتح Package Manager Console

في Visual Studio:
```
Tools → NuGet Package Manager → Package Manager Console
```

### الخطوة 2: إنشاء أول Migration

```powershell
# تأكد من أن Default Project هو TailoringShop.Infrastructure
Add-Migration InitialCreate -Project TailoringShop.Infrastructure -StartupProject TailoringShop.WPF
```

### الخطوة 3: تطبيق Migration

```powershell
Update-Database -Project TailoringShop.Infrastructure -StartupProject TailoringShop.WPF
```

---

## 📝 إضافة Migration جديدة

عند إضافة حقول أو جداول جديدة:

```powershell
# مثال: إضافة ميزة الطباعة
Add-Migration AddInvoicePrinting -Project TailoringShop.Infrastructure -StartupProject TailoringShop.WPF

# مراجعة الملف المُولَّد ثم تطبيق
Update-Database
```

---

## 🔄 أوامر مفيدة

```powershell
# عرض Migrations المطبقة
Get-Migration

# إزالة آخر Migration (قبل التطبيق)
Remove-Migration

# التراجع لـ Migration معينة
Update-Database -Migration PreviousMigrationName

# إنشاء SQL Script بدون تطبيق
Script-Migration
```

---

## ⚠️ ملاحظات مهمة

1. **لا تحذف ملفات Migration بعد تطبيقها**
2. **لا تعدل ملفات Migration بعد تطبيقها**
3. **دائماً راجع الملف المُولَّد قبل التطبيق**
4. **أنشئ نسخة احتياطية قبل تطبيق Migration جديدة**

---

## 📁 هيكل الملفات

```
TailoringShop.Infrastructure/
├── Migrations/
│   ├── 20260309000000_InitialCreate.cs
│   ├── 20260309000000_InitialCreate.Designer.cs
│   ├── 20260310120000_AddInvoicePrinting.cs
│   ├── 20260310120000_AddInvoicePrinting.Designer.cs
│   └── AppDbContextModelSnapshot.cs
└── ...
```
