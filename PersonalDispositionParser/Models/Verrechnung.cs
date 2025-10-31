namespace PersonalDispositionParser.Models;

/// <summary>
/// Repräsentiert eine berechnete Verrechnung
/// </summary>
public class Verrechnung
{
    public int Id { get; set; }
    public PersonalDisposition Disposition { get; set; } = null!;
    public Mitarbeiter Mitarbeiter { get; set; } = null!;
    public decimal Stundensatz { get; set; }
    public decimal ArbeitsStunden { get; set; }
    public decimal ZuschlagProzent { get; set; }
    public decimal ZuschlagBetrag { get; set; }
    public decimal GesamtBetrag { get; set; }
    public DateTime BerechnetAm { get; set; }
    public string Berechnungsgrundlage { get; set; } = string.Empty;

    /// <summary>
    /// Berechnet die Verrechnung basierend auf Disposition und Mitarbeiter
    /// </summary>
    public static Verrechnung Berechnen(PersonalDisposition disposition, Mitarbeiter mitarbeiter)
    {
        var stundensatz = mitarbeiter.StundensatzBasis;
        var arbeitsstunden = disposition.ArbeitsStunden;
        var zuschlagProzent = disposition.ZuschlagProzent;

        // Automatische Zuschläge basierend auf Dispositionstyp
        zuschlagProzent += disposition.Typ switch
        {
            DispositionsTyp.Ueberstunden => 25m,
            DispositionsTyp.Nachtarbeit => 50m,
            DispositionsTyp.Wochenende => 50m,
            DispositionsTyp.Feiertag => 100m,
            DispositionsTyp.Bereitschaft => 15m,
            _ => 0m
        };

        var basisBetrag = stundensatz * arbeitsstunden;
        var zuschlagBetrag = basisBetrag * (zuschlagProzent / 100m);
        var gesamtBetrag = basisBetrag + zuschlagBetrag;

        return new Verrechnung
        {
            Disposition = disposition,
            Mitarbeiter = mitarbeiter,
            Stundensatz = stundensatz,
            ArbeitsStunden = arbeitsstunden,
            ZuschlagProzent = zuschlagProzent,
            ZuschlagBetrag = zuschlagBetrag,
            GesamtBetrag = gesamtBetrag,
            BerechnetAm = DateTime.Now,
            Berechnungsgrundlage = $"Basis: {basisBetrag:C2} + Zuschlag {zuschlagProzent}%: {zuschlagBetrag:C2}"
        };
    }

    public override string ToString()
    {
        return $"{Mitarbeiter.VollstaendigerName} | {ArbeitsStunden:F2}h @ {Stundensatz:C2} | Zuschlag: {ZuschlagProzent}% | Gesamt: {GesamtBetrag:C2}";
    }
}
