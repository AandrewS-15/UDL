namespace UDL.Services.Aredl;

public interface IAredlSyncService
{
    Task<AredlSyncResult> SynchronizeAsync(CancellationToken cancellationToken = default);
}
