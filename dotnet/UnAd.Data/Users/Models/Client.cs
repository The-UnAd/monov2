﻿using System;
using System.Collections.Generic;

namespace UnAd.Data.Users.Models;

public partial class Client
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? SubscriptionId { get; set; }

    public string Locale { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public DateTime? JoinedDate { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<Subscriber> SubscriberPhoneNumbers { get; set; } = new List<Subscriber>();
}
