# 🎯 Schnellstart-Anleitung

## Personal Disposition Verrechnungs Parser & Evaluator mit Superpower

### ✅ Was wurde erstellt?

Ein vollständiges C# Projekt mit:

1. **Parser** - Parst Dispositionsdaten mit Superpower NuGet
2. **Evaluator** - Berechnet Verrechnungen mit intelligenten Zuschlägen
3. **Datenmodelle** - Mitarbeiter, Dispositionen, Verrechnungen
4. **Beispiele** - Vollständige Implementierung mit Testdaten

---

## 📁 Projektstruktur

```
PersonalDispositionParser/
│
├── 📄 PersonalDispositionParser.csproj    # Projekt-Konfiguration
├── 📄 Program.cs                          # Hauptprogramm mit Beispielen
├── 📄 README.md                           # Ausführliche Dokumentation
├── 📄 ANLEITUNG.md                        # Diese Datei
│
├── 📂 Models/
│   ├── Mitarbeiter.cs                     # Mitarbeiter-Stammdaten
│   ├── PersonalDisposition.cs             # Dispositionsdaten & Typen
│   └── Verrechnung.cs                     # Verrechnungslogik
│
├── 📂 Parsers/
│   └── DispositionParser.cs               # Parser mit Superpower
│
├── 📂 Evaluators/
│   └── VerrechnungsEvaluator.cs           # Verrechnungs-Engine
│
└── 📂 Beispieldaten/
    ├── beispiel-dispositionen.txt         # Pipe-Format
    └── beispiel-dispositionen.csv         # CSV-Format
```

---

## 🚀 Projekt ausführen

### Voraussetzungen
- .NET 8.0 SDK oder höher
- Visual Studio, VS Code oder Rider (optional)

### Kompilieren und Ausführen

```bash
# In das Projektverzeichnis wechseln
cd PersonalDispositionParser

# NuGet-Pakete wiederherstellen
dotnet restore

# Projekt kompilieren
dotnet build

# Projekt ausführen
dotnet run
```

### Erwartete Ausgabe
Das Programm zeigt:
- ✅ Geladene Mitarbeiter
- ✅ Geparste Dispositionen (Pipe-Format)
- ✅ Geparste Dispositionen (CSV-Format)
- ✅ Berechnete Verrechnungen mit Zuschlägen
- ✅ Detaillierter Verrechnungsbericht

---

## 🎓 Superpower Features im Einsatz

### 1. Datum-Parser
```csharp
// Parst: 31.10.2025
private static readonly TextParser<DateTime> DatumParser =
    from tag in Numerics.IntegerInt32
    from punkt1 in Character.EqualTo('.')
    from monat in Numerics.IntegerInt32
    from punkt2 in Character.EqualTo('.')
    from jahr in Numerics.IntegerInt32
    select new DateTime(jahr, monat, tag);
```

### 2. Zeit-Parser
```csharp
// Parst: 08:30
private static readonly TextParser<TimeSpan> ZeitParser =
    from stunde in Numerics.IntegerInt32
    from doppelpunkt in Character.EqualTo(':')
    from minute in Numerics.IntegerInt32
    select new TimeSpan(stunde, minute, 0);
```

### 3. Kompletter Dispositions-Parser
```csharp
// Parst: 31.10.2025|MA001|08:00|17:00|Projekt|Tätigkeit|NORMAL|0
public static readonly TextParser<PersonalDisposition> DispositionsZeileParser =
    from datum in DatumParser
    from sep1 in Character.EqualTo('|')
    from personalNr in TextBisTrennzeichen
    // ... weitere Felder
    select new PersonalDisposition { /* ... */ };
```

---

## 💡 Verwendungsbeispiele

### Beispiel 1: Einzelne Zeile parsen

```csharp
var zeile = "31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0";
var result = DispositionParser.ParseZeile(zeile);

if (result.IsSuccess)
{
    Console.WriteLine($"Erfolgreich: {result.Value}");
}
else
{
    Console.WriteLine($"Fehler: {result.Error}");
}
```

### Beispiel 2: Mehrere Zeilen parsen

```csharp
var input = @"
31.10.2025|MA001|08:00|17:00|Projekt Alpha|Entwicklung|NORMAL|0
31.10.2025|MA002|22:00|06:00|Projekt Beta|Support|NACHT|0
";

var dispositionen = DispositionParser.ParseMehrereZeilen(input);
Console.WriteLine($"{dispositionen.Count} Dispositionen geparst");
```

### Beispiel 3: Verrechnung berechnen

```csharp
var evaluator = new VerrechnungsEvaluator();
evaluator.FuegeMitarbeiterHinzu(mitarbeiter);

var verrechnung = evaluator.EvaluiereDisposition(disposition);
Console.WriteLine($"Betrag: {verrechnung.GesamtBetrag:C2}");
```

### Beispiel 4: Benutzerdefinierte Regel

```csharp
evaluator.FuegeRegelHinzu(new VerrechnungsRegel
{
    Name = "Projekt-Bonus",
    Beschreibung = "15% Bonus für VIP-Projekte",
    IstAnwendbar = (disp, ma) => disp.Projekt.Contains("VIP"),
    ZusaetzlicherZuschlag = 15m
});
```

---

## 📊 Dispositionstypen & Zuschläge

| Typ | Kürzel | Beschreibung | Auto-Zuschlag |
|-----|--------|--------------|---------------|
| NORMAL | - | Normale Arbeitszeit | 0% |
| UEBERSTUNDEN | UE | Überstunden | +25% |
| NACHT | - | Nachtarbeit (22-06 Uhr) | +50% |
| WOCHENENDE | WE | Wochenendarbeit | +50% |
| FEIERTAG | FT | Feiertagsarbeit | +100% |
| BEREITSCHAFT | - | Bereitschaftsdienst | +15% |
| SCHULUNG | - | Schulung/Training | 0% |

---

## 🔧 Anpassungen & Erweiterungen

### Eigene Dispositionstypen hinzufügen

1. Erweitern Sie das `DispositionsTyp` Enum in `Models/PersonalDisposition.cs`
2. Aktualisieren Sie die Zuschlagslogik in `Models/Verrechnung.cs`
3. Erweitern Sie den Parser in `Parsers/DispositionParser.cs`

### Neue Eingabeformate unterstützen

Erstellen Sie neue Parser-Methoden in `DispositionParser.cs`:

```csharp
public static List<PersonalDisposition> ParseJson(string jsonInput)
{
    // JSON-Parsing-Logik
}
```

### Zusätzliche Verrechnungsregeln

```csharp
evaluator.FuegeRegelHinzu(new VerrechnungsRegel
{
    Name = "Ihre Regel",
    Beschreibung = "Beschreibung",
    IstAnwendbar = (disposition, mitarbeiter) => 
    {
        // Ihre Bedingung
        return true;
    },
    ZusaetzlicherZuschlag = 20m
});
```

---

## 🐛 Fehlerbehandlung

Das Projekt verwendet das **Result-Pattern** für robuste Fehlerbehandlung:

```csharp
var result = DispositionParser.ParseZeile(zeile);

if (result.IsSuccess)
{
    // Erfolg - verwende result.Value
    var disposition = result.Value;
}
else
{
    // Fehler - verwende result.Error
    Console.WriteLine($"Fehler: {result.Error}");
}
```

---

## 📚 Weitere Informationen

- **README.md** - Ausführliche Dokumentation
- **Program.cs** - Vollständige Beispiel-Implementierung
- **beispiel-dispositionen.txt** - Testdaten im Pipe-Format
- **beispiel-dispositionen.csv** - Testdaten im CSV-Format

---

## 🎯 Nächste Schritte

1. ✅ Projekt ausführen: `dotnet run`
2. ✅ Code in `Program.cs` anschauen
3. ✅ Eigene Testdaten erstellen
4. ✅ Benutzerdefinierte Regeln hinzufügen
5. ✅ Neue Features implementieren

---

## 💬 Superpower Vorteile

✅ **Typsicher** - Compiler-geprüfte Parser
✅ **Komposierbar** - Parser aus kleineren Parsern zusammensetzen
✅ **LINQ-Syntax** - Intuitive `from...select` Syntax
✅ **Fehlerbehandlung** - Eingebaute Fehlerbehandlung
✅ **Performance** - Optimiert für Geschwindigkeit
✅ **Lesbar** - Selbstdokumentierender Code

---

## 🎉 Viel Erfolg!

Bei Fragen oder Problemen schauen Sie in die README.md oder den Quellcode.
Alle Komponenten sind ausführlich kommentiert und dokumentiert.
