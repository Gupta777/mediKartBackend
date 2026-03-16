using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Brand
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
