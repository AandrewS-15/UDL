using System.ComponentModel.DataAnnotations;
using UDL.ViewModels;

namespace UDL.Services.Submissions;

public static class SubmissionRules
{
    public static List<ValidationResult> Validate(CreateSubmissionViewModel input, SubmissionLevelViewModel? level)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), errors, validateAllProperties: true);
        if (level is null) errors.Add(new("El nivel seleccionado no existe.", [nameof(input.IdNivel)]));
        else
        {
            if (!level.Available) errors.Add(new("Este nivel no está disponible actualmente para envío de records.", [nameof(input.IdNivel)]));
            if (level.RequiresRawFootage && string.IsNullOrWhiteSpace(input.RawFootageUrl))
                errors.Add(new("Este nivel requiere Raw Footage.", [nameof(input.RawFootageUrl)]));
        }
        // Notas y dispositivo son texto plano: no se acepta marcado ni se renderiza HTML.
        if (input.Notas?.IndexOfAny(['<', '>']) >= 0) errors.Add(new("Las notas deben ser texto plano, sin HTML.", [nameof(input.Notas)]));
        if (input.Dispositivo?.IndexOfAny(['<', '>']) >= 0) errors.Add(new("El dispositivo debe ser texto plano.", [nameof(input.Dispositivo)]));
        return errors;
    }
}
