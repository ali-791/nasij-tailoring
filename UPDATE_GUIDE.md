# دليل تحديث النظام - TailoringShop Update Guide

## 📋 نظرة عامة

هذا الدليل يشرح كيفية تحديث النظام لدى العميل بأمان مع الحفاظ على جميع البيانات.

---

## 🏗️ نظام التحديث

### النظام يستخدم Entity Framework Migrations ✅

```
┌─────────────────────────────────────────────────────────────────┐
│                    Entity Framework Migrations                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  المميزات:                                                       │
│  ✅ تتبع تلقائي للتغييرات                                        │
│  ✅ توليد SQL تلقائياً                                           │
│  ✅ دعم التراجع (Rollback)                                       │
│  ✅ سجل كامل للتحديثات                                           │
│  ✅ أمر واحد للتطبيق: Migrate()                                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 سيناريو التحديث

```
┌─────────────────────────────────────────────────────────────────┐
│  العميل يستخدم v48                                               │
│  قاعدة البيانات تحتوي على بيانات شهر                             │
│           ↓                                                     │
│  تُطور v49 مع ميزة الطباعة                                       │
│  تُنشئ Migration جديدة                                           │
│           ↓                                                     │
│  تُسلّم النسخة الجديدة للعميل                                    │
│           ↓                                                     │
│  عند التشغيل:                                                   │
│  Database.MigrateAsync() → يُطبّق التحديثات تلقائياً ✅           │
│           ↓                                                     │
│  النظام يعمل + البيانات محفوظة ✅                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📝 كيفية إضافة تحديث جديد (للمطور)

### الخطوة 1: تعديل Entity

```csharp
// في Invoice.cs
public class Invoice : BaseEntity
{
    // ... الحقول الموجودة ...
    
    // حقول جديدة للطباعة
    public string? PrintTemplate { get; set; }
    public DateTime? PrintedAt { get; set; }
    public long? PrintedBy { get; set; }
}
```

### الخطوة 2: إنشاء Migration

في Visual Studio Package Manager Console:

```powershell
# الانتقال لمشروع Infrastructure
cd TailoringShop.Infrastructure

# إنشاء Migration جديدة
Add-Migration AddInvoicePrinting -Project TailoringShop.Infrastructure -StartupProject TailoringShop.WPF
```

أو باستخدام .NET CLI:

```bash
dotnet ef migrations add AddInvoicePrinting --project TailoringShop.Infrastructure --startup-project TailoringShop.WPF
```

### الخطوة 3: مراجعة الـ Migration

سيُنشئ ملف في `Migrations/` مثل:

```csharp
// 20260310120000_AddInvoicePrinting.cs
public partial class AddInvoicePrinting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PrintTemplate",
            table: "Invoices",
            type: "nvarchar(50)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "PrintedAt",
            table: "Invoices",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "PrintedBy",
            table: "Invoices",
            type: "bigint",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PrintTemplate", table: "Invoices");
        migrationBuilder.DropColumn(name: "PrintedAt", table: "Invoices");
        migrationBuilder.DropColumn(name: "PrintedBy", table: "Invoices");
    }
}
```

### الخطوة 4: اختبار محلياً

```powershell
Update-Database
```

### الخطوة 5: تسليم للعميل

فقط استبدل ملفات التطبيق - سيُطبّق تلقائياً!

---

## 📦 أنواع التغييرات المدعومة

| النوع | مثال | آمن؟ |
|-------|------|------|
| إضافة عمود | `AddColumn` | ✅ نعم |
| إضافة جدول | `CreateTable` | ✅ نعم |
| إضافة Index | `CreateIndex` | ✅ نعم |
| تغيير نوع (توسيع) | `nvarchar(50)` → `nvarchar(100)` | ✅ نعم |
| إضافة علاقة | `AddForeignKey` | ✅ نعم |
| حذف عمود | `DropColumn` | ⚠️ يفقد بيانات |
| حذف جدول | `DropTable` | ❌ خطير! |

---

## 💾 النسخ الاحتياطي

### النوع 1: JSON Backup (للتنقل)

```
✅ سهل الاستخدام
✅ يعمل عبر الأجهزة
✅ قابل للقراءة
⚠️ لا يحتوي Schema
```

### النوع 2: SQL Server Backup (الأفضل) ⭐

```sql
-- إنشاء نسخة احتياطية
BACKUP DATABASE TailoringShopDB 
TO DISK = 'C:\Backup\TailoringShop_2026-03-10.bak'
WITH FORMAT, COMPRESSION;

-- استعادة
RESTORE DATABASE TailoringShopDB 
FROM DISK = 'C:\Backup\TailoringShop_2026-03-10.bak'
WITH REPLACE;
```

```
✅ نسخة كاملة (Schema + Data)
✅ سريع جداً
✅ مضغوط
✅ موثوق 100%
```

---

## 🔧 إعداد Migrations لأول مرة

### إذا كان المشروع جديد:

```powershell
# إنشاء أول Migration
Add-Migration InitialCreate -Project TailoringShop.Infrastructure -StartupProject TailoringShop.WPF

# تطبيقها
Update-Database
```

### إذا كانت قاعدة البيانات موجودة (من EnsureCreated):

النظام يتعامل مع هذا تلقائياً في `DbInitializer.HandleLegacyDatabaseAsync()`

---

## ⚠️ قواعد مهمة

### ✅ افعل

1. **دائماً أنشئ Migration قبل تسليم تحديث**
2. **راجع ملف Migration قبل التطبيق**
3. **اختبر على نسخة تجريبية أولاً**
4. **أنشئ نسخة احتياطية قبل التحديث**
5. **استخدم قيم افتراضية للأعمدة الجديدة**

### ❌ لا تفعل

1. **لا تحذف أعمدة تحتوي بيانات**
2. **لا تغير أنواع البيانات بشكل جذري**
3. **لا تعدل ملفات Migration بعد تطبيقها**
4. **لا تحذف ملفات Migration**

---

## 📋 قائمة فحص قبل التسليم

```
□ تم إنشاء Migration جديدة
□ تم مراجعة كود Migration
□ تم اختبار التحديث محلياً
□ تم اختبار على قاعدة بيانات فارغة
□ تم اختبار على قاعدة بيانات تحتوي بيانات
□ تم إنشاء نسخة احتياطية عند العميل
□ تم تسليم النسخة الجديدة
□ تم التحقق من عمل النظام
```

---

## 🆘 حل المشاكل

### المشكلة: Migration لا تُطبّق

**الحل:**
1. تحقق من أن الـ Migration موجودة في مجلد `Migrations/`
2. تحقق من Connection String
3. راجع سجل الأخطاء

### المشكلة: خطأ في التطبيق

**الحل:**
1. استعد من النسخة الاحتياطية
2. راجع ملف Migration
3. أصلح الخطأ وأنشئ Migration جديدة

### المشكلة: قاعدة بيانات قديمة

**الحل:**
النظام يتعامل معها تلقائياً عبر `HandleLegacyDatabaseAsync()`

---

## 📞 الدعم

للاستفسارات:
- راجع هذا الدليل
- راجع وثائق Entity Framework: https://docs.microsoft.com/ef/core/managing-schemas/migrations/
- راجع سجلات الأخطاء في مجلد `Logs/`

---

## 🛡️ ميزات الأمان والاستقرار

### 1. سجل الأخطاء (Error Logging) ✅

```
📁 [AppFolder]/Logs/
├── error_2026-03-09.log
├── error_2026-03-10.log
└── ...
```

يتم تسجيل:
- أخطاء UI Thread
- أخطاء Background Threads
- أخطاء Tasks غير المُراقبة
- معلومات بدء/إغلاق التطبيق

### 2. النسخ الاحتياطي التلقائي ✅

```
📁 [AppFolder]/Backup/
├── Backup_2026-03-09.bak (JSON)
├── Backup_2026-03-10.bak (JSON)
└── ...
```

### 3. SQL Server Full Backup (للإنتاج) ⭐

```csharp
var backupService = new BackupService(contextFactory);

// إنشاء نسخة احتياطية كاملة
var result = await backupService.CreateSqlBackupAsync("C:\\Backup\\shop.bak");

// استعادة
var result = await backupService.RestoreSqlBackupAsync("C:\\Backup\\shop.bak");
```

⚠️ ملاحظة: SQL Backup لا يعمل مع LocalDB، يتطلب SQL Server Express/Standard.

---

## 📊 هيكل مجلدات التطبيق عند العميل

```
📁 TailoringShop/
├── 📁 Backup/           ← النسخ الاحتياطية التلقائية
│   ├── Backup_2026-03-09.bak
│   └── ...
├── 📁 Logs/             ← سجلات الأخطاء
│   ├── error_2026-03-09.log
│   └── ...
├── 📄 TailoringShop.exe
├── 📄 appsettings.json
└── ...
```
