using System;
using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class PlaceOrderRequest
{
    public int UserId { get; set; }
    public string? CouponCode { get; set; }
    public string? PaymentMode { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}

public class OrderItemRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentMode { get; set; }
    public DateTime? CreatedAt { get; set; }
    public OrderItemDto[] Items { get; set; } = Array.Empty<OrderItemDto>();
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderHistoryDto
{
    public int OrderHistoryId { get; set; }
    public int OrderId { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? Note { get; set; }
}
