using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Platform.Application.Authentication;

public sealed class InMemoryAuthenticationTicketStore(TimeProvider timeProvider) : ITicketStore
{
    private readonly ConcurrentDictionary<string, AuthenticationTicket> _tickets = new();

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        RemoveExpiredTickets();
        var key = Guid.NewGuid().ToString("N");
        _tickets[key] = ticket;
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        _tickets[key] = ticket;
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        if (!_tickets.TryGetValue(key, out var ticket))
            return Task.FromResult<AuthenticationTicket?>(null);

        if (ticket.Properties.ExpiresUtc <= timeProvider.GetUtcNow())
        {
            _tickets.TryRemove(key, out _);
            return Task.FromResult<AuthenticationTicket?>(null);
        }

        return Task.FromResult<AuthenticationTicket?>(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _tickets.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void RemoveExpiredTickets()
    {
        var now = timeProvider.GetUtcNow();
        foreach (var pair in _tickets)
        {
            if (pair.Value.Properties.ExpiresUtc <= now)
                _tickets.TryRemove(pair.Key, out _);
        }
    }
}
