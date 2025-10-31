namespace PersonalDispositionParser.Models;

/// <summary>
/// Repräsentiert eine Personaldisposition (Einsatzplanung)
/// </summary>
public class PersonalDisposition
{
    public int Id { get; set; }
    public string PersonalNummer { get; set; } = string.Empty;
    public DateTime Datum { get; set; }
    public TimeSpan StartZeit { get; set; }
    public TimeSpan EndZeit { get; set; }
    public string Projekt { get; set; } = string.Empty;
    public string Taetigkeit { get; set; } = string.Empty;
    public DispositionsTyp Typ { get; set; }
    public decimal ZuschlagProzent { get; set; }
    public string? Bemerkung { get; set; }

    /// <summary>
    /// Berechnet die Arbeitsstunden
    /// </summary>
    public decimal ArbeitsStunden => (decimal)(EndZeit - StartZeit).TotalHours;

    /// <summary>
    /// Prüft ob die Disposition gültig ist
    /// </summary>
    public bool IstGueltig => EndZeit > StartZeit && !string.IsNullOrEmpty(PersonalNummer);

    public override string ToString()
    {
        return $"{Datum:dd.MM.yyyy} | {PersonalNummer} | {StartZeit:hh\\:mm}-{EndZeit:hh\\:mm} | {Projekt} | {ArbeitsStunden:F2}h";
    }
}

/// <summary>
/// Typ der Disposition
/// </summary>
public enum DispositionsTyp
{
    Normal,
    Ueberstunden,
    Nachtarbeit,
    Wochenende,
    Feiertag,
    Bereitschaft,
    Schulung
}
