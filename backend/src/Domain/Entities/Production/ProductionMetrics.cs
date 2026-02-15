namespace Domain.Entities.Production
{
    public class ProductionMetrics
    {
        public decimal OperatorCost { get; set; }
        public decimal MachineCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal ExternalTransportCost { get; set; }
        public decimal TotalWeight { get; set; }


        public ProductionMetrics(decimal operatorCost, decimal machineCost, decimal materialCost, decimal externalServiceCost, decimal externalTransportCost, decimal totalWeight)
        {
            OperatorCost = operatorCost;
            MachineCost = machineCost;
            MaterialCost = materialCost;
            ExternalServiceCost = externalServiceCost;
            ExternalTransportCost = externalTransportCost;
            TotalWeight = totalWeight;
        }

        public decimal TotalCost()
        {
            return OperatorCost + MachineCost + MaterialCost + ExternalServiceCost + ExternalTransportCost;
        }
    }
}
