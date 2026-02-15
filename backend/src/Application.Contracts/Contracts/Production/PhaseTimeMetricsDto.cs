namespace Application.Contracts.Contracts.Production;

/// <summary>
/// DTO containing estimated vs actual time metrics for a work order phase.
/// Used for progress tracking in the plant module.
/// </summary>
public class PhaseTimeMetricsDto
{
    /// <summary>Work Order Phase ID</summary>
    public Guid PhaseId { get; set; }

    /// <summary>Machine Status ID used for filtering actual machine time</summary>
    public Guid MachineStatusId { get; set; }

    /// <summary>Operator ID used for filtering actual operator time (optional)</summary>
    public Guid? OperatorId { get; set; }

    /// <summary>
    /// Estimated machine time in minutes based on phase details.
    /// Calculated from WorkOrderPhaseDetail.EstimatedTime, multiplied by PlannedQuantity if IsCycleTime is true.
    /// Only includes time for details matching the specified MachineStatusId.
    /// </summary>
    public decimal EstimatedMachineTimeMinutes { get; set; }

    /// <summary>
    /// Estimated operator time in minutes based on phase details.
    /// Calculated from WorkOrderPhaseDetail.EstimatedOperatorTime, multiplied by PlannedQuantity if IsCycleTime is true.
    /// Only includes time for details matching the specified MachineStatusId.
    /// </summary>
    public decimal EstimatedOperatorTimeMinutes { get; set; }

    /// <summary>
    /// Actual machine time in minutes accumulated from WorkcenterShiftDetail records.
    /// Filtered by WorkOrderPhaseId and MachineStatusId.
    /// Includes time from StartTime to EndTime (or current time if EndTime is null).
    /// </summary>
    public decimal ActualMachineTimeMinutes { get; set; }

    /// <summary>
    /// Actual operator time in minutes accumulated from WorkcenterShiftDetail records.
    /// Filtered by WorkOrderPhaseId and OperatorId.
    /// Time is divided by ConcurrentOperatorWorkcenters for each record.
    /// Includes time from StartTime to EndTime (or current time if EndTime is null).
    /// </summary>
    public decimal ActualOperatorTimeMinutes { get; set; }

    /// <summary>
    /// Timestamp when actual times were calculated.
    /// Frontend uses this as base to add elapsed seconds locally.
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
