namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatStockFilter_Unitats : IReferenceFormatStockFilter
    {
        public bool IsCompatible(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness)
        {
            // UNITATS: tot l'stock de la referència és vàlid
            return true;
        }
    }
}
