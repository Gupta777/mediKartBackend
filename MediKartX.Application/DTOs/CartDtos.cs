using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}

public class CartDto
{
    public int CartId { get; set; }
    public string? GuestToken { get; set; }
    public int? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
}

public class AddToCartRequest
{
    public string? GuestToken { get; set; }
    public int? UserId { get; set; } // optional for tests; real auth uses user context
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}

public class MergeCartRequest
{
    public string GuestToken { get; set; } = string.Empty;
}
