using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class ApiLog
{
    public int ApiLogId { get; set; }

    public int? UserId { get; set; }

    public string Endpoint { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int StatusCode { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
