using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly MediKartXDbContext _db;
    private readonly ICouponService _couponSvc;
    private readonly IEmailSender _emailSender;

    public OrderService(MediKartXDbContext db, ICouponService couponSvc, IEmailSender emailSender)
    {
        _db = db;
        _couponSvc = couponSvc;
        _emailSender = emailSender;
    }

    public async Task<(bool ok, string? error, OrderDto? order)> PlaceOrderAsync(PlaceOrderRequest req)
    {
        if (req.Items == null || req.Items.Count == 0) return (false, "No items", null);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // validate stock and activity
            foreach (var it in req.Items)
            {
                var med = await _db.Medicines.FindAsync(it.MedicineId);
                if (med == null) return (false, $"Medicine {it.MedicineId} not found", null);
                if (med.IsActive == false) return (false, $"Medicine {med.Name} is inactive", null);
                if (med.Stock < it.Quantity) return (false, $"Insufficient stock for {med.Name}", null);
            }

            // compute totals
            decimal subtotal = 0m;
            foreach (var it in req.Items)
            {
                var med = await _db.Medicines.FindAsync(it.MedicineId);
                subtotal += (med!.SellingPrice * it.Quantity);
            }

            decimal discount = 0m;
            if (!string.IsNullOrWhiteSpace(req.CouponCode))
            {
                var res = await _couponSvc.ValidateAndApplyAsync(req.CouponCode!, req.UserId, subtotal);
                if (!res.Success) return (false, res.Message, null);
                discount = res.DiscountAmount;
            }

            var total = subtotal - discount;

            var order = new Order
            {
                UserId = req.UserId,
                TotalAmount = total,
                PaymentMode = req.PaymentMode,
                OrderStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // create order items and deduct stock
            foreach (var it in req.Items)
            {
                var med = await _db.Medicines.FindAsync(it.MedicineId)!;
                var oi = new OrderItem
                {
                    OrderId = order.OrderId,
                    MedicineId = it.MedicineId,
                    Quantity = it.Quantity,
                    Price = med.SellingPrice,
                    CreatedAt = DateTime.UtcNow
                };
                _db.OrderItems.Add(oi);

                // deduct stock and record stock history (use existing StockHistory properties)
                med.Stock -= it.Quantity;
                _db.StockHistories.Add(new StockHistory { MedicineId = med.MedicineId, ChangedStock = -it.Quantity, ChangedAt = DateTime.UtcNow, Reason = $"Order {order.OrderId}" });
            }

            // record initial order history
            _db.OrderHistories.Add(new OrderHistory { OrderId = order.OrderId, FromStatus = null, ToStatus = "Pending", ChangedAt = DateTime.UtcNow, Note = "Order placed" });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // async notification (best effort)
            try
            {
                var user = await _db.Users.FindAsync(req.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    await _emailSender.SendEmailAsync(user.Email, "Order placed", $"Your order #{order.OrderId} has been placed.");
            }
            catch { }

            var dto = new OrderDto { OrderId = order.OrderId, UserId = order.UserId, TotalAmount = order.TotalAmount, OrderStatus = order.OrderStatus, PaymentMode = order.PaymentMode, CreatedAt = order.CreatedAt };
            return (true, null, dto);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, ex.Message, null);
        }
    }

    public async Task<OrderDto?> GetByIdAsync(int orderId)
    {
        var o = await _db.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Medicine).FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return null;
        return new OrderDto {
            OrderId = o.OrderId,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            OrderStatus = o.OrderStatus,
            PaymentMode = o.PaymentMode,
            CreatedAt = o.CreatedAt,
            Items = o.OrderItems.Select(oi => new OrderItemDto { OrderItemId = oi.OrderItemId, MedicineId = oi.MedicineId, MedicineName = oi.Medicine?.Name, Quantity = oi.Quantity, Price = oi.Price }).ToArray()
        };
    }

    public async Task<OrderHistoryDto[]> GetOrderHistoryAsync(int orderId)
    {
        var hist = await _db.OrderHistories.Where(h => h.OrderId == orderId).OrderBy(h => h.ChangedAt).ToListAsync();
        return hist.Select(h => new OrderHistoryDto { OrderHistoryId = h.OrderHistoryId, OrderId = h.OrderId, FromStatus = h.FromStatus, ToStatus = h.ToStatus, ChangedAt = h.ChangedAt, Note = h.Note }).ToArray();
    }

    public async Task<(bool ok, string? error)> UpdateOrderStatusAsync(int orderId, string newStatus, string? note = null)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return (false, "Order not found");
            var prev = order.OrderStatus;
            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _db.OrderHistories.Add(new OrderHistory { OrderId = orderId, FromStatus = prev, ToStatus = newStatus, ChangedAt = DateTime.UtcNow, Note = note });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // optional notification
            try
            {
                var user = await _db.Users.FindAsync(order.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    await _emailSender.SendEmailAsync(user.Email, "Order status updated", $"Your order #{order.OrderId} status changed to {newStatus}.");
            }
            catch { }

            return (true, null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, ex.Message);
        }
    }
}
