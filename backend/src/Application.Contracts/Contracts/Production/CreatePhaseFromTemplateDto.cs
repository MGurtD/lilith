namespace Application.Contracts
{
    public class CreatePhaseFromTemplateDto
    {
        public Guid PhaseTemplateId { get; set; }
        public Guid WorkOrderId { get; set; }
        public Guid WorkcenterTypeId { get; set; }
        public Guid PreferredWorkcenterId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
