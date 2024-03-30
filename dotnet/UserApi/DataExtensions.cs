using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users.Models;

namespace UserApi;

public static class DataExtensions {
    public static IQueryable<Client> WithActiveSubscriptions(this DbSet<Client> clients) =>
        clients.Where(clients => !string.IsNullOrEmpty(clients.SubscriptionId));
}
