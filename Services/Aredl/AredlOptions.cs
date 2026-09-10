namespace UDL.Services.Aredl;

public sealed class AredlOptions
{
    public const string SectionName = "Aredl";
    public string BaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
}
