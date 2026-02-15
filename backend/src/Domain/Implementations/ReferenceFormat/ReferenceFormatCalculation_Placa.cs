using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Placa : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            decimal result;
            decimal factor = 0.000001M;
            if ((referenceDimensions.Width > 0) && (referenceDimensions.Length > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                result = referenceDimensions.Width * referenceDimensions.Length * referenceDimensions.Height * referenceDimensions.Density * factor;
            }
            else 
            {
                throw new Exception($"L'amplada, l'alçada, la longitud i la densitat han de ser superiors a 0 per calcular les plaques");
            }            
            return result;
        }
    }
}
