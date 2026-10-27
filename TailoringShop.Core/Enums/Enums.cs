namespace TailoringShop.Core.Enums;

public enum CashboxType
{
    Tailoring,
    ReadyMade,
    Perfumes,
    General
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Sale,
    Expense,
    Refund,
    Adjustment
}

public enum InvoiceType
{
    Sale,
    Purchase
}

public enum InvoiceStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public enum PaymentStatus
{
    Unpaid,
    Partial,
    Paid
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Other
}

public enum DiscountType
{
    Percentage,
    Fixed
}

public enum OrderStatus
{
    New,
    InProgress,
    Ready,
    Delivered,
    Cancelled
}

public enum PaymentType
{
    Deposit,
    Partial,
    Final,
    Full
}

public enum MovementType
{
    Purchase,
    Sale,
    ReturnIn,
    ReturnOut,
    AdjustmentIn,
    AdjustmentOut,
    Damage
}

public enum BackupType
{
    Full,
    Partial,
    Auto
}

public enum BackupStatus
{
    Completed,
    Failed,
    InProgress
}

public enum StockStatus
{
    InStock,
    LowStock,
    OutOfStock
}
