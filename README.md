# نظام إدارة متجر الخياطة
# Tailoring Shop Management System

## 📋 المتطلبات

قبل تشغيل المشروع، تأكد من توفر التالي:

1. **Visual Studio 2022** (الإصدار 17.0 أو أحدث)
   - تحميل من: https://visualstudio.microsoft.com/

2. **.NET 8.0 SDK**
   - تحميل من: https://dotnet.microsoft.com/download/dotnet/8.0

3. **SQL Server** (أي من الخيارات التالية):
   - SQL Server 2019 أو أحدث
   - SQL Server Express (مجاني)
   - SQL Server LocalDB (يأتي مع Visual Studio)

---

## 🚀 خطوات التشغيل

### الخطوة 1: فتح المشروع

1. فك ضغط الملف `TailoringShop.zip`
2. افتح Visual Studio 2022
3. اختر `Open a project or solution`
4. انتقل إلى مجلد المشروع واختر `TailoringShop.sln`

### الخطوة 2: استعادة الحزم

عند فتح المشروع لأول مرة، سيقوم Visual Studio تلقائياً باستعادة حزم NuGet.

أو يمكنك تشغيل الأمر يدوياً:
```
dotnet restore
```

### الخطوة 3: إعداد قاعدة البيانات

#### الخيار أ: استخدام LocalDB (الأسهل)
- المشروع مُعد مسبقاً لاستخدام LocalDB
- ستُنشأ قاعدة البيانات تلقائياً عند أول تشغيل

#### الخيار ب: استخدام SQL Server
1. افتح ملف `TailoringShopDB_SQLServer.sql` في SQL Server Management Studio
2. نفذ الملف لإنشاء قاعدة البيانات والجداول
3. عدّل Connection String في `App.xaml.cs`:

```csharp
options.UseSqlServer(
    "Server=YOUR_SERVER_NAME;Database=TailoringShopDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
```

### الخطوة 4: تشغيل المشروع

1. اضغط `F5` أو انقر على زر `Start` الأخضر
2. انتظر حتى يكتمل البناء
3. ستظهر نافذة تسجيل الدخول

### الخطوة 5: تسجيل الدخول

استخدم بيانات المستخدم الافتراضي:
- **اسم المستخدم:** `admin`
- **كلمة المرور:** `admin123`

---

## 📁 هيكل المشروع

```
TailoringShop/
├── TailoringShop.Core/           # المنطق الأساسي
│   ├── Entities/                 # الكيانات (Models)
│   ├── Enums/                    # التعدادات
│   └── Interfaces/               # الواجهات
│
├── TailoringShop.Infrastructure/ # البنية التحتية
│   ├── Data/                     # DbContext
│   ├── Repositories/             # Repository Pattern
│   └── Services/                 # الخدمات
│
├── TailoringShop.WPF/            # واجهة المستخدم
│   ├── Views/                    # الصفحات (XAML)
│   │   ├── Windows/              # النوافذ الرئيسية
│   │   ├── Pages/                # الصفحات
│   │   ├── Controls/             # عناصر مخصصة
│   │   └── Dialogs/              # نوافذ الحوار
│   ├── ViewModels/               # MVVM ViewModels
│   ├── Resources/                # الموارد
│   │   └── Styles/               # الأنماط
│   ├── Converters/               # محولات البيانات
│   └── Helpers/                  # مساعدات
│
└── TailoringShopDB_SQLServer.sql # ملف قاعدة البيانات
```

---

## ⚙️ إعدادات Connection String

### LocalDB (الافتراضي):
```
Server=(localdb)\mssqllocaldb;Database=TailoringShopDB;Trusted_Connection=True;MultipleActiveResultSets=true
```

### SQL Server Express:
```
Server=.\SQLEXPRESS;Database=TailoringShopDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

### SQL Server (بكلمة مرور):
```
Server=YOUR_SERVER;Database=TailoringShopDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True
```

---

## 🔧 استكشاف الأخطاء

### خطأ: "Cannot connect to database"
- تأكد من أن SQL Server قيد التشغيل
- تحقق من Connection String
- تأكد من صلاحيات الوصول لقاعدة البيانات

### خطأ: "Package not found"
```
dotnet restore
```

### خطأ: ".NET 8 not found"
- حمّل وثبّت .NET 8 SDK من الموقع الرسمي

### خطأ عند تسجيل الدخول
- تأكد من تنفيذ ملف SQL لإنشاء البيانات الافتراضية
- المستخدم الافتراضي: admin / admin123

---

## 📱 الميزات

- ✅ إدارة المستخدمين والصلاحيات (RBAC)
- ✅ إدارة القاصات المتعددة
- ✅ طلبات الفصال (خياطة)
- ✅ الفواتير (بيع/شراء)
- ✅ إدارة المنتجات والمخزون
- ✅ إدارة العملاء
- ✅ تسجيل المصروفات
- ✅ لوحة تحكم ذكية
- ✅ تقارير متقدمة
- ✅ دعم العملات المتعددة

---

## 🎨 الألوان المستخدمة

| العنصر | اللون |
|--------|-------|
| Primary | #667eea → #764ba2 |
| Accent | #f5a623 |
| Success | #00b894 |
| Danger | #e74c3c |
| Background | #f5f6fa |

---

## 📞 الدعم

إذا واجهت أي مشاكل، تأكد من:
1. تثبيت جميع المتطلبات
2. إعداد قاعدة البيانات بشكل صحيح
3. تشغيل Visual Studio كمسؤول (إذا لزم الأمر)

---

## 📝 ملاحظات للتطوير

- المشروع يستخدم **MVVM Pattern**
- يستخدم **Entity Framework Core** للتعامل مع قاعدة البيانات
- يستخدم **CommunityToolkit.Mvvm** للـ ViewModels
- يستخدم **Dependency Injection** لإدارة الخدمات

---

الإصدار: 1.0.0
