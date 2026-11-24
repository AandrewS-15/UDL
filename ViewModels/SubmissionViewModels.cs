using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UDL.ViewModels;

public sealed class HttpVideoUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is null || string.IsNullOrWhiteSpace(value.ToString())
        || IsHttpUrl(value.ToString()!);
    public static bool IsHttpUrl(string value) => Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == "https" || uri.Scheme == "http") && !string.IsNullOrEmpty(uri.Host)
        && string.IsNullOrEmpty(uri.UserInfo);
}

public sealed class CreateSubmissionViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un nivel existente.")]
    public int IdNivel { get; set; }
    // No hay campo en la vista. Si un request intenta enviarlo, solo se acepta 100.
    [Required, Range(100, 100, ErrorMessage = "Solo se aceptan completions al 100%."), Display(Name = "Porcentaje")]
    public int? Porcentaje { get; set; } = 100;
    [Required, StringLength(500), HttpVideoUrl(ErrorMessage = "El video debe tener una URL HTTP o HTTPS válida."), Display(Name = "Video URL")]
    public string VideoUrl { get; set; } = "";
    [StringLength(500), HttpVideoUrl(ErrorMessage = "Raw footage debe tener una URL HTTP o HTTPS válida."), Display(Name = "Raw footage URL")]
    public string? RawFootageUrl { get; set; }
    [Range(1, int.MaxValue), Display(Name = "FPS")]
    public int? FPS { get; set; }
    [StringLength(50)] public string? Dispositivo { get; set; }
    [StringLength(500)] public string? Notas { get; set; }
    [BindNever] public SubmissionLevelViewModel? Level { get; set; }
    [BindNever] public string Search { get; set; } = "";
    [BindNever] public List<SubmissionLevelViewModel> Results { get; set; } = [];
}

public sealed record SubmissionLevelViewModel(int IdNivel, string Nombre, int? PosicionAredl, bool RequiresRawFootage)
{
    public bool Available => PosicionAredl > 0;
    public string? Category => PosicionAredl switch { > 0 and <= 75 => "Main", <= 150 and > 75 => "Extended", >= 151 => "Legacy", _ => null };
}

public sealed record SubmissionHistoryItemViewModel(int IdSubmission, int IdNivel, string Nombre, byte Porcentaje,
    string Estado, DateTime FechaEnvio, string VideoUrl, string? MotivoRechazo);
public sealed record SubmissionHistoryViewModel(List<SubmissionHistoryItemViewModel> Items, int Page, bool HasNext);
