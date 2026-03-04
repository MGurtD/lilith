namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatStockFilter_Rodo : IReferenceFormatStockFilter
    {
        public bool IsCompatible(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness)
        {
            // Diàmetre ha de ser igual o superior al del BOM
            // Longitud ha de ser com a mínim la longitud d'una unitat del BOM
            return stockDiameter >= bomDiameter
                && stockLength >= bomLength;
        }
    }
}
