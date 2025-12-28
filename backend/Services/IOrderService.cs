using Backend.DTOs;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public interface IOrderService
{
    // User order operations
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId);
    Task<OrderDto?> GetOrderByIdAsync(int orderId); // Admin version - no user restriction
    Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId);
    Task<PagedResult<OrderDto>> GetAllOrdersAsync(int pageNumber, int pageSize);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);

    // Guest order operations
    Task<GuestOrderResponse> CreateGuestOrderAsync(GuestCheckoutRequest request);
    Task<GuestOrderResponse?> GetGuestOrderByNumberAsync(string orderNumber, string email);
}
