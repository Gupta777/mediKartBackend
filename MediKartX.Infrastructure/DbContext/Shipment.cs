using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public string? CourierName { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Status { get; set; }

    public DateOnly? EstimatedDeliveryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
