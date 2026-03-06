namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatStockFilter_Rodo : IReferenceFormatStockFilter
    {
        /// <summary>
        /// Merma per unitat tallada (mm). Inclou toleràncies de tall.
        /// </summary>
        private const decimal LENGTH_WASTE = 3m;

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

        /// <summary>
        /// Calcula la merma que deixaria una barra d'estoc per la quantitat total del BOM.
        /// Longitud total necessària = bomQuantity * (bomLength + LENGTH_WASTE).
        /// Merma = stockLength - longitudTotal.
        /// Si la barra no pot servir totes les unitats, calcula quantes unitats hi caben.
        /// </summary>
        public decimal? CalculateWaste(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness,
            decimal bomQuantity)
        {
            decimal unitLength = bomLength + LENGTH_WASTE;
            decimal totalNeeded = bomQuantity * unitLength;

            if (stockLength >= totalNeeded)
            {
                // La barra serveix per totes les unitats
                return stockLength - totalNeeded;
            }
            else
            {
                // Calcular quantes unitats hi caben
                int unitsFit = (int)Math.Floor(stockLength / unitLength);
                if (unitsFit > 0)
                {
                    return stockLength - (unitsFit * unitLength);
                }

                // No hi cap cap unitat
                return decimal.MaxValue;
            }
        }
    }
}
