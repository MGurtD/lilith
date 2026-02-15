using Domain.Entities.Shared;

namespace Domain.Entities.Warehouse
{
    public class Stock : Entity
    {
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set;}
        public Guid LocationId {get; set;}
        public Location? Location {get; set;}
        public int Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }
    }
}