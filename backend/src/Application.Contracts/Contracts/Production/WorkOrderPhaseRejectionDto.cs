using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class WorkOrderPhaseRejectionDto
{
    [Required(ErrorMessage = "El RejectionReasonId es obligatorio.")]
    public Guid RejectionReasonId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades del motivo de rechazo deben ser mayores que 0.")]
    public decimal Quantity { get; set; }
}

public class RegisterWorkOrderPhaseRejectionsDto
{
    [Required(ErrorMessage = "El WorkcenterId es obligatorio.")]
    public Guid WorkcenterId { get; set; }

    [Required(ErrorMessage = "El WorkOrderPhaseId es obligatorio.")]
    public Guid WorkOrderPhaseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Las unidades NOK deben ser mayores o iguales a 0.")]
    public decimal QuantityKo { get; set; }

    public List<WorkOrderPhaseRejectionDto> Rejections { get; set; } = [];
}

public class WorkOrderPhaseRejectionDisplayDto
{
    public Guid Id { get; set; }
    public Guid WorkOrderPhaseId { get; set; }
    public Guid RejectionReasonId { get; set; }
    public string RejectionReasonCode { get; set; } = string.Empty;
    public string RejectionReasonName { get; set; } = string.Empty;
    public string RejectionReasonColor { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime CreatedOn { get; set; }
}
