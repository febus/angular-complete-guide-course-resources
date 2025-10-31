using Superpower;
using Superpower.Parsers;
using Superpower.Model;
using PersonalDispositionParser.Models;

namespace PersonalDispositionParser.Parsers;

/// <summary>
/// Parser für Personaldispositionsdaten mit Superpower
/// Unterstützt verschiedene Eingabeformate
/// </summary>
public static class DispositionParser
{
    // Basis-Parser für Datum im Format DD.MM.YYYY
    private static readonly TextParser<DateTime> DatumParser =
        from tag in Numerics.IntegerInt32
        from punkt1 in Character.EqualTo('.')
        from monat in Numerics.IntegerInt32
        from punkt2 in Character.EqualTo('.')
        from jahr in Numerics.IntegerInt32
        select new DateTime(jahr, monat, tag);

    // Parser für Zeit im Format HH:MM
    private static readonly TextParser<TimeSpan> ZeitParser =
        from stunde in Numerics.IntegerInt32
        from doppelpunkt in Character.EqualTo(':')
        from minute in Numerics.IntegerInt32
        select new TimeSpan(stunde, minute, 0);

    // Parser für Personalnummer (alphanumerisch)
    private static readonly TextParser<string> PersonalNummerParser =
        from buchstaben in Character.Letter.AtLeastOnce()
        from ziffern in Character.Digit.AtLeastOnce()
        select new string(buchstaben.Concat(ziffern).ToArray());

    // Parser für Text bis zum nächsten Trennzeichen
    private static readonly TextParser<string> TextBisTrennzeichen =
        Character.ExceptIn('|', '\n', '\r').AtLeastOnce().Select(chars => new string(chars).Trim());

    // Parser für Dezimalzahlen
    private static readonly TextParser<decimal> DezimalParser =
        from zahl in Numerics.Decimal
        select zahl;

    // Parser für DispositionsTyp
    private static readonly TextParser<DispositionsTyp> DispositionsTypParser =
        from text in Character.Letter.AtLeastOnce()
        let typText = new string(text).ToUpper()
        select typText switch
        {
            "NORMAL" => DispositionsTyp.Normal,
            "UEBERSTUNDEN" or "UE" => DispositionsTyp.Ueberstunden,
            "NACHT" => DispositionsTyp.Nachtarbeit,
            "WOCHENENDE" or "WE" => DispositionsTyp.Wochenende,
            "FEIERTAG" or "FT" => DispositionsTyp.Feiertag,
            "BEREITSCHAFT" => DispositionsTyp.Bereitschaft,
            "SCHULUNG" => DispositionsTyp.Schulung,
            _ => DispositionsTyp.Normal
        };

    /// <summary>
    /// Parser für eine komplette Dispositionszeile im Format:
    /// DD.MM.YYYY|PN123|HH:MM|HH:MM|Projekt|Tätigkeit|TYP|Zuschlag%
    /// </summary>
    public static readonly TextParser<PersonalDisposition> DispositionsZeileParser =
        from datum in DatumParser
        from sep1 in Character.EqualTo('|')
        from personalNr in TextBisTrennzeichen
        from sep2 in Character.EqualTo('|')
        from startZeit in ZeitParser
        from sep3 in Character.EqualTo('|')
        from endZeit in ZeitParser
        from sep4 in Character.EqualTo('|')
        from projekt in TextBisTrennzeichen
        from sep5 in Character.EqualTo('|')
        from taetigkeit in TextBisTrennzeichen
        from sep6 in Character.EqualTo('|')
        from typ in TextBisTrennzeichen
        from sep7 in Character.EqualTo('|')
        from zuschlag in Numerics.Decimal
        select new PersonalDisposition
        {
            Datum = datum,
            PersonalNummer = personalNr,
            StartZeit = startZeit,
            EndZeit = endZeit,
            Projekt = projekt,
            Taetigkeit = taetigkeit,
            Typ = ParseDispositionsTyp(typ),
            ZuschlagProzent = zuschlag
        };

    /// <summary>
    /// Parst eine einzelne Dispositionszeile
    /// </summary>
    public static Result<PersonalDisposition, string> ParseZeile(string zeile)
    {
        try
        {
            var result = DispositionsZeileParser.TryParse(zeile);
            if (result.HasValue)
            {
                return Result.Success<PersonalDisposition, string>(result.Value);
            }
            else
            {
                return Result.Failure<PersonalDisposition, string>(
                    $"Parsing fehlgeschlagen: {result.ErrorMessage ?? "Unbekannter Fehler"}");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<PersonalDisposition, string>($"Fehler beim Parsen: {ex.Message}");
        }
    }

    /// <summary>
    /// Parst mehrere Dispositionszeilen
    /// </summary>
    public static List<PersonalDisposition> ParseMehrereZeilen(string input)
    {
        var zeilen = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var dispositionen = new List<PersonalDisposition>();

        foreach (var zeile in zeilen)
        {
            if (string.IsNullOrWhiteSpace(zeile) || zeile.StartsWith("#"))
                continue;

            var result = ParseZeile(zeile);
            if (result.IsSuccess)
            {
                dispositionen.Add(result.Value);
            }
        }

        return dispositionen;
    }

    /// <summary>
    /// Parst CSV-Format (komma-separiert)
    /// </summary>
    public static List<PersonalDisposition> ParseCsv(string csvInput)
    {
        var zeilen = csvInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var dispositionen = new List<PersonalDisposition>();

        // Erste Zeile überspringen (Header)
        for (int i = 1; i < zeilen.Length; i++)
        {
            var felder = zeilen[i].Split(',');
            if (felder.Length < 8) continue;

            try
            {
                var disposition = new PersonalDisposition
                {
                    Datum = DateTime.Parse(felder[0]),
                    PersonalNummer = felder[1].Trim(),
                    StartZeit = TimeSpan.Parse(felder[2]),
                    EndZeit = TimeSpan.Parse(felder[3]),
                    Projekt = felder[4].Trim(),
                    Taetigkeit = felder[5].Trim(),
                    Typ = ParseDispositionsTyp(felder[6].Trim()),
                    ZuschlagProzent = decimal.Parse(felder[7])
                };

                dispositionen.Add(disposition);
            }
            catch
            {
                // Fehlerhafte Zeile überspringen
                continue;
            }
        }

        return dispositionen;
    }

    private static DispositionsTyp ParseDispositionsTyp(string typ)
    {
        return typ.ToUpper() switch
        {
            "NORMAL" => DispositionsTyp.Normal,
            "UEBERSTUNDEN" or "UE" => DispositionsTyp.Ueberstunden,
            "NACHT" => DispositionsTyp.Nachtarbeit,
            "WOCHENENDE" or "WE" => DispositionsTyp.Wochenende,
            "FEIERTAG" or "FT" => DispositionsTyp.Feiertag,
            "BEREITSCHAFT" => DispositionsTyp.Bereitschaft,
            "SCHULUNG" => DispositionsTyp.Schulung,
            _ => DispositionsTyp.Normal
        };
    }
}

/// <summary>
/// Hilfsklasse für Result-Pattern
/// </summary>
public class Result<TSuccess, TFailure>
{
    public bool IsSuccess { get; }
    public TSuccess Value { get; }
    public TFailure Error { get; }

    private Result(bool isSuccess, TSuccess value, TFailure error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TSuccess, TFailure> Success(TSuccess value) =>
        new Result<TSuccess, TFailure>(true, value, default!);

    public static Result<TSuccess, TFailure> Failure(TFailure error) =>
        new Result<TSuccess, TFailure>(false, default!, error);
}
