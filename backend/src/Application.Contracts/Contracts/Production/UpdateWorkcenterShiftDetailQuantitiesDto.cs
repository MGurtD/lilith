using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class UpdateWorkcenterShiftDetailQuantitiesDto
{
    [Required(ErrorMessage = "El WorkcenterId es obligatorio.")]
    public Guid WorkcenterId { get; set; }

    [Required(ErrorMessage = "El WorkOrderPhaseId es obligatorio.")]
    public Guid WorkOrderPhaseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Las unidades OK deben ser mayores o iguales a 0.")]
    public decimal QuantityOk { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Las unidades NOK deben ser mayores o iguales a 0.")]
    public decimal QuantityKo { get; set; }
}
