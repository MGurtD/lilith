namespace Application.Contracts;

public interface IWorkOrderPhaseCloseHandler
{
    Task HandlePhaseClose(WorkOrderPhaseCloseRequest request);
}
