using System;
using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class WishlistDto
{
    public int WishlistId { get; set; }
    public int UserId { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}

public class WishlistItemDto
{
    public int WishlistItemId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}