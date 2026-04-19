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

    public OrderService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<(bool ok, string? error, OrderDto? order)> PlaceOrderAsync(PlaceOrderRequest req)
    {
        if (req.Items == null || !req.Items.Any())
            return (false, "Cart is empty", null);

        var medicines = await _db.Medicines
            .Where(m => req.Items.Select(i => i.MedicineId).Contains(m.MedicineId))
            .ToListAsync();

        decimal total = 0;

        // ✅ Validate stock + calculate total
        foreach (var item in req.Items)
        {
            var med = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
            if (med == null)
                return (false, $"Medicine {item.MedicineId} not found", null);

            if (med.Stock < item.Quantity)
                return (false, $"Insufficient stock for {med.Name}", null);

            total += med.SellingPrice * item.Quantity;
        }

        // ✅ Apply coupon
        if (!string.IsNullOrEmpty(req.CouponCode))
        {
            var coupon = await _db.Coupons
                .FirstOrDefaultAsync(c => c.Code == req.CouponCode && (bool)c.IsActive);

            if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow)
                return (false, "Invalid or expired coupon", null);

            total -= (total * coupon.DiscountPercent / 100);

            _db.CouponUsages.Add(new CouponUsage
            {
                CouponId = coupon.CouponId,
                UserId = req.UserId,
                UsedAt = DateTime.UtcNow
            });
        }

        // ✅ Create Order
        var order = new Order
        {
            UserId = req.UserId,
            TotalAmount = total,
            PaymentMode = req.PaymentMode ?? "COD",
            OrderStatus = "Placed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // ✅ Order Items
        foreach (var item in req.Items)
        {
            var med = medicines.First(m => m.MedicineId == item.MedicineId);

            _db.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                Price = med.SellingPrice
            });

            // reduce stock
            med.Stock -= item.Quantity;

            _db.StockHistories.Add(new StockHistory
            {
                MedicineId = med.MedicineId,
                PreviousStock = med.Stock + item.Quantity,
                ChangedStock = med.Stock,
                Reason = "Order placed"
            });
        }

        // ✅ Payment entry
        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            OrderId = order.OrderId,
            PaymentMode = order.PaymentMode,
            PaymentStatus = order.PaymentMode == "COD" ? "Pending" : "Paid",
            Amount = total
        });

        // ✅ Reward points (simple logic)
        var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.UserId == req.UserId);
        if (reward == null)
        {
            reward = new Reward { UserId = req.UserId, Points = 0 };
            _db.Rewards.Add(reward);
        }

        int earnedPoints = (int)(total / 10); // 10₹ = 1 point
        reward.Points += earnedPoints;

        _db.RewardTransactions.Add(new RewardTransaction
        {
            RewardId = reward.RewardId,
            OrderId = order.OrderId,
            PointsChanged = earnedPoints,
            Reason = "Order reward"
        });

        // ✅ Shipment (manual for now)
        _db.Shipments.Add(new Shipment
        {
            OrderId = order.OrderId,
            CourierName = "Manual Delivery",
            Status = "Pending",
            EstimatedDeliveryDate =DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3)    
        });

        await _db.SaveChangesAsync();

        // ✅ Response DTO
        var dto = new OrderDto
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            PaymentMode = order.PaymentMode,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            Items = await _db.OrderItems
                .Where(x => x.OrderId == order.OrderId)
                .Include(x => x.Medicine)
                .Select(x => new OrderItemDto
                {
                    OrderItemId = x.OrderItemId,
                    MedicineId = x.MedicineId,
                    MedicineName = x.Medicine.Name,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToArrayAsync()
        };

        return (true, null, dto);
    }

    public async Task<OrderDto[]> GetUserOrdersAsync(int userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Medicine)
            .Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                PaymentMode = o.PaymentMode,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    OrderItemId = i.OrderItemId,
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine.Name,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToArray()
            }).ToArrayAsync();
    }

    /*updated code
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
    */
    
}
