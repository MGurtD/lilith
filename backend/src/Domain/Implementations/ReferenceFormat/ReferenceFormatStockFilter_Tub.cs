namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatStockFilter_Tub : IReferenceFormatStockFilter
    {
        public bool IsCompatible(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness)
        {
            // Diàmetre ha de ser igual o superior al del BOM
            // Gruix (thickness) ha de ser igual o inferior al del BOM
            // Longitud ha de ser com a mínim la longitud d'una unitat del BOM
            return stockDiameter >= bomDiameter
                && stockThickness <= bomThickness
                && stockLength >= bomLength;
        }
    }
}
