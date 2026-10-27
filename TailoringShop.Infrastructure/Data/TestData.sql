-- =====================================================
-- سكريبت البيانات التجريبية - نظام إدارة محل الخياطة
-- Test/Dummy Data Script
-- =====================================================

USE TailoringShopDB;
GO

-- =====================================================
-- 1. العملاء (Customers)
-- =====================================================
INSERT INTO Customers (FullName, Phone, IsActive, CreatedAt, UpdatedAt) VALUES
(N'أحمد محمد علي', '07701234567', 1, DATEADD(DAY, -60, GETDATE()), GETDATE()),
(N'محمد حسين جاسم', '07702345678', 1, DATEADD(DAY, -55, GETDATE()), GETDATE()),
(N'علي كريم عبدالله', '07703456789', 1, DATEADD(DAY, -50, GETDATE()), GETDATE()),
(N'حسن عبدالرحمن', '07704567890', 1, DATEADD(DAY, -45, GETDATE()), GETDATE()),
(N'عمر فاضل محمود', '07705678901', 1, DATEADD(DAY, -40, GETDATE()), GETDATE()),
(N'خالد سامي نجم', '07706789012', 1, DATEADD(DAY, -35, GETDATE()), GETDATE()),
(N'يوسف طارق حسين', '07707890123', 1, DATEADD(DAY, -30, GETDATE()), GETDATE()),
(N'سعد جمال عزيز', '07708901234', 1, DATEADD(DAY, -25, GETDATE()), GETDATE()),
(N'فاضل كامل رشيد', '07709012345', 1, DATEADD(DAY, -20, GETDATE()), GETDATE()),
(N'ماجد علاء الدين', '07700123456', 1, DATEADD(DAY, -15, GETDATE()), GETDATE()),
(N'رائد صباح نوري', '07711234567', 1, DATEADD(DAY, -10, GETDATE()), GETDATE()),
(N'باسم عادل سلمان', '07712345678', 1, DATEADD(DAY, -5, GETDATE()), GETDATE());

-- =====================================================
-- 2. المنتجات (Products)
-- =====================================================
DECLARE @CategoryId1 BIGINT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'أقمشة');
DECLARE @CategoryId2 BIGINT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'إكسسوارات');
DECLARE @CategoryId3 BIGINT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'عطور');
DECLARE @CategoryId4 BIGINT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'ملابس أطفال');
DECLARE @CategoryId5 BIGINT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'ملابس جاهزة');
DECLARE @CurrencyId BIGINT = (SELECT TOP 1 Id FROM Currencies WHERE IsDefault = 1);

-- If categories don't exist, use first available
IF @CategoryId1 IS NULL SET @CategoryId1 = (SELECT TOP 1 Id FROM Categories);
IF @CategoryId2 IS NULL SET @CategoryId2 = @CategoryId1;
IF @CategoryId3 IS NULL SET @CategoryId3 = @CategoryId1;
IF @CategoryId4 IS NULL SET @CategoryId4 = @CategoryId1;
IF @CategoryId5 IS NULL SET @CategoryId5 = @CategoryId1;

INSERT INTO Products (Name, Barcode, CategoryId, CurrencyId, CostPrice, SellingPrice, MinStockLevel, Unit, IsActive, CreatedAt, UpdatedAt) VALUES
-- أقمشة
(N'قماش صيفي قطني أبيض', 'FAB001', @CategoryId1, @CurrencyId, 15000, 25000, 10, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'قماش صيفي قطني بيج', 'FAB002', @CategoryId1, @CurrencyId, 15000, 25000, 10, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'قماش شتوي صوف رمادي', 'FAB003', @CategoryId1, @CurrencyId, 25000, 40000, 5, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'قماش شتوي صوف كحلي', 'FAB004', @CategoryId1, @CurrencyId, 25000, 40000, 5, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'قماش كتان أزرق فاتح', 'FAB005', @CategoryId1, @CurrencyId, 20000, 32000, 8, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'قماش حرير أسود', 'FAB006', @CategoryId1, @CurrencyId, 35000, 55000, 3, N'متر', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
-- إكسسوارات
(N'أزرار ذهبية (12 قطعة)', 'ACC001', @CategoryId2, @CurrencyId, 3000, 5000, 20, N'علبة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'أزرار فضية (12 قطعة)', 'ACC002', @CategoryId2, @CurrencyId, 3000, 5000, 20, N'علبة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'خيط تطريز ذهبي', 'ACC003', @CategoryId2, @CurrencyId, 2000, 3500, 15, N'بكرة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'سحاب 20 سم أبيض', 'ACC004', @CategoryId2, @CurrencyId, 500, 1000, 50, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
-- عطور
(N'عطر عود كمبودي 50 مل', 'PER001', @CategoryId3, @CurrencyId, 45000, 75000, 5, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'عطر مسك أبيض 30 مل', 'PER002', @CategoryId3, @CurrencyId, 20000, 35000, 10, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'بخور عود هندي 50 غم', 'PER003', @CategoryId3, @CurrencyId, 15000, 25000, 8, N'علبة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
-- ملابس جاهزة
(N'دشداشة جاهزة بيضاء M', 'RDY001', @CategoryId5, @CurrencyId, 25000, 45000, 5, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'دشداشة جاهزة بيضاء L', 'RDY002', @CategoryId5, @CurrencyId, 25000, 45000, 5, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'دشداشة جاهزة بيضاء XL', 'RDY003', @CategoryId5, @CurrencyId, 27000, 48000, 5, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'عباية رجالية سوداء', 'RDY004', @CategoryId5, @CurrencyId, 35000, 60000, 3, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
-- ملابس أطفال
(N'دشداشة أطفال بيضاء (4-6)', 'KID001', @CategoryId4, @CurrencyId, 12000, 22000, 8, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE()),
(N'دشداشة أطفال بيضاء (7-10)', 'KID002', @CategoryId4, @CurrencyId, 15000, 25000, 8, N'قطعة', 1, DATEADD(DAY, -90, GETDATE()), GETDATE());

-- =====================================================
-- 3. المخزون (Inventory)
-- =====================================================
DECLARE @StoreId BIGINT = (SELECT TOP 1 Id FROM Stores WHERE IsActive = 1);

INSERT INTO Inventories (ProductId, StoreId, Quantity, CreatedAt, UpdatedAt)
SELECT Id, @StoreId, 
    CASE 
        WHEN Barcode LIKE 'FAB%' THEN ABS(CHECKSUM(NEWID())) % 50 + 20  -- 20-70 متر
        WHEN Barcode LIKE 'ACC%' THEN ABS(CHECKSUM(NEWID())) % 100 + 30 -- 30-130 قطعة
        WHEN Barcode LIKE 'PER%' THEN ABS(CHECKSUM(NEWID())) % 20 + 5   -- 5-25 قطعة
        ELSE ABS(CHECKSUM(NEWID())) % 15 + 5                            -- 5-20 قطعة
    END,
    GETDATE(), GETDATE()
FROM Products WHERE IsActive = 1 AND Barcode IS NOT NULL;

-- =====================================================
-- 4. طلبات الفصال (Tailoring Orders)
-- =====================================================

-- طلب 1 - مكتمل ومستلم
INSERT INTO TailoringOrders (OrderNumber, CustomerId, CurrencyId, OrderDate, DeliveryDate, Status, TotalAmount, DepositAmount, RemainingAmount, Notes, CreatedAt, UpdatedAt)
VALUES (N'TO-20260101-001', 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()), 
    @CurrencyId,
    DATEADD(DAY, -45, GETDATE()),
    DATEADD(DAY, -38, GETDATE()),
    4, -- Delivered
    85000, 85000, 0,
    N'تم التسليم بنجاح',
    DATEADD(DAY, -45, GETDATE()), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260101-001'),
    N'دشداشة صيفية', N'دشداشة قطنية بيضاء مع تطريز ذهبي على الصدر', 2, 35000, 70000, GETDATE(), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260101-001'),
    N'سروال', N'سروال قطني أبيض', 1, 15000, 15000, GETDATE(), GETDATE());

-- طلب 2 - قيد الخياطة
INSERT INTO TailoringOrders (OrderNumber, CustomerId, CurrencyId, OrderDate, DeliveryDate, Status, TotalAmount, DepositAmount, RemainingAmount, Notes, CreatedAt, UpdatedAt)
VALUES (N'TO-20260115-002', 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()), 
    @CurrencyId,
    DATEADD(DAY, -20, GETDATE()),
    DATEADD(DAY, 5, GETDATE()),
    2, -- InProgress
    120000, 60000, 60000,
    N'يفضل العميل الياقة الكورية',
    DATEADD(DAY, -20, GETDATE()), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260115-002'),
    N'دشداشة شتوية', N'دشداشة صوف رمادي - ياقة كورية + جيوب داخلية', 3, 40000, 120000, GETDATE(), GETDATE());

-- طلب 3 - جديد
INSERT INTO TailoringOrders (OrderNumber, CustomerId, CurrencyId, OrderDate, DeliveryDate, Status, TotalAmount, DepositAmount, RemainingAmount, Notes, CreatedAt, UpdatedAt)
VALUES (N'TO-20260210-003', 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()), 
    @CurrencyId,
    DATEADD(DAY, -5, GETDATE()),
    DATEADD(DAY, 10, GETDATE()),
    1, -- Pending
    95000, 30000, 65000,
    NULL,
    DATEADD(DAY, -5, GETDATE()), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260210-003'),
    N'دشداشة صيفية', N'دشداشة كتان أزرق فاتح', 2, 38000, 76000, GETDATE(), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260210-003'),
    N'غترة', N'غترة بيضاء مع تطريز الاسم', 1, 19000, 19000, GETDATE(), GETDATE());

-- طلب 4 - جاهز للاستلام
INSERT INTO TailoringOrders (OrderNumber, CustomerId, CurrencyId, OrderDate, DeliveryDate, Status, TotalAmount, DepositAmount, RemainingAmount, Notes, CreatedAt, UpdatedAt)
VALUES (N'TO-20260201-004', 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()), 
    @CurrencyId,
    DATEADD(DAY, -15, GETDATE()),
    DATEADD(DAY, -2, GETDATE()),
    3, -- Ready
    55000, 55000, 0,
    N'جاهز - تم الاتصال بالعميل',
    DATEADD(DAY, -15, GETDATE()), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260201-004'),
    N'دشداشة', N'دشداشة قطنية بيج', 1, 35000, 35000, GETDATE(), GETDATE());

INSERT INTO TailoringOrderItems (OrderId, ItemName, Description, Quantity, UnitPrice, Total, CreatedAt, UpdatedAt)
VALUES ((SELECT TOP 1 Id FROM TailoringOrders WHERE OrderNumber = N'TO-20260201-004'),
    N'سروال', N'سروال بيج', 1, 20000, 20000, GETDATE(), GETDATE());

-- طلب 5 - ملغي
INSERT INTO TailoringOrders (OrderNumber, CustomerId, CurrencyId, OrderDate, DeliveryDate, Status, TotalAmount, DepositAmount, RemainingAmount, Notes, CreatedAt, UpdatedAt)
VALUES (N'TO-20260120-005', 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()), 
    @CurrencyId,
    DATEADD(DAY, -30, GETDATE()),
    DATEADD(DAY, -20, GETDATE()),
    5, -- Cancelled
    45000, 0, 0,
    N'ملغي - العميل غير رأيه',
    DATEADD(DAY, -30, GETDATE()), GETDATE());

-- =====================================================
-- 5. فواتير البيع (Sales Invoices)
-- =====================================================
DECLARE @CashboxId BIGINT = (SELECT TOP 1 Id FROM Cashboxes WHERE IsActive = 1);

-- فاتورة 1
INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'INV-20260101-001', 0, 1, 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()),
    @CurrencyId, @CashboxId, @StoreId,
    DATEADD(DAY, -40, GETDATE()),
    145000, 5000, 140000, 140000, 0, 2,
    NULL, DATEADD(DAY, -40, GETDATE()), GETDATE());

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260101-001'),
    Id, 3, 25000, 15000, 0, 75000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'FAB001';

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260101-001'),
    Id, 2, 35000, 20000, 0, 70000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'PER002';

-- فاتورة 2
INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'INV-20260110-002', 0, 1, 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()),
    @CurrencyId, @CashboxId, @StoreId,
    DATEADD(DAY, -30, GETDATE()),
    95000, 0, 95000, 95000, 0, 2,
    NULL, DATEADD(DAY, -30, GETDATE()), GETDATE());

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260110-002'),
    Id, 2, 45000, 25000, 0, 90000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'RDY001';

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260110-002'),
    Id, 1, 5000, 3000, 0, 5000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'ACC001';

-- فاتورة 3 - آجل (غير مدفوعة بالكامل)
INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'INV-20260205-003', 0, 0, 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()),
    @CurrencyId, @CashboxId, @StoreId,
    DATEADD(DAY, -10, GETDATE()),
    175000, 0, 175000, 100000, 75000, 1,
    N'دفع جزئي - المتبقي بعد أسبوع',
    DATEADD(DAY, -10, GETDATE()), GETDATE());

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260205-003'),
    Id, 1, 75000, 45000, 0, 75000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'PER001';

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260205-003'),
    Id, 2, 25000, 15000, 0, 50000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'PER003';

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260205-003'),
    Id, 2, 25000, 15000, 0, 50000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'FAB002';

-- فاتورة 4 - اليوم
INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'INV-20260218-004', 0, 1, 
    (SELECT TOP 1 Id FROM Customers ORDER BY NEWID()),
    @CurrencyId, @CashboxId, @StoreId,
    GETDATE(),
    67000, 2000, 65000, 65000, 0, 2,
    NULL, GETDATE(), GETDATE());

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260218-004'),
    Id, 1, 22000, 12000, 0, 22000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'KID001';

INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice, DiscountAmount, Total, CreatedAt, UpdatedAt)
SELECT TOP 1 
    (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = N'INV-20260218-004'),
    Id, 1, 45000, 25000, 0, 45000, GETDATE(), GETDATE()
FROM Products WHERE Barcode = 'RDY002';

-- =====================================================
-- 6. فواتير الشراء (Purchase Invoices)
-- =====================================================
INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, SupplierName, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'PUR-20260101-001', 1, 1, 
    NULL, N'مورد الأقمشة الرئيسي',
    @CurrencyId, @CashboxId, @StoreId,
    DATEADD(DAY, -60, GETDATE()),
    500000, 0, 500000, 500000, 0, 2,
    N'شراء أقمشة من المورد الرئيسي',
    DATEADD(DAY, -60, GETDATE()), GETDATE());

INSERT INTO Invoices (InvoiceNumber, Type, Status, CustomerId, SupplierName, CurrencyId, CashboxId, StoreId, InvoiceDate, Subtotal, DiscountAmount, Total, PaidAmount, RemainingAmount, PaymentStatus, Notes, CreatedAt, UpdatedAt)
VALUES (N'PUR-20260115-002', 1, 1, 
    NULL, N'مورد العطور',
    @CurrencyId, @CashboxId, @StoreId,
    DATEADD(DAY, -35, GETDATE()),
    250000, 0, 250000, 250000, 0, 2,
    N'شراء عطور ومستلزمات',
    DATEADD(DAY, -35, GETDATE()), GETDATE());

-- =====================================================
-- 7. المصروفات (Expenses)
-- =====================================================
DECLARE @ExpCat1 BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name LIKE N'%إيجار%');
DECLARE @ExpCat2 BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name LIKE N'%كهرباء%');
DECLARE @ExpCat3 BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name LIKE N'%رواتب%');
DECLARE @ExpCat4 BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name LIKE N'%صيانة%');
DECLARE @ExpCat5 BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name LIKE N'%أخرى%');

-- If categories don't exist, use first available
DECLARE @DefaultExpCat BIGINT = (SELECT TOP 1 Id FROM ExpenseCategories);
IF @ExpCat1 IS NULL SET @ExpCat1 = @DefaultExpCat;
IF @ExpCat2 IS NULL SET @ExpCat2 = @DefaultExpCat;
IF @ExpCat3 IS NULL SET @ExpCat3 = @DefaultExpCat;
IF @ExpCat4 IS NULL SET @ExpCat4 = @DefaultExpCat;
IF @ExpCat5 IS NULL SET @ExpCat5 = @DefaultExpCat;

INSERT INTO Expenses (ExpenseNumber, CategoryId, CashboxId, CurrencyId, Amount, ExpenseDate, Description, ReferenceNumber, CreatedAt, UpdatedAt) VALUES
(N'EXP-20260101-001', @ExpCat1, @CashboxId, @CurrencyId, 500000, DATEADD(DAY, -45, GETDATE()), N'إيجار المحل - شهر يناير', N'REC-001', DATEADD(DAY, -45, GETDATE()), GETDATE()),
(N'EXP-20260105-002', @ExpCat2, @CashboxId, @CurrencyId, 75000, DATEADD(DAY, -40, GETDATE()), N'فاتورة الكهرباء - يناير', N'ELEC-001', DATEADD(DAY, -40, GETDATE()), GETDATE()),
(N'EXP-20260110-003', @ExpCat3, @CashboxId, @CurrencyId, 350000, DATEADD(DAY, -35, GETDATE()), N'راتب الموظف أحمد', NULL, DATEADD(DAY, -35, GETDATE()), GETDATE()),
(N'EXP-20260110-004', @ExpCat3, @CashboxId, @CurrencyId, 300000, DATEADD(DAY, -35, GETDATE()), N'راتب الموظف محمد', NULL, DATEADD(DAY, -35, GETDATE()), GETDATE()),
(N'EXP-20260115-005', @ExpCat4, @CashboxId, @CurrencyId, 50000, DATEADD(DAY, -30, GETDATE()), N'صيانة ماكينة الخياطة', N'MAINT-001', DATEADD(DAY, -30, GETDATE()), GETDATE()),
(N'EXP-20260120-006', @ExpCat5, @CashboxId, @CurrencyId, 25000, DATEADD(DAY, -25, GETDATE()), N'مستلزمات تنظيف', NULL, DATEADD(DAY, -25, GETDATE()), GETDATE()),
(N'EXP-20260201-007', @ExpCat1, @CashboxId, @CurrencyId, 500000, DATEADD(DAY, -15, GETDATE()), N'إيجار المحل - شهر فبراير', N'REC-002', DATEADD(DAY, -15, GETDATE()), GETDATE()),
(N'EXP-20260205-008', @ExpCat2, @CashboxId, @CurrencyId, 80000, DATEADD(DAY, -10, GETDATE()), N'فاتورة الكهرباء - فبراير', N'ELEC-002', DATEADD(DAY, -10, GETDATE()), GETDATE()),
(N'EXP-20260210-009', @ExpCat3, @CashboxId, @CurrencyId, 350000, DATEADD(DAY, -5, GETDATE()), N'راتب الموظف أحمد - فبراير', NULL, DATEADD(DAY, -5, GETDATE()), GETDATE()),
(N'EXP-20260210-010', @ExpCat3, @CashboxId, @CurrencyId, 300000, DATEADD(DAY, -5, GETDATE()), N'راتب الموظف محمد - فبراير', NULL, DATEADD(DAY, -5, GETDATE()), GETDATE()),
(N'EXP-20260215-011', @ExpCat5, @CashboxId, @CurrencyId, 15000, DATEADD(DAY, -3, GETDATE()), N'شراء أكياس تغليف', NULL, DATEADD(DAY, -3, GETDATE()), GETDATE());

-- =====================================================
-- 8. تحديث رصيد الصندوق
-- =====================================================
-- حساب الإيرادات
DECLARE @TotalSales DECIMAL(18,2) = (SELECT ISNULL(SUM(PaidAmount), 0) FROM Invoices WHERE Type = 0 AND Status != 2);
DECLARE @TotalTailoring DECIMAL(18,2) = (SELECT ISNULL(SUM(DepositAmount), 0) FROM TailoringOrders WHERE Status != 5);
DECLARE @TotalExpenses DECIMAL(18,2) = (SELECT ISNULL(SUM(Amount), 0) FROM Expenses);
DECLARE @TotalPurchases DECIMAL(18,2) = (SELECT ISNULL(SUM(PaidAmount), 0) FROM Invoices WHERE Type = 1);

-- تحديث رصيد الصندوق
UPDATE Cashboxes 
SET CurrentBalance = 1000000 + @TotalSales + @TotalTailoring - @TotalExpenses - @TotalPurchases,
    UpdatedAt = GETDATE()
WHERE Id = @CashboxId;

-- =====================================================
-- عرض ملخص البيانات المضافة
-- =====================================================
SELECT 'Summary of Added Data' AS [Report];
SELECT 'Customers' AS [Table], COUNT(*) AS [Count] FROM Customers
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Inventory Records', COUNT(*) FROM Inventories
UNION ALL
SELECT 'Tailoring Orders', COUNT(*) FROM TailoringOrders
UNION ALL
SELECT 'Tailoring Items', COUNT(*) FROM TailoringOrderItems
UNION ALL
SELECT 'Sales Invoices', COUNT(*) FROM Invoices WHERE Type = 0
UNION ALL
SELECT 'Purchase Invoices', COUNT(*) FROM Invoices WHERE Type = 1
UNION ALL
SELECT 'Invoice Items', COUNT(*) FROM InvoiceItems
UNION ALL
SELECT 'Expenses', COUNT(*) FROM Expenses;

SELECT 'Cashbox Balance' AS [Info], FORMAT(CurrentBalance, 'N0') + ' IQD' AS [Value] FROM Cashboxes WHERE Id = @CashboxId;

PRINT N'✅ تم إضافة البيانات التجريبية بنجاح!';
GO
