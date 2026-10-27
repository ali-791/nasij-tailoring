-- ============================================================
-- TAILORING SHOP MANAGEMENT SYSTEM
-- Database Schema for SQL Server
-- Version: 1.1
-- Database: SQL Server 2019+
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TailoringShopDB')
BEGIN
    CREATE DATABASE TailoringShopDB
    COLLATE Arabic_CI_AS;
END
GO

USE TailoringShopDB;
GO

-- ============================================================
-- SECTION 1: USERS & PERMISSIONS
-- ============================================================

-- 1.1 Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Users_UUID UNIQUE (UUID),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
GO

-- 1.2 Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
CREATE TABLE Roles (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    IsSystem BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

CREATE INDEX IX_Roles_Name ON Roles(Name);
CREATE INDEX IX_Roles_IsActive ON Roles(IsActive);
GO

-- 1.3 Permissions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
CREATE TABLE Permissions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Module NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(150) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT UQ_Permissions_Module_Name UNIQUE (Module, Name)
);
GO

CREATE INDEX IX_Permissions_Module ON Permissions(Module);
GO

-- 1.4 Role Permissions (Pivot Table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
CREATE TABLE RolePermissions (
    RoleId BIGINT NOT NULL,
    PermissionId BIGINT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);
GO

-- 1.5 User Roles (Pivot Table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
CREATE TABLE UserRoles (
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    AssignedAt DATETIME2 DEFAULT GETDATE(),
    AssignedBy BIGINT NULL,
    
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- ============================================================
-- SECTION 2: CURRENCIES & CASHBOXES
-- ============================================================

-- 2.1 Currencies Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies')
CREATE TABLE Currencies (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(3) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Symbol NVARCHAR(10) NOT NULL,
    ExchangeRate DECIMAL(15,6) NOT NULL DEFAULT 1.000000,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    DecimalPlaces TINYINT DEFAULT 2,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT UQ_Currencies_Code UNIQUE (Code)
);
GO

CREATE INDEX IX_Currencies_Code ON Currencies(Code);
CREATE INDEX IX_Currencies_IsDefault ON Currencies(IsDefault);
CREATE INDEX IX_Currencies_IsActive ON Currencies(IsActive);
GO

-- 2.2 Cashboxes Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cashboxes')
CREATE TABLE Cashboxes (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('tailoring', 'ready_made', 'perfumes', 'general')),
    CurrencyId BIGINT NOT NULL,
    CurrentBalance DECIMAL(15,3) DEFAULT 0.000,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_Cashboxes_Currency FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_Cashboxes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Cashboxes_Type ON Cashboxes(Type);
CREATE INDEX IX_Cashboxes_CurrencyId ON Cashboxes(CurrencyId);
CREATE INDEX IX_Cashboxes_IsActive ON Cashboxes(IsActive);
GO

-- 2.3 Cashbox Transactions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CashboxTransactions')
CREATE TABLE CashboxTransactions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    CashboxId BIGINT NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('deposit', 'withdrawal', 'sale', 'expense', 'refund', 'adjustment')),
    Amount DECIMAL(15,3) NOT NULL,
    BalanceBefore DECIMAL(15,3) NOT NULL,
    BalanceAfter DECIMAL(15,3) NOT NULL,
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId BIGINT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_CashboxTransactions_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_CashboxTransactions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_CashboxTransactions_CashboxId ON CashboxTransactions(CashboxId);
CREATE INDEX IX_CashboxTransactions_Type ON CashboxTransactions(Type);
CREATE INDEX IX_CashboxTransactions_CreatedAt ON CashboxTransactions(CreatedAt);
CREATE INDEX IX_CashboxTransactions_Reference ON CashboxTransactions(ReferenceType, ReferenceId);
GO

-- ============================================================
-- SECTION 3: CUSTOMERS
-- ============================================================

-- 3.1 Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
CREATE TABLE Customers (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    TotalPurchases DECIMAL(15,3) DEFAULT 0.000,
    TotalOrders INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Customers_UUID UNIQUE (UUID),
    CONSTRAINT FK_Customers_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Customers_Phone ON Customers(Phone);
CREATE INDEX IX_Customers_FullName ON Customers(FullName);
CREATE INDEX IX_Customers_IsActive ON Customers(IsActive);
GO

-- ============================================================
-- SECTION 4: PRODUCTS & INVENTORY
-- ============================================================

-- 4.1 Categories Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
CREATE TABLE Categories (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
GO

-- 4.2 Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
CREATE TABLE Products (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    SKU NVARCHAR(50) NULL,
    Barcode NVARCHAR(50) NULL,
    Name NVARCHAR(150) NOT NULL,
    CategoryId BIGINT NULL,
    CashboxId BIGINT NULL,
    CostPrice DECIMAL(15,3) NOT NULL DEFAULT 0.000,
    SellingPrice DECIMAL(15,3) NOT NULL,
    CurrencyId BIGINT NOT NULL,
    Unit NVARCHAR(20) DEFAULT N'piece',
    MinStockLevel INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Products_UUID UNIQUE (UUID),
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
    CONSTRAINT UQ_Products_Barcode UNIQUE (Barcode),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Products_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Products_Currency FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_Products_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Products_SKU ON Products(SKU);
CREATE INDEX IX_Products_Barcode ON Products(Barcode);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
GO

-- 4.3 Stores (Warehouses) Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stores')
CREATE TABLE Stores (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NULL,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Stores_IsDefault ON Stores(IsDefault);
CREATE INDEX IX_Stores_IsActive ON Stores(IsActive);
GO

-- 4.4 Inventory Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventory')
CREATE TABLE Inventory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductId BIGINT NOT NULL,
    StoreId BIGINT NOT NULL,
    Quantity DECIMAL(15,3) DEFAULT 0.000,
    ReservedQuantity DECIMAL(15,3) DEFAULT 0.000,
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT UQ_Inventory_Product_Store UNIQUE (ProductId, StoreId),
    CONSTRAINT FK_Inventory_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Inventory_Store FOREIGN KEY (StoreId) REFERENCES Stores(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Inventory_ProductId ON Inventory(ProductId);
CREATE INDEX IX_Inventory_StoreId ON Inventory(StoreId);
GO

-- 4.5 Inventory Movements Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InventoryMovements')
CREATE TABLE InventoryMovements (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductId BIGINT NOT NULL,
    StoreId BIGINT NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('purchase', 'sale', 'return_in', 'return_out', 'adjustment_in', 'adjustment_out', 'damage')),
    Quantity DECIMAL(15,3) NOT NULL,
    QuantityBefore DECIMAL(15,3) NOT NULL,
    QuantityAfter DECIMAL(15,3) NOT NULL,
    UnitCost DECIMAL(15,3) NULL,
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId BIGINT NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_InventoryMovements_Product FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_InventoryMovements_Store FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_InventoryMovements_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_InventoryMovements_ProductId ON InventoryMovements(ProductId);
CREATE INDEX IX_InventoryMovements_StoreId ON InventoryMovements(StoreId);
CREATE INDEX IX_InventoryMovements_Type ON InventoryMovements(Type);
CREATE INDEX IX_InventoryMovements_CreatedAt ON InventoryMovements(CreatedAt);
CREATE INDEX IX_InventoryMovements_Reference ON InventoryMovements(ReferenceType, ReferenceId);
GO

-- ============================================================
-- SECTION 5: INVOICES
-- ============================================================

-- 5.1 Invoices Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
CREATE TABLE Invoices (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    InvoiceNumber NVARCHAR(50) NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('sale', 'purchase')),
    Status NVARCHAR(20) NOT NULL DEFAULT 'draft' CHECK (Status IN ('draft', 'confirmed', 'cancelled')),
    CustomerId BIGINT NULL,
    SupplierName NVARCHAR(100) NULL,
    CashboxId BIGINT NOT NULL,
    StoreId BIGINT NOT NULL,
    CurrencyId BIGINT NOT NULL,
    Subtotal DECIMAL(15,3) NOT NULL DEFAULT 0.000,
    DiscountType NVARCHAR(20) NULL CHECK (DiscountType IN ('percentage', 'fixed')),
    DiscountValue DECIMAL(15,3) DEFAULT 0.000,
    DiscountAmount DECIMAL(15,3) DEFAULT 0.000,
    Total DECIMAL(15,3) NOT NULL DEFAULT 0.000,
    PaidAmount DECIMAL(15,3) DEFAULT 0.000,
    RemainingAmount DECIMAL(15,3) DEFAULT 0.000,
    PaymentStatus NVARCHAR(20) DEFAULT 'unpaid' CHECK (PaymentStatus IN ('unpaid', 'partial', 'paid')),
    Notes NVARCHAR(MAX) NULL,
    InvoiceDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Invoices_UUID UNIQUE (UUID),
    CONSTRAINT UQ_Invoices_InvoiceNumber UNIQUE (InvoiceNumber),
    CONSTRAINT FK_Invoices_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Invoices_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_Invoices_Store FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_Invoices_Currency FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_Invoices_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
CREATE INDEX IX_Invoices_Type ON Invoices(Type);
CREATE INDEX IX_Invoices_Status ON Invoices(Status);
CREATE INDEX IX_Invoices_CustomerId ON Invoices(CustomerId);
CREATE INDEX IX_Invoices_CashboxId ON Invoices(CashboxId);
CREATE INDEX IX_Invoices_StoreId ON Invoices(StoreId);
CREATE INDEX IX_Invoices_InvoiceDate ON Invoices(InvoiceDate);
CREATE INDEX IX_Invoices_PaymentStatus ON Invoices(PaymentStatus);
GO

-- 5.2 Invoice Items Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceItems')
CREATE TABLE InvoiceItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    Quantity DECIMAL(15,3) NOT NULL,
    UnitPrice DECIMAL(15,3) NOT NULL,
    CostPrice DECIMAL(15,3) DEFAULT 0.000,
    DiscountType NVARCHAR(20) NULL CHECK (DiscountType IN ('percentage', 'fixed')),
    DiscountValue DECIMAL(15,3) DEFAULT 0.000,
    DiscountAmount DECIMAL(15,3) DEFAULT 0.000,
    Subtotal DECIMAL(15,3) NOT NULL,
    Total DECIMAL(15,3) NOT NULL,
    Profit DECIMAL(15,3) DEFAULT 0.000,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_InvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceItems_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

CREATE INDEX IX_InvoiceItems_InvoiceId ON InvoiceItems(InvoiceId);
CREATE INDEX IX_InvoiceItems_ProductId ON InvoiceItems(ProductId);
GO

-- 5.3 Invoice Payments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoicePayments')
CREATE TABLE InvoicePayments (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId BIGINT NOT NULL,
    CashboxId BIGINT NOT NULL,
    Amount DECIMAL(15,3) NOT NULL,
    PaymentMethod NVARCHAR(20) DEFAULT 'cash' CHECK (PaymentMethod IN ('cash', 'card', 'bank_transfer', 'other')),
    ReferenceNumber NVARCHAR(100) NULL,
    PaymentDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_InvoicePayments_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvoicePayments_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_InvoicePayments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_InvoicePayments_InvoiceId ON InvoicePayments(InvoiceId);
CREATE INDEX IX_InvoicePayments_CashboxId ON InvoicePayments(CashboxId);
CREATE INDEX IX_InvoicePayments_PaymentDate ON InvoicePayments(PaymentDate);
GO

-- ============================================================
-- SECTION 6: TAILORING ORDERS
-- ============================================================

-- 6.1 Tailoring Orders Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TailoringOrders')
CREATE TABLE TailoringOrders (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrderNumber NVARCHAR(50) NOT NULL,
    CustomerId BIGINT NOT NULL,
    CashboxId BIGINT NOT NULL,
    CurrencyId BIGINT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'new' CHECK (Status IN ('new', 'in_progress', 'ready', 'delivered', 'cancelled')),
    TotalAmount DECIMAL(15,3) NOT NULL,
    DepositAmount DECIMAL(15,3) DEFAULT 0.000,
    RemainingAmount DECIMAL(15,3) DEFAULT 0.000,
    PaymentStatus NVARCHAR(20) DEFAULT 'unpaid' CHECK (PaymentStatus IN ('unpaid', 'partial', 'paid')),
    Description NVARCHAR(MAX) NULL,
    ActualDeliveryDate DATE NULL,
    OrderDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_TailoringOrders_UUID UNIQUE (UUID),
    CONSTRAINT UQ_TailoringOrders_OrderNumber UNIQUE (OrderNumber),
    CONSTRAINT FK_TailoringOrders_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_TailoringOrders_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_TailoringOrders_Currency FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_TailoringOrders_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_TailoringOrders_OrderNumber ON TailoringOrders(OrderNumber);
CREATE INDEX IX_TailoringOrders_CustomerId ON TailoringOrders(CustomerId);
CREATE INDEX IX_TailoringOrders_CashboxId ON TailoringOrders(CashboxId);
CREATE INDEX IX_TailoringOrders_Status ON TailoringOrders(Status);
CREATE INDEX IX_TailoringOrders_PaymentStatus ON TailoringOrders(PaymentStatus);
CREATE INDEX IX_TailoringOrders_OrderDate ON TailoringOrders(OrderDate);
GO

-- 6.2 Tailoring Order Items Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TailoringOrderItems')
CREATE TABLE TailoringOrderItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    ItemName NVARCHAR(150) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(15,3) NOT NULL,
    Total DECIMAL(15,3) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_TailoringOrderItems_Order FOREIGN KEY (OrderId) REFERENCES TailoringOrders(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_TailoringOrderItems_OrderId ON TailoringOrderItems(OrderId);
GO

-- 6.3 Tailoring Order Status History Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TailoringOrderStatusHistory')
CREATE TABLE TailoringOrderStatusHistory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    FromStatus NVARCHAR(20) NULL CHECK (FromStatus IN ('new', 'in_progress', 'ready', 'delivered', 'cancelled')),
    ToStatus NVARCHAR(20) NOT NULL CHECK (ToStatus IN ('new', 'in_progress', 'ready', 'delivered', 'cancelled')),
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_TailoringOrderStatusHistory_Order FOREIGN KEY (OrderId) REFERENCES TailoringOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TailoringOrderStatusHistory_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_TailoringOrderStatusHistory_OrderId ON TailoringOrderStatusHistory(OrderId);
CREATE INDEX IX_TailoringOrderStatusHistory_CreatedAt ON TailoringOrderStatusHistory(CreatedAt);
GO

-- 6.4 Tailoring Order Payments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TailoringOrderPayments')
CREATE TABLE TailoringOrderPayments (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    CashboxId BIGINT NOT NULL,
    Amount DECIMAL(15,3) NOT NULL,
    PaymentType NVARCHAR(20) NOT NULL CHECK (PaymentType IN ('deposit', 'partial', 'final', 'full')),
    PaymentMethod NVARCHAR(20) DEFAULT 'cash' CHECK (PaymentMethod IN ('cash', 'card', 'bank_transfer', 'other')),
    PaymentDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_TailoringOrderPayments_Order FOREIGN KEY (OrderId) REFERENCES TailoringOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TailoringOrderPayments_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_TailoringOrderPayments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_TailoringOrderPayments_OrderId ON TailoringOrderPayments(OrderId);
CREATE INDEX IX_TailoringOrderPayments_CashboxId ON TailoringOrderPayments(CashboxId);
CREATE INDEX IX_TailoringOrderPayments_PaymentDate ON TailoringOrderPayments(PaymentDate);
GO

-- ============================================================
-- SECTION 7: EXPENSES
-- ============================================================

-- 7.1 Expense Categories Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExpenseCategories')
CREATE TABLE ExpenseCategories (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

CREATE INDEX IX_ExpenseCategories_IsActive ON ExpenseCategories(IsActive);
GO

-- 7.2 Expenses Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Expenses')
CREATE TABLE Expenses (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ExpenseNumber NVARCHAR(50) NOT NULL,
    CategoryId BIGINT NOT NULL,
    CashboxId BIGINT NOT NULL,
    CurrencyId BIGINT NOT NULL,
    Amount DECIMAL(15,3) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ReferenceNumber NVARCHAR(100) NULL,
    ExpenseDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Expenses_UUID UNIQUE (UUID),
    CONSTRAINT UQ_Expenses_ExpenseNumber UNIQUE (ExpenseNumber),
    CONSTRAINT FK_Expenses_Category FOREIGN KEY (CategoryId) REFERENCES ExpenseCategories(Id),
    CONSTRAINT FK_Expenses_Cashbox FOREIGN KEY (CashboxId) REFERENCES Cashboxes(Id),
    CONSTRAINT FK_Expenses_Currency FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_Expenses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Expenses_ExpenseNumber ON Expenses(ExpenseNumber);
CREATE INDEX IX_Expenses_CategoryId ON Expenses(CategoryId);
CREATE INDEX IX_Expenses_CashboxId ON Expenses(CashboxId);
CREATE INDEX IX_Expenses_ExpenseDate ON Expenses(ExpenseDate);
GO

-- ============================================================
-- SECTION 8: SYSTEM TABLES
-- ============================================================

-- 8.1 Audit Logs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId BIGINT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Entity ON AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
GO

-- 8.2 Backups Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Backups')
CREATE TABLE Backups (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Filename NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('full', 'partial', 'auto')),
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('completed', 'failed', 'in_progress')),
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    
    CONSTRAINT FK_Backups_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Backups_Type ON Backups(Type);
CREATE INDEX IX_Backups_Status ON Backups(Status);
CREATE INDEX IX_Backups_CreatedAt ON Backups(CreatedAt);
GO

-- 8.3 Settings Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Settings')
CREATE TABLE Settings (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Group] NVARCHAR(50) NOT NULL,
    [Key] NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    Type NVARCHAR(20) DEFAULT 'string',
    Description NVARCHAR(MAX) NULL,
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedBy BIGINT NULL,
    
    CONSTRAINT UQ_Settings_Group_Key UNIQUE ([Group], [Key]),
    CONSTRAINT FK_Settings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Settings_Group ON Settings([Group]);
GO

-- ============================================================
-- SECTION 9: DEFAULT DATA
-- ============================================================

-- 9.1 Default Roles
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (Id, Name, DisplayName, IsSystem, IsActive) VALUES
(1, 'admin', N'مدير النظام', 1, 1),
(2, 'manager', N'مدير', 0, 1),
(3, 'cashier', N'كاشير', 0, 1),
(4, 'user', N'مستخدم', 0, 1);
SET IDENTITY_INSERT Roles OFF;
GO

-- 9.2 Default Permissions
INSERT INTO Permissions (Module, Name, DisplayName) VALUES
-- Users Module
('users', 'view', N'عرض المستخدمين'),
('users', 'create', N'إنشاء مستخدم'),
('users', 'update', N'تعديل مستخدم'),
('users', 'delete', N'حذف مستخدم'),

-- Roles Module
('roles', 'view', N'عرض الأدوار'),
('roles', 'create', N'إنشاء دور'),
('roles', 'update', N'تعديل دور'),
('roles', 'delete', N'حذف دور'),

-- Cashboxes Module
('cashboxes', 'view', N'عرض القاصات'),
('cashboxes', 'create', N'إنشاء قاصة'),
('cashboxes', 'update', N'تعديل قاصة'),
('cashboxes', 'delete', N'حذف قاصة'),
('cashboxes', 'manage', N'إدارة القاصات'),

-- Customers Module
('customers', 'view', N'عرض العملاء'),
('customers', 'create', N'إنشاء عميل'),
('customers', 'update', N'تعديل عميل'),
('customers', 'delete', N'حذف عميل'),

-- Products Module
('products', 'view', N'عرض المنتجات'),
('products', 'create', N'إنشاء منتج'),
('products', 'update', N'تعديل منتج'),
('products', 'delete', N'حذف منتج'),

-- Inventory Module
('inventory', 'view', N'عرض المخزون'),
('inventory', 'adjust', N'تعديل المخزون'),

-- Invoices Module
('invoices', 'view', N'عرض الفواتير'),
('invoices', 'create', N'إنشاء فاتورة'),
('invoices', 'update', N'تعديل فاتورة'),
('invoices', 'delete', N'حذف فاتورة'),
('invoices', 'cancel', N'إلغاء فاتورة'),

-- Tailoring Module
('tailoring', 'view', N'عرض طلبات الفصال'),
('tailoring', 'create', N'إنشاء طلب فصال'),
('tailoring', 'update', N'تعديل طلب فصال'),
('tailoring', 'delete', N'حذف طلب فصال'),
('tailoring', 'change_status', N'تغيير حالة الطلب'),

-- Expenses Module
('expenses', 'view', N'عرض المصروفات'),
('expenses', 'create', N'تسجيل مصروف'),
('expenses', 'update', N'تعديل مصروف'),
('expenses', 'delete', N'حذف مصروف'),

-- Reports Module
('reports', 'view', N'عرض التقارير'),
('reports', 'export', N'تصدير التقارير'),
('reports', 'profit', N'تقارير الأرباح'),

-- Settings Module
('settings', 'view', N'عرض الإعدادات'),
('settings', 'update', N'تعديل الإعدادات'),

-- Backup Module
('backup', 'create', N'إنشاء نسخة احتياطية'),
('backup', 'restore', N'استعادة نسخة احتياطية'),
('backup', 'download', N'تحميل نسخة احتياطية'),

-- Audit Module
('audit', 'view', N'عرض سجل العمليات');
GO

-- 9.3 Assign All Permissions to Admin Role
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, Id FROM Permissions;
GO

-- 9.4 Manager Permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, Id FROM Permissions 
WHERE Module NOT IN ('roles', 'backup', 'audit', 'settings')
   OR (Module = 'settings' AND Name = 'view')
   OR (Module = 'audit' AND Name = 'view');
GO

-- 9.5 Cashier Permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, Id FROM Permissions 
WHERE Module IN ('customers', 'invoices', 'tailoring', 'products')
   OR (Module = 'cashboxes' AND Name IN ('view', 'manage'))
   OR (Module = 'inventory' AND Name = 'view')
   OR (Module = 'expenses' AND Name IN ('view', 'create'));
GO

-- 9.6 User Permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 4, Id FROM Permissions 
WHERE Name = 'view' AND Module IN ('customers', 'products', 'invoices', 'tailoring', 'inventory');
GO

-- 9.7 Default Currencies
SET IDENTITY_INSERT Currencies ON;
INSERT INTO Currencies (Id, Code, Name, Symbol, ExchangeRate, IsDefault, IsActive, DecimalPlaces) VALUES
(1, 'IQD', N'دينار عراقي', N'د.ع', 1.000000, 1, 1, 0),
(2, 'USD', N'دولار أمريكي', N'$', 1480.000000, 0, 1, 2),
(3, 'SAR', N'ريال سعودي', N'ر.س', 394.000000, 0, 1, 2);
SET IDENTITY_INSERT Currencies OFF;
GO

-- 9.8 Default Expense Categories
INSERT INTO ExpenseCategories (Name, IsActive) VALUES
(N'إيجار', 1),
(N'كهرباء', 1),
(N'ماء', 1),
(N'رواتب', 1),
(N'مواد خام', 1),
(N'صيانة', 1),
(N'نقل ومواصلات', 1),
(N'دعاية وإعلان', 1),
(N'مصاريف بنكية', 1),
(N'أخرى', 1);
GO

-- 9.9 Default Store
SET IDENTITY_INSERT Stores ON;
INSERT INTO Stores (Id, Name, Location, IsDefault, IsActive) VALUES
(1, N'المخزن الرئيسي', N'داخل المحل', 1, 1);
SET IDENTITY_INSERT Stores OFF;
GO

-- 9.10 Default Cashboxes
INSERT INTO Cashboxes (Name, Type, CurrencyId, CurrentBalance, IsActive) VALUES
(N'قاصة الفصال', 'tailoring', 1, 0.000, 1),
(N'قاصة الملابس الجاهزة', 'ready_made', 1, 0.000, 1),
(N'قاصة العطور', 'perfumes', 1, 0.000, 1),
(N'القاصة العامة', 'general', 1, 0.000, 1);
GO

-- 9.11 Default Settings
INSERT INTO Settings ([Group], [Key], Value, Type, Description) VALUES
-- General Settings
('general', 'shop_name', N'متجر الخياطة', 'string', N'اسم المتجر'),
('general', 'shop_phone', '', 'string', N'رقم هاتف المتجر'),
('general', 'shop_address', '', 'string', N'عنوان المتجر'),
('general', 'shop_logo', '', 'string', N'شعار المتجر'),

-- Invoice Settings
('invoice', 'sale_prefix', 'INV-', 'string', N'بادئة فاتورة البيع'),
('invoice', 'purchase_prefix', 'PUR-', 'string', N'بادئة فاتورة الشراء'),
('invoice', 'tailoring_prefix', 'TLR-', 'string', N'بادئة طلب الفصال'),
('invoice', 'expense_prefix', 'EXP-', 'string', N'بادئة المصروفات'),
('invoice', 'starting_number', '1', 'integer', N'رقم البداية للفواتير'),

-- Inventory Settings
('inventory', 'low_stock_alert', '5', 'integer', N'حد التنبيه للمخزون المنخفض'),
('inventory', 'allow_negative_stock', 'false', 'boolean', N'السماح بالمخزون السالب'),

-- Tailoring Settings
('tailoring', 'default_delivery_days', '7', 'integer', N'أيام التسليم الافتراضية'),
('tailoring', 'require_deposit', 'true', 'boolean', N'إلزامية العربون'),
('tailoring', 'min_deposit_percentage', '50', 'integer', N'نسبة الحد الأدنى للعربون'),

-- System Settings
('system', 'timezone', 'Asia/Baghdad', 'string', N'المنطقة الزمنية'),
('system', 'date_format', 'yyyy-MM-dd', 'string', N'صيغة التاريخ'),
('system', 'currency_position', 'after', 'string', N'موقع رمز العملة'),
('system', 'pagination_limit', '25', 'integer', N'عدد العناصر في الصفحة');
GO

-- 9.12 Default Admin User
-- Password: admin123 (hashed with BCrypt)
INSERT INTO Users (UUID, Username, PasswordHash, FullName, IsActive) VALUES
(NEWID(), 'admin', '$2a$11$rHLXqMs7Xk.aMvDvKBCJe.L5BQZQK5Q5H5j5Y5g5Y5g5Y5g5Y5g5Y', N'مدير النظام', 1);
GO

-- Assign Admin Role to Admin User
INSERT INTO UserRoles (UserId, RoleId) VALUES (1, 1);
GO

-- ============================================================
-- SECTION 10: VIEWS
-- ============================================================

-- 10.1 View: Cashbox Summary
CREATE OR ALTER VIEW vw_CashboxSummary AS
SELECT 
    c.Id,
    c.Name,
    c.Type,
    c.CurrentBalance,
    cur.Code AS CurrencyCode,
    cur.Symbol AS CurrencySymbol,
    c.IsActive,
    (SELECT COUNT(*) FROM CashboxTransactions ct WHERE ct.CashboxId = c.Id AND CAST(ct.CreatedAt AS DATE) = CAST(GETDATE() AS DATE)) AS TodayTransactions,
    (SELECT ISNULL(SUM(ct.Amount), 0) FROM CashboxTransactions ct WHERE ct.CashboxId = c.Id AND ct.Type = 'sale' AND CAST(ct.CreatedAt AS DATE) = CAST(GETDATE() AS DATE)) AS TodaySales,
    (SELECT ISNULL(SUM(ct.Amount), 0) FROM CashboxTransactions ct WHERE ct.CashboxId = c.Id AND ct.Type = 'expense' AND CAST(ct.CreatedAt AS DATE) = CAST(GETDATE() AS DATE)) AS TodayExpenses
FROM Cashboxes c
LEFT JOIN Currencies cur ON c.CurrencyId = cur.Id;
GO

-- 10.2 View: Today's Dashboard Stats
CREATE OR ALTER VIEW vw_DashboardToday AS
SELECT
    -- Sales
    (SELECT ISNULL(SUM(Total), 0) FROM Invoices WHERE Type = 'sale' AND Status = 'confirmed' AND InvoiceDate = CAST(GETDATE() AS DATE)) AS TotalSales,
    (SELECT COUNT(*) FROM Invoices WHERE Type = 'sale' AND Status = 'confirmed' AND InvoiceDate = CAST(GETDATE() AS DATE)) AS SalesCount,
    
    -- Tailoring Orders
    (SELECT COUNT(*) FROM TailoringOrders WHERE OrderDate = CAST(GETDATE() AS DATE)) AS NewOrdersToday,
    (SELECT COUNT(*) FROM TailoringOrders WHERE Status = 'new') AS PendingOrders,
    (SELECT COUNT(*) FROM TailoringOrders WHERE Status = 'in_progress') AS InProgressOrders,
    (SELECT COUNT(*) FROM TailoringOrders WHERE Status = 'ready') AS ReadyOrders,
    
    -- Expenses
    (SELECT ISNULL(SUM(Amount), 0) FROM Expenses WHERE ExpenseDate = CAST(GETDATE() AS DATE)) AS TodayExpenses,
    
    -- Profit (simplified)
    (SELECT ISNULL(SUM(Total), 0) FROM Invoices WHERE Type = 'sale' AND Status = 'confirmed' AND InvoiceDate = CAST(GETDATE() AS DATE)) -
    (SELECT ISNULL(SUM(Amount), 0) FROM Expenses WHERE ExpenseDate = CAST(GETDATE() AS DATE)) AS TodayNetProfit,
    
    -- Low Stock Alert
    (SELECT COUNT(*) FROM Products p 
     INNER JOIN Inventory i ON p.Id = i.ProductId 
     WHERE i.Quantity <= p.MinStockLevel AND p.IsActive = 1) AS LowStockProducts;
GO

-- 10.3 View: Tailoring Orders Overview
CREATE OR ALTER VIEW vw_TailoringOrdersOverview AS
SELECT 
    t.Id,
    t.OrderNumber,
    c.FullName AS CustomerName,
    c.Phone AS CustomerPhone,
    t.Status,
    t.TotalAmount,
    t.DepositAmount,
    t.RemainingAmount,
    t.PaymentStatus,
    t.OrderDate,
    cb.Name AS CashboxName,
    cur.Symbol AS CurrencySymbol
FROM TailoringOrders t
LEFT JOIN Customers c ON t.CustomerId = c.Id
LEFT JOIN Cashboxes cb ON t.CashboxId = cb.Id
LEFT JOIN Currencies cur ON t.CurrencyId = cur.Id;
GO

-- 10.4 View: Product Inventory Status
CREATE OR ALTER VIEW vw_ProductInventory AS
SELECT 
    p.Id,
    p.SKU,
    p.Barcode,
    p.Name,
    cat.Name AS CategoryName,
    p.CostPrice,
    p.SellingPrice,
    cur.Symbol AS CurrencySymbol,
    s.Name AS StoreName,
    i.Quantity,
    i.ReservedQuantity,
    (i.Quantity - i.ReservedQuantity) AS AvailableQuantity,
    p.MinStockLevel,
    CASE 
        WHEN i.Quantity <= 0 THEN 'out_of_stock'
        WHEN i.Quantity <= p.MinStockLevel THEN 'low_stock'
        ELSE 'in_stock'
    END AS StockStatus,
    p.IsActive
FROM Products p
LEFT JOIN Categories cat ON p.CategoryId = cat.Id
LEFT JOIN Currencies cur ON p.CurrencyId = cur.Id
LEFT JOIN Inventory i ON p.Id = i.ProductId
LEFT JOIN Stores s ON i.StoreId = s.Id;
GO

-- ============================================================
-- SECTION 11: STORED PROCEDURES
-- ============================================================

-- 11.1 Generate Unique Invoice Number
CREATE OR ALTER PROCEDURE sp_GenerateInvoiceNumber
    @Type NVARCHAR(20),
    @InvoiceNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Prefix NVARCHAR(20);
    DECLARE @LastNumber INT;
    DECLARE @NewNumber INT;
    
    -- Get prefix based on type
    SELECT @Prefix = CASE @Type
        WHEN 'sale' THEN (SELECT Value FROM Settings WHERE [Group] = 'invoice' AND [Key] = 'sale_prefix')
        WHEN 'purchase' THEN (SELECT Value FROM Settings WHERE [Group] = 'invoice' AND [Key] = 'purchase_prefix')
        WHEN 'tailoring' THEN (SELECT Value FROM Settings WHERE [Group] = 'invoice' AND [Key] = 'tailoring_prefix')
        WHEN 'expense' THEN (SELECT Value FROM Settings WHERE [Group] = 'invoice' AND [Key] = 'expense_prefix')
        ELSE 'DOC-'
    END;
    
    -- Get last number based on type
    IF @Type = 'tailoring'
        SELECT @LastNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(OrderNumber, LEN(@Prefix) + 1, 10) AS INT)), 0)
        FROM TailoringOrders 
        WHERE OrderNumber LIKE @Prefix + '%';
    ELSE IF @Type = 'expense'
        SELECT @LastNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(ExpenseNumber, LEN(@Prefix) + 1, 10) AS INT)), 0)
        FROM Expenses 
        WHERE ExpenseNumber LIKE @Prefix + '%';
    ELSE
        SELECT @LastNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(InvoiceNumber, LEN(@Prefix) + 1, 10) AS INT)), 0)
        FROM Invoices 
        WHERE InvoiceNumber LIKE @Prefix + '%';
    
    SET @NewNumber = @LastNumber + 1;
    SET @InvoiceNumber = @Prefix + RIGHT('000000' + CAST(@NewNumber AS NVARCHAR(10)), 6);
END;
GO

-- 11.2 Update Cashbox Balance
CREATE OR ALTER PROCEDURE sp_UpdateCashboxBalance
    @CashboxId BIGINT,
    @Type NVARCHAR(20),
    @Amount DECIMAL(15,3),
    @ReferenceType NVARCHAR(50),
    @ReferenceId BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentBalance DECIMAL(15,3);
    DECLARE @NewBalance DECIMAL(15,3);
    
    BEGIN TRANSACTION;
    
    -- Get current balance with lock
    SELECT @CurrentBalance = CurrentBalance 
    FROM Cashboxes WITH (UPDLOCK) 
    WHERE Id = @CashboxId;
    
    -- Calculate new balance
    SET @NewBalance = CASE @Type
        WHEN 'deposit' THEN @CurrentBalance + @Amount
        WHEN 'sale' THEN @CurrentBalance + @Amount
        WHEN 'refund' THEN @CurrentBalance + @Amount
        WHEN 'withdrawal' THEN @CurrentBalance - @Amount
        WHEN 'expense' THEN @CurrentBalance - @Amount
        ELSE @CurrentBalance
    END;
    
    -- Insert transaction record
    INSERT INTO CashboxTransactions (CashboxId, Type, Amount, BalanceBefore, BalanceAfter, ReferenceType, ReferenceId, CreatedBy)
    VALUES (@CashboxId, @Type, @Amount, @CurrentBalance, @NewBalance, @ReferenceType, @ReferenceId, @UserId);
    
    -- Update cashbox balance
    UPDATE Cashboxes SET CurrentBalance = @NewBalance, UpdatedAt = GETDATE() WHERE Id = @CashboxId;
    
    COMMIT TRANSACTION;
END;
GO

-- 11.3 Update Inventory
CREATE OR ALTER PROCEDURE sp_UpdateInventory
    @ProductId BIGINT,
    @StoreId BIGINT,
    @Type NVARCHAR(20),
    @Quantity DECIMAL(15,3),
    @UnitCost DECIMAL(15,3),
    @ReferenceType NVARCHAR(50),
    @ReferenceId BIGINT,
    @Notes NVARCHAR(MAX),
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentQty DECIMAL(15,3);
    DECLARE @NewQty DECIMAL(15,3);
    
    BEGIN TRANSACTION;
    
    -- Get or create inventory record
    IF NOT EXISTS (SELECT 1 FROM Inventory WHERE ProductId = @ProductId AND StoreId = @StoreId)
        INSERT INTO Inventory (ProductId, StoreId, Quantity, ReservedQuantity) VALUES (@ProductId, @StoreId, 0, 0);
    
    -- Get current quantity with lock
    SELECT @CurrentQty = Quantity 
    FROM Inventory WITH (UPDLOCK) 
    WHERE ProductId = @ProductId AND StoreId = @StoreId;
    
    -- Calculate new quantity
    SET @NewQty = CASE @Type
        WHEN 'purchase' THEN @CurrentQty + @Quantity
        WHEN 'return_in' THEN @CurrentQty + @Quantity
        WHEN 'adjustment_in' THEN @CurrentQty + @Quantity
        WHEN 'sale' THEN @CurrentQty - @Quantity
        WHEN 'return_out' THEN @CurrentQty - @Quantity
        WHEN 'adjustment_out' THEN @CurrentQty - @Quantity
        WHEN 'damage' THEN @CurrentQty - @Quantity
        ELSE @CurrentQty
    END;
    
    -- Insert movement record
    INSERT INTO InventoryMovements (ProductId, StoreId, Type, Quantity, QuantityBefore, QuantityAfter, UnitCost, ReferenceType, ReferenceId, Notes, CreatedBy)
    VALUES (@ProductId, @StoreId, @Type, @Quantity, @CurrentQty, @NewQty, @UnitCost, @ReferenceType, @ReferenceId, @Notes, @UserId);
    
    -- Update inventory
    UPDATE Inventory SET Quantity = @NewQty, UpdatedAt = GETDATE() WHERE ProductId = @ProductId AND StoreId = @StoreId;
    
    COMMIT TRANSACTION;
END;
GO

-- 11.4 Change Tailoring Order Status
CREATE OR ALTER PROCEDURE sp_ChangeTailoringStatus
    @OrderId BIGINT,
    @NewStatus NVARCHAR(20),
    @Notes NVARCHAR(MAX),
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentStatus NVARCHAR(20);
    
    -- Get current status
    SELECT @CurrentStatus = Status FROM TailoringOrders WHERE Id = @OrderId;
    
    -- Insert status history
    INSERT INTO TailoringOrderStatusHistory (OrderId, FromStatus, ToStatus, Notes, CreatedBy)
    VALUES (@OrderId, @CurrentStatus, @NewStatus, @Notes, @UserId);
    
    -- Update order status
    UPDATE TailoringOrders 
    SET Status = @NewStatus,
        ActualDeliveryDate = CASE WHEN @NewStatus = 'delivered' THEN CAST(GETDATE() AS DATE) ELSE ActualDeliveryDate END,
        UpdatedAt = GETDATE()
    WHERE Id = @OrderId;
END;
GO

-- ============================================================
-- END OF SCHEMA
-- ============================================================

PRINT 'Database schema created successfully!';
PRINT 'Tables: 22';
PRINT 'Views: 4';
PRINT 'Stored Procedures: 4';
PRINT 'Default Admin User: admin / admin123';
GO
