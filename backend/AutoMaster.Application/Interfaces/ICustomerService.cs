using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface ICustomerService
{
    Task<(List<CustomerDto> Items, int Total)> GetCustomersAsync(string? search, string? status, int page, int limit);
    Task<CustomerDetailDto> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);
    Task DeleteCustomerAsync(Guid id);
}
