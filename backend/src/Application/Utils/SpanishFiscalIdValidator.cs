using System.Text.RegularExpressions;

namespace Application.Utils;

/// <summary>
/// Validates Spanish fiscal identifiers (NIF, NIE, CIF) using the official algorithms.
/// Performs format and check-digit validation only — does not call AEAT.
/// </summary>
public static class SpanishFiscalIdValidator
{
    private const string NifPattern = @"^[0-9]{8}[A-Z]$";
    private const string NiePattern = @"^[XYZ][0-9]{7}[A-Z]$";
    private const string CifPattern = @"^[ABCDEFGHJKLMNPQRSUVW][0-9]{7}[0-9A-J]$";

    private static readonly char[] DniLetters =
        "TRWAGMYFPDXBNJZSQVHLCKE".ToCharArray();

    private static readonly Dictionary<char, string> CifControlLetters = new()
    {
        { 'A', "JABCDEFGHI" },
        { 'B', "JABCDEFGHI" },
        { 'E', "JABCDEFGHI" },
        { 'H', "JABCDEFGHI" },
        { 'K', "JABCDEFGHI" },
        { 'L', "JABCDEFGHI" },
        { 'M', "JABCDEFGHI" },
        { 'Q', "JABCDEFGHI" },
        { 'S', "JABCDEFGHI" },
        { 'C', "JABCDEFGHI" },
        { 'G', "JABCDEFGHI" },
        { 'N', "JABCDEFGHI" },
        { 'P', "JABCDEFGHI" },
        { 'R', "JABCDEFGHI" },
        { 'D', "JABCDEFGHI" },
        { 'F', "JABCDEFGHI" },
        { 'J', "JABCDEFGHI" },
        { 'U', "JABCDEFGHI" },
        { 'V', "JABCDEFGHI" },
        { 'W', "JABCDEFGHI" },
    };

    public static bool IsValidSpanishFiscalId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = Normalize(value);
        if (string.IsNullOrEmpty(normalized)) return false;

        return IsValidNif(normalized)
            || IsValidNie(normalized)
            || IsValidCif(normalized);
    }

    public static bool IsValidNif(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = Normalize(value);
        if (!Regex.IsMatch(v, NifPattern)) return false;

        var numberPart = int.Parse(v.Substring(0, 8));
        var expectedLetter = DniLetters[numberPart % 23];
        return v[8] == expectedLetter;
    }

    public static bool IsValidNie(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = Normalize(value);
        if (!Regex.IsMatch(v, NiePattern)) return false;

        var prefix = v[0] switch
        {
            'X' => '0',
            'Y' => '1',
            'Z' => '2',
            _ => v[0],
        };
        var numberPart = int.Parse($"{prefix}{v.Substring(1, 7)}");
        var expectedLetter = DniLetters[numberPart % 23];
        return v[8] == expectedLetter;
    }

    public static bool IsValidCif(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = Normalize(value);
        if (!Regex.IsMatch(v, CifPattern)) return false;

        var firstLetter = v[0];
        var digits = v.Substring(1, 7);
        var providedControl = v[8];

        var sumEven = 0;
        for (var i = 0; i < digits.Length; i += 2)
        {
            var doubled = (digits[i] - '0') * 2;
            sumEven += doubled / 10 + doubled % 10;
        }

        var sumOdd = 0;
        for (var i = 1; i < digits.Length; i += 2)
        {
            sumOdd += digits[i] - '0';
        }

        var total = sumEven + sumOdd;
        var controlDigit = (10 - (total % 10)) % 10;

        if (firstLetter is 'A' or 'B' or 'E' or 'H' or 'K')
        {
            return providedControl == (char)('0' + controlDigit);
        }

        if (CifControlLetters.TryGetValue(firstLetter, out var letters))
        {
            return providedControl == letters[controlDigit];
        }

        return providedControl == (char)('0' + controlDigit)
            || CifControlLetters.TryGetValue(firstLetter, out var bothLetters)
                && bothLetters[controlDigit] == providedControl;
    }

    private static string Normalize(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        return trimmed;
    }
}
