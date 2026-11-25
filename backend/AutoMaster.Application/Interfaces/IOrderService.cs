using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IOrderService
{
    Task<(List<OrderDto> Items, int Total)> GetOrdersAsync(string? status, string? search, int page, int limit);
    Task<OrderDetailDto> GetOrderByIdAsync(Guid id);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, Guid currentUserId);
    Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderRequest request);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid currentUserId);
    Task DeleteOrderAsync(Guid id);
    Task<List<KanbanColumnDto>> GetKanbanAsync();
    Task<byte[]> ExportCsvAsync();
}
