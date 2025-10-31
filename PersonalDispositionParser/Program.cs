using PersonalDispositionParser.Models;
using PersonalDispositionParser.Parsers;
using PersonalDispositionParser.Evaluators;

namespace PersonalDispositionParser;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     PERSONAL DISPOSITION VERRECHNUNGS PARSER & EVALUATOR                   ║");
        Console.WriteLine("║     Mit Superpower NuGet Package                                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝\n");

        // 1. Mitarbeiter erstellen
        Console.WriteLine("📋 SCHRITT 1: Mitarbeiter-Stammdaten laden\n");
        var mitarbeiter = ErstelleMitarbeiter();
        foreach (var ma in mitarbeiter)
        {
            Console.WriteLine($"  ✓ {ma}");
        }

        // 2. Evaluator initialisieren
        Console.WriteLine("\n⚙️  SCHRITT 2: Verrechnungs-Evaluator initialisieren\n");
        var evaluator = new VerrechnungsEvaluator();
        foreach (var ma in mitarbeiter)
        {
            evaluator.FuegeMitarbeiterHinzu(ma);
        }

        // Benutzerdefinierte Regel hinzufügen
        evaluator.FuegeRegelHinzu(new VerrechnungsRegel
        {
            Name = "Projekt-Premium-Zuschlag",
            Beschreibung = "10% Zuschlag für Premium-Projekte",
            IstAnwendbar = (disp, ma) => disp.Projekt.Contains("Premium"),
            ZusaetzlicherZuschlag = 10m
        });
        Console.WriteLine("  ✓ Standard-Regeln geladen");
        Console.WriteLine("  ✓ Benutzerdefinierte Regel 'Projekt-Premium-Zuschlag' hinzugefügt");

        // 3. Dispositionsdaten parsen
        Console.WriteLine("\n📄 SCHRITT 3: Dispositionsdaten parsen (mit Superpower)\n");
        
        // Beispiel 1: Pipe-separiertes Format
        var dispositionsInput = @"
31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0
31.10.2025|MA002|09:00|18:00|Premium Projekt Beta|Beratung|NORMAL|0
31.10.2025|MA003|22:00|06:00|Projekt Gamma|Support|NACHT|0
01.11.2025|MA001|10:00|14:00|Projekt Alpha|Meeting|WE|0
01.11.2025|MA002|08:00|20:00|Premium Projekt Beta|Entwicklung|UE|0
";

        var dispositionen = DispositionParser.ParseMehrereZeilen(dispositionsInput);
        Console.WriteLine($"  ✓ {dispositionen.Count} Dispositionen erfolgreich geparst:");
        foreach (var disp in dispositionen)
        {
            Console.WriteLine($"    • {disp}");
        }

        // Beispiel 2: Einzelne Zeile parsen mit Fehlerbehandlung
        Console.WriteLine("\n📝 SCHRITT 4: Einzelne Zeile parsen (mit Fehlerbehandlung)\n");
        var testZeile = "02.11.2025|MA003|14:00|18:00|Projekt Delta|Schulung|SCHULUNG|5";
        var parseResult = DispositionParser.ParseZeile(testZeile);
        
        if (parseResult.IsSuccess)
        {
            Console.WriteLine($"  ✓ Erfolgreich geparst: {parseResult.Value}");
            dispositionen.Add(parseResult.Value);
        }
        else
        {
            Console.WriteLine($"  ✗ Fehler: {parseResult.Error}");
        }

        // 4. Verrechnungen evaluieren
        Console.WriteLine("\n💰 SCHRITT 5: Verrechnungen evaluieren\n");
        var verrechnungen = evaluator.EvaluiereMehrereDispositionen(dispositionen);
        
        Console.WriteLine($"  ✓ {verrechnungen.Count} Verrechnungen erstellt:\n");
        foreach (var verrechnung in verrechnungen)
        {
            Console.WriteLine($"    {verrechnung}");
            Console.WriteLine($"      └─ {verrechnung.Berechnungsgrundlage}");
        }

        // 5. Bericht erstellen
        Console.WriteLine("\n📊 SCHRITT 6: Verrechnungsbericht erstellen");
        var bericht = evaluator.ErstelleBericht(verrechnungen);
        bericht.DruckeBericht();

        // 6. CSV-Format demonstrieren
        Console.WriteLine("\n\n📑 BONUS: CSV-Format parsen\n");
        var csvInput = @"Datum,PersonalNr,Start,Ende,Projekt,Tätigkeit,Typ,Zuschlag
03.11.2025,MA001,08:00,16:00,Projekt Epsilon,Entwicklung,NORMAL,0
03.11.2025,MA002,18:00,02:00,Projekt Zeta,Wartung,NACHT,0";

        var csvDispositionen = DispositionParser.ParseCsv(csvInput);
        Console.WriteLine($"  ✓ {csvDispositionen.Count} Dispositionen aus CSV geparst:");
        foreach (var disp in csvDispositionen)
        {
            Console.WriteLine($"    • {disp}");
        }

        // CSV-Dispositionen auch verrechnen
        var csvVerrechnungen = evaluator.EvaluiereMehrereDispositionen(csvDispositionen);
        Console.WriteLine($"\n  ✓ {csvVerrechnungen.Count} CSV-Verrechnungen erstellt:");
        foreach (var verrechnung in csvVerrechnungen)
        {
            Console.WriteLine($"    {verrechnung}");
        }

        Console.WriteLine("\n\n✅ ALLE SCHRITTE ERFOLGREICH ABGESCHLOSSEN!");
        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine("ZUSAMMENFASSUNG:");
        Console.WriteLine($"  • Mitarbeiter: {mitarbeiter.Count}");
        Console.WriteLine($"  • Dispositionen (Pipe-Format): {dispositionen.Count}");
        Console.WriteLine($"  • Dispositionen (CSV-Format): {csvDispositionen.Count}");
        Console.WriteLine($"  • Gesamt Verrechnungen: {verrechnungen.Count + csvVerrechnungen.Count}");
        Console.WriteLine($"  • Gesamt Betrag: {(verrechnungen.Sum(v => v.GesamtBetrag) + csvVerrechnungen.Sum(v => v.GesamtBetrag)):C2}");
        Console.WriteLine(new string('═', 80));
    }

    static List<Mitarbeiter> ErstelleMitarbeiter()
    {
        return new List<Mitarbeiter>
        {
            new Mitarbeiter
            {
                Id = 1,
                PersonalNummer = "MA001",
                Vorname = "Max",
                Nachname = "Mustermann",
                Abteilung = "IT-Entwicklung",
                StundensatzBasis = 85.00m,
                Qualifikation = "Senior Developer"
            },
            new Mitarbeiter
            {
                Id = 2,
                PersonalNummer = "MA002",
                Vorname = "Anna",
                Nachname = "Schmidt",
                Abteilung = "Consulting",
                StundensatzBasis = 95.00m,
                Qualifikation = "Experte Consultant"
            },
            new Mitarbeiter
            {
                Id = 3,
                PersonalNummer = "MA003",
                Vorname = "Thomas",
                Nachname = "Weber",
                Abteilung = "Support",
                StundensatzBasis = 65.00m,
                Qualifikation = "Support Specialist"
            }
        };
    }
}
