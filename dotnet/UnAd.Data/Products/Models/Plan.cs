using System;
using System.Collections.Generic;

namespace UnAd.Data.Products.Models;

public partial class Plan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<PriceTier> PriceTiers { get; set; } = new List<PriceTier>();
}
