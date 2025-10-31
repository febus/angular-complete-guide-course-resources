# Personal Disposition Verrechnungs Parser & Evaluator

Ein leistungsstarker C# Parser und Evaluator für Personaldispositionsdaten mit automatischer Verrechnungslogik, entwickelt mit dem **Superpower** NuGet-Package.

## 🚀 Features

- **Flexibles Parsing**: Unterstützt mehrere Eingabeformate (Pipe-separiert, CSV)
- **Superpower Integration**: Nutzt die Superpower-Bibliothek für robustes Parsing
- **Automatische Zuschlagsberechnung**: Intelligente Berechnung von Zuschlägen basierend auf:
  - Dispositionstyp (Überstunden, Nachtarbeit, Wochenende, Feiertag)
  - Mitarbeiterqualifikation
  - Projekttyp
  - Benutzerdefinierten Regeln
- **Umfassende Berichte**: Detaillierte Verrechnungsberichte pro Mitarbeiter und Projekt
- **Fehlerbehandlung**: Robuste Fehlerbehandlung mit Result-Pattern

## 📦 Abhängigkeiten

```xml
<PackageReference Include="Superpower" Version="3.0.0" />
```

## 🏗️ Projektstruktur

```
PersonalDispositionParser/
├── Models/
│   ├── Mitarbeiter.cs              # Mitarbeiter-Stammdaten
│   ├── PersonalDisposition.cs      # Dispositionsdaten
│   └── Verrechnung.cs              # Verrechnungsergebnisse
├── Parsers/
│   └── DispositionParser.cs        # Parser mit Superpower
├── Evaluators/
│   └── VerrechnungsEvaluator.cs    # Verrechnungslogik
├── Program.cs                       # Hauptprogramm mit Beispielen
└── PersonalDispositionParser.csproj
```

## 📝 Eingabeformate

### Pipe-separiertes Format

```
DD.MM.YYYY|PersonalNr|HH:MM|HH:MM|Projekt|Tätigkeit|TYP|Zuschlag%
```

**Beispiel:**
```
31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0
31.10.2025|MA002|22:00|06:00|Projekt Beta|Support|NACHT|0
```

### CSV-Format

```csv
Datum,PersonalNr,Start,Ende,Projekt,Tätigkeit,Typ,Zuschlag
31.10.2025,MA001,08:00,16:00,Projekt Alpha,Entwicklung,NORMAL,0
```

## 🎯 Dispositionstypen

| Typ | Beschreibung | Auto-Zuschlag |
|-----|--------------|---------------|
| `NORMAL` | Normale Arbeitszeit | 0% |
| `UEBERSTUNDEN` / `UE` | Überstunden | +25% |
| `NACHT` | Nachtarbeit (22:00-06:00) | +50% |
| `WOCHENENDE` / `WE` | Wochenendarbeit | +50% |
| `FEIERTAG` / `FT` | Feiertagsarbeit | +100% |
| `BEREITSCHAFT` | Bereitschaftsdienst | +15% |
| `SCHULUNG` | Schulung/Training | 0% |

## 💻 Verwendung

### Basis-Beispiel

```csharp
using PersonalDispositionParser.Models;
using PersonalDispositionParser.Parsers;
using PersonalDispositionParser.Evaluators;

// 1. Mitarbeiter erstellen
var mitarbeiter = new Mitarbeiter
{
    PersonalNummer = "MA001",
    Vorname = "Max",
    Nachname = "Mustermann",
    StundensatzBasis = 85.00m,
    Qualifikation = "Senior Developer"
};

// 2. Evaluator initialisieren
var evaluator = new VerrechnungsEvaluator();
evaluator.FuegeMitarbeiterHinzu(mitarbeiter);

// 3. Disposition parsen
var zeile = "31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0";
var result = DispositionParser.ParseZeile(zeile);

if (result.IsSuccess)
{
    // 4. Verrechnung evaluieren
    var verrechnung = evaluator.EvaluiereDisposition(result.Value);
    Console.WriteLine(verrechnung);
}
```

### Mehrere Dispositionen parsen

```csharp
var input = @"
31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0
31.10.2025|MA002|22:00|06:00|Projekt Beta|Support|NACHT|0
";

var dispositionen = DispositionParser.ParseMehrereZeilen(input);
var verrechnungen = evaluator.EvaluiereMehrereDispositionen(dispositionen);
```

### CSV-Format parsen

```csharp
var csvInput = @"Datum,PersonalNr,Start,Ende,Projekt,Tätigkeit,Typ,Zuschlag
31.10.2025,MA001,08:00,16:00,Projekt Alpha,Entwicklung,NORMAL,0";

var dispositionen = DispositionParser.ParseCsv(csvInput);
```

### Benutzerdefinierte Regeln

```csharp
evaluator.FuegeRegelHinzu(new VerrechnungsRegel
{
    Name = "Premium-Projekt-Zuschlag",
    Beschreibung = "10% Zuschlag für Premium-Projekte",
    IstAnwendbar = (disp, ma) => disp.Projekt.Contains("Premium"),
    ZusaetzlicherZuschlag = 10m
});
```

### Bericht erstellen

```csharp
var bericht = evaluator.ErstelleBericht(verrechnungen);
bericht.DruckeBericht();
```

## 🔧 Kompilieren und Ausführen

```bash
# Projekt kompilieren
dotnet build

# Projekt ausführen
dotnet run

# Mit Release-Konfiguration
dotnet run --configuration Release
```

## 📊 Beispiel-Output

```
╔════════════════════════════════════════════════════════════════════════════╗
║     PERSONAL DISPOSITION VERRECHNUNGS PARSER & EVALUATOR                   ║
║     Mit Superpower NuGet Package                                           ║
╚════════════════════════════════════════════════════════════════════════════╝

📋 SCHRITT 1: Mitarbeiter-Stammdaten laden

  ✓ MA001: Max Mustermann (IT-Entwicklung)
  ✓ MA002: Anna Schmidt (Consulting)
  ✓ MA003: Thomas Weber (Support)

💰 SCHRITT 5: Verrechnungen evaluieren

  ✓ 6 Verrechnungen erstellt:

    Max Mustermann | 9.00h @ €85.00 | Zuschlag: 0% | Gesamt: €765.00
      └─ Basis: €765.00 + Zuschlag 0%: €0.00
    Anna Schmidt | 9.00h @ €95.00 | Zuschlag: 20% | Gesamt: €1,026.00
      └─ Basis: €855.00 + Zuschlag 20%: €171.00

================================================================================
VERRECHNUNGSBERICHT
================================================================================
Erstellt am: 31.10.2025 14:30:00
Anzahl Verrechnungen: 6
Gesamt Stunden: 52.00h
Gesamt Zuschläge: €1,234.50
GESAMTBETRAG: €5,678.90
================================================================================
```

## 🎓 Superpower Features

Dieses Projekt nutzt folgende Superpower-Features:

- **TextParser**: Typsichere Parser-Kombinatoren
- **Numerics**: Parser für Zahlen (Integer, Decimal)
- **Character**: Parser für Zeichen und Zeichenketten
- **Parser-Kombinatoren**: `from...select` LINQ-Syntax für Parser-Komposition
- **Fehlerbehandlung**: Robuste Fehlerbehandlung mit `TryParse`

## 🔍 Erweiterte Features

### Automatische Nachtarbeitserkennung
Der Parser erkennt automatisch Nachtarbeit zwischen 22:00 und 06:00 Uhr.

### Qualifikationszuschläge
Mitarbeiter mit "Senior" oder "Experte" in der Qualifikation erhalten automatisch 10% Zuschlag.

### Projekt-basierte Zuschläge
Projekte mit "Premium" im Namen können automatisch höhere Zuschläge erhalten.

## 📄 Lizenz

Dieses Projekt ist ein Beispiel für die Verwendung von Superpower in C#.

## 🤝 Beiträge

Erweiterungen und Verbesserungen sind willkommen!

## 📚 Weitere Ressourcen

- [Superpower GitHub](https://github.com/datalust/superpower)
- [Superpower Dokumentation](https://github.com/datalust/superpower/wiki)
