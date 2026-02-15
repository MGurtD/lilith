using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class ValidatePreviousPhaseQuantityRequest
{
    [Required(ErrorMessage = "El WorkOrderPhaseId es obligatorio.")]
    public Guid WorkOrderPhaseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores o iguales a 0.")]
    public decimal Quantity { get; set; }
}
