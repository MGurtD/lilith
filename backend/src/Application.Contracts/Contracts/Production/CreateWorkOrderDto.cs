namespace Application.Contracts
{
    public class CreateWorkOrderDto
    {
        public Guid WorkMasterId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Comment { get; set; } = string.Empty;
}
}
