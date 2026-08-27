namespace Application.Contracts
{
    public class CreateWorkOrderDto
    {
        public Guid WorkMasterId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Comment { get; set; } = string.Empty;

        // Codi de lot de sortida opcional, usat només quan Production.AutoBatch = false
        public string? LotCode { get; set; }
}
}
