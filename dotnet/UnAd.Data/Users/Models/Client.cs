using System;
using System.Collections.Generic;

namespace UnAd.Data.Users.Models;

public partial class Client {
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? SubscriptionId { get; set; }

    public string Locale { get; set; } = null!;

    public virtual ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
}
