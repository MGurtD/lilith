namespace Domain.Implementations.ReferenceFormat
{
    public struct ReferenceFormatCodes
    {
        public static string TUB = "TUB";
        public static string RODO = "RODO";
        public static string PLACA = "PLACA";
        public static string UNITATS = "UNITATS";
    }

    public static class ReferenceFormatCalculationFactory
    {
        public static IReferenceFormatCalculation Create(string Format)
        {
            if (Format == null) throw new ArgumentNullException(nameof(Format));

            if (Format.Equals(ReferenceFormatCodes.TUB))
                return new ReferenceFormatCalculation_Tub();
            else if (Format.Equals(ReferenceFormatCodes.PLACA))
                return new ReferenceFormatCalculation_Placa();
            else if (Format.Equals(ReferenceFormatCodes.RODO))
                return new ReferenceFormatCalculation_Rodo();
            else if (Format.Equals(ReferenceFormatCodes.UNITATS))
                return new ReferenceFormatCalculation_Unitats();
            else
                throw new NotImplementedException($"ReferenceFormatCalculation for '{Format}' not implemented");
        }
    }
}
