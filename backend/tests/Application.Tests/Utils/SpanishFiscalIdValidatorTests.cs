using Application.Utils;
using Xunit;

namespace Application.Tests.Utils;

/// <summary>
/// Unit tests for <see cref="SpanishFiscalIdValidator"/> — issue #69.
/// Covers NIF, NIE and CIF validation algorithms including control-digit
/// checks. Tests do not call AEAT (the validator only checks format + control).
/// </summary>
public class SpanishFiscalIdValidatorTests
{
    // -------- NIF --------

    [Theory]
    [InlineData("12345678Z")]   // 12345678 % 23 = 14 → 'Z'
    [InlineData("00000000T")]   // 0 % 23 = 0 → 'T'
    [InlineData("00000023T")]   // 23 % 23 = 0 → 'T'
    public void IsValidNif_returns_true_for_valid_nif(string nif)
    {
        Assert.True(SpanishFiscalIdValidator.IsValidNif(nif));
    }

    [Theory]
    [InlineData("12345678A")]   // wrong control letter
    [InlineData("1234567Z")]    // too short
    [InlineData("123456789Z")]  // too long
    [InlineData("12345678")]    // missing letter
    [InlineData("ABCDEFGHZ")]   // non-digits
    public void IsValidNif_returns_false_for_invalid_nif(string nif)
    {
        Assert.False(SpanishFiscalIdValidator.IsValidNif(nif));
    }

    // -------- NIE --------
    // X-prefix → 0, Y → 1, Z → 2; then standard NIF check on the resulting 8 digits.

    [Theory]
    [InlineData("X1234567L")]   // X=0 → 01234567 = 1234567; %23 = 19 → 'L'
    [InlineData("X0000000T")]   // X=0 → 00000000 = 0; %23 = 0 → 'T'
    public void IsValidNie_returns_true_for_valid_nie(string nie)
    {
        Assert.True(SpanishFiscalIdValidator.IsValidNie(nie));
    }

    [Theory]
    [InlineData("A1234567L")]   // wrong prefix (A is CIF, not NIE)
    [InlineData("X123456L")]    // too short
    [InlineData("X12345678L")]  // too long
    [InlineData("12345678L")]   // no prefix
    [InlineData("X1234567D")]   // right shape, wrong control letter (should be 'L' for 1234567)
    public void IsValidNie_returns_false_for_invalid_nie(string nie)
    {
        Assert.False(SpanishFiscalIdValidator.IsValidNie(nie));
    }

    // -------- CIF --------
    // Digit-control types (A, B, E, H, K) → control must be a digit.
    // Letter-control types (C, D, F, G, J, N, P, Q, R, S, U, V, W) → control must be a letter.

    [Theory]
    // A/B/E/H/K use digit control. With digits 1234567 the control is '4'.
    [InlineData("A12345674")]
    [InlineData("B12345674")]
    [InlineData("E12345674")]
    [InlineData("H12345674")]
    [InlineData("K12345674")]
    // Letter-control types. With digits 1234567 the control is 'D' (J=0,A=1,B=2,C=3,D=4).
    [InlineData("P1234567D")]
    [InlineData("S1234567D")]
    [InlineData("G1234567D")]
    [InlineData("Q1234567D")]
    public void IsValidCif_returns_true_for_valid_cif(string cif)
    {
        Assert.True(SpanishFiscalIdValidator.IsValidCif(cif));
    }

    [Theory]
    [InlineData("A12345670")]   // wrong control digit
    [InlineData("A123456")]     // too short
    [InlineData("A1234567890")] // too long
    [InlineData("12345678L")]   // no letter prefix
    [InlineData("IA234567L")]   // 'I' is not a valid CIF prefix
    [InlineData("OA234567L")]   // 'O' is not a valid CIF prefix
    [InlineData("X1234567D")]   // 'X' is NIE-prefix, not a CIF prefix
    public void IsValidCif_returns_false_for_invalid_cif(string cif)
    {
        Assert.False(SpanishFiscalIdValidator.IsValidCif(cif));
    }

    // -------- Aggregate IsValidSpanishFiscalId --------

    [Theory]
    [InlineData("12345678Z")]          // valid NIF
    [InlineData("X1234567L")]          // valid NIE
    [InlineData("A28015865")]          // valid CIF — real AEAT example (Telefónica)
    [InlineData("  12345678Z  ")]      // surrounding whitespace (normalized away)
    [InlineData("12-345-678-Z")]       // dashes (normalized away)
    public void IsValidSpanishFiscalId_returns_true_for_valid_inputs(string id)
    {
        Assert.True(SpanishFiscalIdValidator.IsValidSpanishFiscalId(id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-fiscal-id")]
    [InlineData("12345678")]    // missing letter
    [InlineData("A12345670")]   // wrong CIF control
    public void IsValidSpanishFiscalId_returns_false_for_invalid_inputs(string? id)
    {
        Assert.False(SpanishFiscalIdValidator.IsValidSpanishFiscalId(id));
    }

    // -------- Normalization behaviour --------

    [Theory]
    [InlineData("a28015865")]   // lowercase → uppercased
    [InlineData("A 28015865")] // internal spaces removed
    public void IsValidCif_normalizes_input(string cif)
    {
        Assert.True(SpanishFiscalIdValidator.IsValidCif(cif));
    }
}