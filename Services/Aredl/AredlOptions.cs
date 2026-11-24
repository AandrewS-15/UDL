namespace UDL.Services.Aredl;

public sealed class AredlOptions
{
    public const string SectionName = "Aredl";
    public string BaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public bool SyncEnabled { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 60;
    public int SyncInitialDelaySeconds { get; set; } = 15;
}
