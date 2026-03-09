namespace Application.Contracts;

public interface IProductionPartGeneratorHandler
{
    Task GenerateFromPhaseClose(GenerateProductionPartsRequest request);
}
