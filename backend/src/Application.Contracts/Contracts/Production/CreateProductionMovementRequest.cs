namespace Application.Contracts;

public class CreateProductionMovementRequest : Contract
{
    public Guid WorkOrderId { get; set; }
    public int Quantity { get; set; }
}
