using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface IOrderService
{
    //Task<(bool ok, string? error, OrderDto? order)> PlaceOrderAsync(PlaceOrderRequest req);
    //Task<OrderDto?> GetByIdAsync(int orderId);
    //Task<OrderHistoryDto[]> GetOrderHistoryAsync(int orderId);
    //Task<(bool ok, string? error)> UpdateOrderStatusAsync(int orderId, string newStatus, string? note = null);

 // ✅ Place Order
        Task<(bool ok, string? error, OrderDto? order)> PlaceOrderAsync(PlaceOrderRequest request);

        // ✅ Get Orders for Logged-in User
        Task<OrderDto[]> GetUserOrdersAsync(int userId);
}
