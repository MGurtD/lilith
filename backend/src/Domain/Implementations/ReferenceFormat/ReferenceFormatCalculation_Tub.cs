using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Tub : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            double result, radiInt, diamExt;
            double factor = 0.000001;
            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Thickness > 0) && (referenceDimensions.Length > 0) && (referenceDimensions.Density > 0))
            {
                diamExt = (double) referenceDimensions.Diameter;
                radiInt = (double) ((referenceDimensions.Diameter/2) - referenceDimensions.Thickness);
                var length = (double) referenceDimensions.Length;
                var density = (double) referenceDimensions.Density;

                result = ((Math.Pow(diamExt / 2, 2) - Math.Pow(radiInt, 2)) * Math.PI * length * density)*factor;
            } 
            else 
            {
                throw new Exception($"El diàmetre, el gruix, l'alçada i la densitat han de ser superiors a 0 per calcular el format tub");
            }            
            return (decimal) result;
        }
    }
}
