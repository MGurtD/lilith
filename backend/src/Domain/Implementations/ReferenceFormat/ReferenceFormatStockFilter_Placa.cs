namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatStockFilter_Placa : IReferenceFormatStockFilter
    {
        public bool IsCompatible(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness)
        {
            return stockWidth >= bomWidth
                && stockLength >= bomLength
                && stockHeight >= bomHeight;
        }
    }
}
