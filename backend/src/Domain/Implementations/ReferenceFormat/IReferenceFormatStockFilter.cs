namespace Domain.Implementations.ReferenceFormat
{
    /// <summary>
    /// Defineix el filtre d'stock per format de referència.
    /// Retorna true si les dimensions de l'stock són compatibles amb les dimensions del BOM.
    /// </summary>
    public interface IReferenceFormatStockFilter
    {
        bool IsCompatible(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness);

        /// <summary>
        /// Calcula la merma que deixaria una peça d'estoc per una quantitat donada del BOM.
        /// Retorna null si no aplica (el format no calcula merma).
        /// </summary>
        decimal? CalculateWaste(
            decimal stockWidth, decimal stockLength, decimal stockHeight,
            decimal stockDiameter, decimal stockThickness,
            decimal bomWidth, decimal bomLength, decimal bomHeight,
            decimal bomDiameter, decimal bomThickness,
            decimal bomQuantity)
        {
            return null;
        }
    }
}
