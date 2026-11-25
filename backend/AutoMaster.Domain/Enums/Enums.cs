namespace AutoMaster.Domain.Enums;

public enum UserRole
{
    Admin,
    Manager,
    Mechanic,
    Accountant,
    Client
}

public enum CustomerStatus
{
    Regular,
    Vip,
    Corporate
}

public enum OrderStatus
{
    Intake,
    Diagnosis,
    Estimate,
    Approved,
    InProgress,
    Waiting,
    Ready,
    Completed
}

public enum OrderPriority
{
    Normal,
    High,
    Urgent
}

public enum MechanicSpecialization
{
    Engine,
    Suspension,
    Electrical,
    Body,
    General
}

public enum MechanicLevel
{
    Junior,
    Middle,
    Senior,
    Expert
}

public enum PartCategory
{
    Filters,
    Oils,
    Brakes,
    Suspension,
    Engine,
    Electrical,
    Body,
    Consumables
}

public enum TransactionType
{
    Income,
    Expense
}

public enum BranchType
{
    Own,
    Franchise,
    WhiteLabel,
    Partner
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}

public enum LoyaltyTransactionType
{
    Award,
    Redeem
}
