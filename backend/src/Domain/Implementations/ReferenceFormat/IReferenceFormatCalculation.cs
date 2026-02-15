using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public interface IReferenceFormatCalculation
    {
        decimal Calculate(ReferenceDimensions referenceDimensions);
    }
}
