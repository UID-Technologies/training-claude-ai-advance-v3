using Claims.Application;
using Claims.Domain;

namespace Claims.Infrastructure;

public sealed class ConsoleNotificationService : INotificationService
{
    public Task SendClaimSubmittedAsync(
        Claim claim,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Notification: claim {claim.Id} submitted for member {claim.MemberId}");
        return Task.CompletedTask;
    }
}
