using PersonalDispositionParser.Models;

namespace PersonalDispositionParser.Evaluators;

/// <summary>
/// Evaluator für Verrechnungslogik mit erweiterten Berechnungsfunktionen
/// </summary>
public class VerrechnungsEvaluator
{
    private readonly Dictionary<string, Mitarbeiter> _mitarbeiterCache;
    private readonly List<VerrechnungsRegel> _regeln;

    public VerrechnungsEvaluator()
    {
        _mitarbeiterCache = new Dictionary<string, Mitarbeiter>();
        _regeln = new List<VerrechnungsRegel>();
        InitialisiereStandardRegeln();
    }

    /// <summary>
    /// Fügt einen Mitarbeiter zum Cache hinzu
    /// </summary>
    public void FuegeMitarbeiterHinzu(Mitarbeiter mitarbeiter)
    {
        _mitarbeiterCache[mitarbeiter.PersonalNummer] = mitarbeiter;
    }

    /// <summary>
    /// Fügt eine benutzerdefinierte Verrechnungsregel hinzu
    /// </summary>
    public void FuegeRegelHinzu(VerrechnungsRegel regel)
    {
        _regeln.Add(regel);
    }

    /// <summary>
    /// Evaluiert eine einzelne Disposition und erstellt eine Verrechnung
    /// </summary>
    public Verrechnung? EvaluiereDisposition(PersonalDisposition disposition)
    {
        if (!disposition.IstGueltig)
        {
            Console.WriteLine($"⚠️  Ungültige Disposition: {disposition}");
            return null;
        }

        if (!_mitarbeiterCache.TryGetValue(disposition.PersonalNummer, out var mitarbeiter))
        {
            Console.WriteLine($"⚠️  Mitarbeiter nicht gefunden: {disposition.PersonalNummer}");
            return null;
        }

        // Wende benutzerdefinierte Regeln an
        var zuschlagProzent = disposition.ZuschlagProzent;
        foreach (var regel in _regeln)
        {
            if (regel.IstAnwendbar(disposition, mitarbeiter))
            {
                zuschlagProzent += regel.ZusaetzlicherZuschlag;
            }
        }

        // Aktualisiere Zuschlag in Disposition
        disposition.ZuschlagProzent = zuschlagProzent;

        // Berechne Verrechnung
        return Verrechnung.Berechnen(disposition, mitarbeiter);
    }

    /// <summary>
    /// Evaluiert mehrere Dispositionen
    /// </summary>
    public List<Verrechnung> EvaluiereMehrereDispositionen(List<PersonalDisposition> dispositionen)
    {
        var verrechnungen = new List<Verrechnung>();

        foreach (var disposition in dispositionen)
        {
            var verrechnung = EvaluiereDisposition(disposition);
            if (verrechnung != null)
            {
                verrechnungen.Add(verrechnung);
            }
        }

        return verrechnungen;
    }

    /// <summary>
    /// Erstellt einen Verrechnungsbericht
    /// </summary>
    public VerrechnungsBericht ErstelleBericht(List<Verrechnung> verrechnungen)
    {
        var bericht = new VerrechnungsBericht
        {
            ErstelltAm = DateTime.Now,
            AnzahlVerrechnungen = verrechnungen.Count,
            GesamtStunden = verrechnungen.Sum(v => v.ArbeitsStunden),
            GesamtBetrag = verrechnungen.Sum(v => v.GesamtBetrag),
            GesamtZuschlaege = verrechnungen.Sum(v => v.ZuschlagBetrag)
        };

        // Gruppiere nach Mitarbeiter
        bericht.VerrechnungenProMitarbeiter = verrechnungen
            .GroupBy(v => v.Mitarbeiter.PersonalNummer)
            .ToDictionary(
                g => g.Key,
                g => new MitarbeiterVerrechnung
                {
                    Mitarbeiter = g.First().Mitarbeiter,
                    AnzahlDispositionen = g.Count(),
                    GesamtStunden = g.Sum(v => v.ArbeitsStunden),
                    GesamtBetrag = g.Sum(v => v.GesamtBetrag),
                    Verrechnungen = g.ToList()
                }
            );

        // Gruppiere nach Projekt
        bericht.VerrechnungenProProjekt = verrechnungen
            .GroupBy(v => v.Disposition.Projekt)
            .ToDictionary(
                g => g.Key,
                g => new ProjektVerrechnung
                {
                    Projekt = g.Key,
                    AnzahlDispositionen = g.Count(),
                    GesamtStunden = g.Sum(v => v.ArbeitsStunden),
                    GesamtBetrag = g.Sum(v => v.GesamtBetrag)
                }
            );

        return bericht;
    }

    /// <summary>
    /// Initialisiert Standard-Verrechnungsregeln
    /// </summary>
    private void InitialisiereStandardRegeln()
    {
        // Regel: Nachtarbeit zwischen 22:00 und 06:00
        _regeln.Add(new VerrechnungsRegel
        {
            Name = "Nachtarbeit-Automatik",
            Beschreibung = "Automatischer Nachtzuschlag für Arbeit zwischen 22:00 und 06:00",
            IstAnwendbar = (disp, ma) =>
            {
                var nachtStart = new TimeSpan(22, 0, 0);
                var nachtEnde = new TimeSpan(6, 0, 0);
                return disp.StartZeit >= nachtStart || disp.EndZeit <= nachtEnde;
            },
            ZusaetzlicherZuschlag = 0m // Bereits in DispositionsTyp berücksichtigt
        });

        // Regel: Qualifikationszuschlag für Spezialisten
        _regeln.Add(new VerrechnungsRegel
        {
            Name = "Qualifikationszuschlag",
            Beschreibung = "Zusätzlicher Zuschlag für hochqualifizierte Mitarbeiter",
            IstAnwendbar = (disp, ma) =>
                ma.Qualifikation.Contains("Senior") || ma.Qualifikation.Contains("Experte"),
            ZusaetzlicherZuschlag = 10m
        });
    }
}

/// <summary>
/// Repräsentiert eine Verrechnungsregel
/// </summary>
public class VerrechnungsRegel
{
    public string Name { get; set; } = string.Empty;
    public string Beschreibung { get; set; } = string.Empty;
    public Func<PersonalDisposition, Mitarbeiter, bool> IstAnwendbar { get; set; } = (_, _) => false;
    public decimal ZusaetzlicherZuschlag { get; set; }
}

/// <summary>
/// Verrechnungsbericht mit Zusammenfassungen
/// </summary>
public class VerrechnungsBericht
{
    public DateTime ErstelltAm { get; set; }
    public int AnzahlVerrechnungen { get; set; }
    public decimal GesamtStunden { get; set; }
    public decimal GesamtBetrag { get; set; }
    public decimal GesamtZuschlaege { get; set; }
    public Dictionary<string, MitarbeiterVerrechnung> VerrechnungenProMitarbeiter { get; set; } = new();
    public Dictionary<string, ProjektVerrechnung> VerrechnungenProProjekt { get; set; } = new();

    public void DruckeBericht()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("VERRECHNUNGSBERICHT");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"Erstellt am: {ErstelltAm:dd.MM.yyyy HH:mm:ss}");
        Console.WriteLine($"Anzahl Verrechnungen: {AnzahlVerrechnungen}");
        Console.WriteLine($"Gesamt Stunden: {GesamtStunden:F2}h");
        Console.WriteLine($"Gesamt Zuschläge: {GesamtZuschlaege:C2}");
        Console.WriteLine($"GESAMTBETRAG: {GesamtBetrag:C2}");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📊 VERRECHNUNG PRO MITARBEITER:");
        Console.WriteLine(new string('-', 80));
        foreach (var (personalNr, mv) in VerrechnungenProMitarbeiter)
        {
            Console.WriteLine($"\n{mv.Mitarbeiter.VollstaendigerName} ({personalNr}):");
            Console.WriteLine($"  Dispositionen: {mv.AnzahlDispositionen}");
            Console.WriteLine($"  Stunden: {mv.GesamtStunden:F2}h");
            Console.WriteLine($"  Betrag: {mv.GesamtBetrag:C2}");
        }

        Console.WriteLine("\n\n📁 VERRECHNUNG PRO PROJEKT:");
        Console.WriteLine(new string('-', 80));
        foreach (var (projekt, pv) in VerrechnungenProProjekt)
        {
            Console.WriteLine($"\n{projekt}:");
            Console.WriteLine($"  Dispositionen: {pv.AnzahlDispositionen}");
            Console.WriteLine($"  Stunden: {pv.GesamtStunden:F2}h");
            Console.WriteLine($"  Betrag: {pv.GesamtBetrag:C2}");
        }

        Console.WriteLine("\n" + new string('=', 80));
    }
}

public class MitarbeiterVerrechnung
{
    public Mitarbeiter Mitarbeiter { get; set; } = null!;
    public int AnzahlDispositionen { get; set; }
    public decimal GesamtStunden { get; set; }
    public decimal GesamtBetrag { get; set; }
    public List<Verrechnung> Verrechnungen { get; set; } = new();
}

public class ProjektVerrechnung
{
    public string Projekt { get; set; } = string.Empty;
    public int AnzahlDispositionen { get; set; }
    public decimal GesamtStunden { get; set; }
    public decimal GesamtBetrag { get; set; }
}
