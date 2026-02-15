namespace Domain.Entities.Production
{
    public class ShiftDetail : Entity
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsProductiveTime { get; set; }
        public Guid ShiftId { get; set; }
    }
}
