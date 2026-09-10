using System.Net;

namespace UDL.Services.Aredl;

public enum AredlFailure { Http, Network, Timeout, InvalidResponse }

public sealed class AredlException(
    AredlFailure failure, string message, Exception? innerException = null,
    HttpStatusCode? upstreamStatusCode = null) : Exception(message, innerException)
{
    public AredlFailure Failure { get; } = failure;
    public HttpStatusCode? UpstreamStatusCode { get; } = upstreamStatusCode;
}
