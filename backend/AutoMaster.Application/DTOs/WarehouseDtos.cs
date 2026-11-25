namespace AutoMaster.Application.DTOs;

public record PartDto(
    Guid Id,
    string Name,
    string Sku,
    string Category,
    int Quantity,
    int MinQuantity,
    decimal PurchasePrice,
    decimal SalePrice,
    double MarginPercent,
    string Status);

public record CreatePartRequest(
    string Name,
    string Sku,
    string Category,
    int Quantity,
    int MinQuantity,
    decimal PurchasePrice,
    decimal SalePrice);

public record UpdatePartRequest(
    string Name,
    string Sku,
    string Category,
    int Quantity,
    int MinQuantity,
    decimal PurchasePrice,
    decimal SalePrice);

public record UpdatePartQuantityRequest(int Quantity);

public record WarehouseStatsDto(
    int TotalItems,
    int LowStockCount,
    decimal TotalValue,
    decimal MonthlyTurnover);
