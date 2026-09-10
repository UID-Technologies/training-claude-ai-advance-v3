using Claims.Domain;

namespace Claims.Application;

public interface INotificationService
{
    Task SendClaimSubmittedAsync(Claim claim, CancellationToken cancellationToken = default);
}
