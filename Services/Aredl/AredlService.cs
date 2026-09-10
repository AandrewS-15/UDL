using System.Text.Json;
using UDL.Dtos.Aredl;

namespace UDL.Services.Aredl;

public sealed class AredlService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        RespectNullableAnnotations = true
    };

    public async Task<IReadOnlyList<AredlLevelDto>> GetLevelsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Relativa a /v2/api/: no anteponer '/' ni repetir 'api/'.
            using var response = await httpClient.GetAsync("aredl/levels", cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new AredlException(AredlFailure.Http,
                    "AREDL devolvió un error HTTP.", upstreamStatusCode: response.StatusCode);

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                throw new AredlException(AredlFailure.InvalidResponse,
                    "AREDL no devolvió contenido application/json.");

            var levels = await response.Content.ReadFromJsonAsync<List<AredlLevelDto>>(
                JsonOptions, cancellationToken);
            if (levels is null || levels.Any(level => level is null
                || level.Id == Guid.Empty || level.PublisherId == Guid.Empty
                || string.IsNullOrWhiteSpace(level.Name) || string.IsNullOrWhiteSpace(level.Status)))
                throw new AredlException(AredlFailure.InvalidResponse,
                    "AREDL devolvió una lista de niveles inválida.");

            return levels;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AredlException(AredlFailure.Timeout, "AREDL excedió el tiempo de espera.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AredlException(AredlFailure.Network, "No se pudo conectar con AREDL.", ex);
        }
        catch (JsonException ex)
        {
            throw new AredlException(AredlFailure.InvalidResponse,
                "El JSON de AREDL no coincide con el contrato esperado.", ex);
        }
    }
}
