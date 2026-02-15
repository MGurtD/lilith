namespace Domain.Entities.Production
{
    public class WorkcenterCost : Entity
    {
        public Guid WorkcenterId { get; set; }
        public Workcenter? Workcenter { get; set; }
        public Guid MachineStatusId { get; set; }
        public MachineStatus? MachineStatus { get; set; }

        public decimal Cost { get; set; }
    }
}
