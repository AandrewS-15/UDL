using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UDL.Services.Import;

public static class ImportNames
{
    public static string Clean(string name) => string.Concat(name.EnumerateRunes()
        .Where(r => Rune.GetUnicodeCategory(r) != UnicodeCategory.Format).Select(r => r.ToString()))
        .Trim().Normalize(NormalizationForm.FormC);
    public static string Username(string name) => Clean(name).ToUpperInvariant();
    // Solo Victors: quitar un único punto final, nunca puntos internos o del nombre canónico.
    public static string Victor(string name)
    {
        var key = Username(name);
        return key.EndsWith('.') ? key[..^1].TrimEnd() : key;
    }
    // La capitalización distingue identidades: Erebus != ErebuS.
    public static string Level(string name) => Regex.Replace(Clean(name), @"\s+", " ");
    public static bool HasInvisibleCharacters(string name) => name.Any(c =>
        char.GetUnicodeCategory(c) is UnicodeCategory.Format or UnicodeCategory.Control);

    // Exclusivamente para sugerencias manuales; nunca es una clave de vinculación.
    public static string Suggestion(string name)
    {
        var visible = new string(name.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.Format).ToArray());
        return Level(Regex.Replace(visible, @"\s*\([^)]*\)\s*$", "")).TrimEnd('.');
    }
}
