namespace PersonalDispositionParser.Models;

/// <summary>
/// Repräsentiert einen Mitarbeiter mit Stammdaten
/// </summary>
public class Mitarbeiter
{
    public int Id { get; set; }
    public string PersonalNummer { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string Abteilung { get; set; } = string.Empty;
    public decimal StundensatzBasis { get; set; }
    public string Qualifikation { get; set; } = string.Empty;

    public string VollstaendigerName => $"{Vorname} {Nachname}";

    public override string ToString()
    {
        return $"{PersonalNummer}: {VollstaendigerName} ({Abteilung})";
    }
}
