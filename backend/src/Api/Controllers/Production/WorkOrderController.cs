using Application.Contracts;
using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController(IWorkOrderService workOrderService, IWorkOrderPhaseService phaseService, IDetailedWorkOrderService detailedWorkOrderService, IWorkOrderReportService reportService) : ControllerBase
    {
        [HttpPost("CreateFromWorkMaster")]
        public async Task<IActionResult> CreateFromWorkMaster([FromBody] CreateWorkOrderDto request)
        {
            var creationResult = await workOrderService.CreateFromWorkMaster(request);
            if (creationResult.Result)
                return Ok(creationResult);
            else
                return BadRequest(creationResult);
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkOrder request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await workOrderService.Create(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpGet]
        public IActionResult GetWorkOrders(DateTime startTime, DateTime endTime, Guid? statusId, Guid? referenceId, Guid? customerId)
        {
            IEnumerable<WorkOrder> workOrders = [];
            workOrders = workOrderService.GetBetweenDatesAndStatus(startTime, endTime, statusId);

            if (referenceId.HasValue)
                workOrders = workOrders.Where(e => e.ReferenceId == referenceId.Value);

            if (customerId.HasValue)
                workOrders = workOrders.Where(e => e.Reference!.CustomerId == customerId.Value);

            return Ok(workOrders.OrderByDescending(e => e.Code));
        }

        [HttpGet("SalesOrder/{id:guid}")]
        public async Task<IActionResult> GetBySalesOrderId(Guid id)
        {
            IEnumerable<WorkOrder> workOrders = await workOrderService.GetBySalesOrderId(id);
            return Ok(workOrders.OrderBy(e => e.Code));
        }

        [HttpPost("Loaded")]
        [SwaggerOperation("GetLoadedWorkOrdersByPhaseIds")]
        [ProducesResponseType(typeof(IEnumerable<WorkOrderWithPhasesDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoadedWorkOrdersByPhaseIds([FromBody] List<Guid> phaseIds)
        {
            var workOrders = await phaseService.GetLoadedWorkOrdersByPhaseIds(phaseIds);
            return Ok(workOrders);
        }

        [HttpPost("Phase/ValidatePreviousQuantity")]
        [SwaggerOperation("ValidatePreviousPhaseQuantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidatePreviousPhaseQuantity(ValidatePreviousPhaseQuantityRequest request)
        {
            var response = await phaseService.ValidatePreviousPhaseQuantity(request);
            if (response is not null)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Gets estimated vs actual time metrics for a work order phase.
        /// Used for progress tracking in the plant module.
        /// </summary>
        /// <param name="phaseId">Work order phase ID</param>
        /// <param name="machineStatusId">Machine status ID to filter phase details and actual machine time</param>
        /// <param name="operatorId">Optional operator ID to filter actual operator time</param>
        /// <returns>Phase time metrics with estimated and actual times</returns>
        [HttpGet("Phase/{phaseId}/TimeMetrics")]
        [SwaggerOperation("GetPhaseTimeMetrics")]
        [ProducesResponseType(typeof(PhaseTimeMetricsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhaseTimeMetrics(Guid phaseId, [FromQuery] Guid machineStatusId, [FromQuery] Guid? operatorId)
        {
            var response = await phaseService.GetPhaseTimeMetrics(phaseId, machineStatusId, operatorId);
            if (response.Result)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound(response);
            }
        }

        [HttpGet("Plannable")]
        [SwaggerOperation("GetPlannableWorkOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlannableWorkOrders()
        {
            var workorders = await workOrderService.GetPlannableWorkOrders();
            return Ok(workorders);
        }
        
        [HttpPost("Priorize")]
        public async Task<IActionResult> Priorize(List<UpdateWorkOrderOrderDTO> orders)
        {
            var response = await workOrderService.Priorize(orders);
            
            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("{id:guid}/Detailed")]
        public IActionResult GetDetailedWorkOrder(Guid id)
        {
            var details = workOrderService.GetWorkOrderDetails(id);
            return Ok(details);
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetReportDataById(Guid id)
        {
            var reportData = await reportService.GetReportById(id);
            return Ok(reportData);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await workOrderService.GetById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkOrder request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            await workOrderService.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await workOrderService.Delete(id);
            if (response.Result)
                return Ok();
            else
                return BadRequest(response);
        }

        #region Phases
        [HttpGet("Phase/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseById(Guid id)
        {
            var phase = await phaseService.GetById(id);
            if (phase is not null)
                return Ok(phase);
            else
                return NotFound();
        }        

        [HttpGet("Phase/External")]
        [SwaggerOperation("GetExternalWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExternalWorkOrderPhase(DateTime startTime, DateTime endTime)
        {
            var phases = await phaseService.GetExternalPhases(startTime, endTime);
            return Ok(phases);
        }

        [HttpGet("Planned/WorkcenterType/{workcenterTypeId:guid}")]
        [SwaggerOperation("GetPlannedByWorkcenterType")]
        [ProducesResponseType(typeof(IEnumerable<WorkOrderWithPhasesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlannedByWorkcenterType(Guid workcenterTypeId)
        {
            var workOrders = await phaseService.GetPlannedByWorkcenterType(workcenterTypeId);
            return Ok(workOrders);
        }

        [HttpGet("{workOrderId:guid}/PhasesDetailed")]
        [SwaggerOperation("GetWorkOrderPhasesDetailed")]
        [ProducesResponseType(typeof(IEnumerable<WorkOrderPhaseDetailedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhasesDetailed(Guid workOrderId)
        {
            var phases = await phaseService.GetWorkOrderPhasesDetailed(workOrderId);            
            return Ok(phases);
        }

        [HttpGet("Phase/{currentPhaseId:guid}/NextForWorkcenter/{workcenterId:guid}")]
        [SwaggerOperation("GetNextPhaseForWorkcenter")]
        [ProducesResponseType(typeof(NextPhaseInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetNextPhaseForWorkcenter(Guid currentPhaseId, Guid workcenterId)
        {
            var nextPhase = await phaseService.GetNextPhaseForWorkcenter(workcenterId, currentPhaseId);
            if (nextPhase == null)
                return NoContent();
            return Ok(nextPhase);
        }


        [HttpPost("Phase")]
        [SwaggerOperation("CreateWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhase(WorkOrderPhase request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.Create(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkOrderPhaseById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhase(Guid Id, WorkOrderPhase request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await phaseService.Update(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("Phase/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhase(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await phaseService.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
        #endregion

        #region PhaseDetails
        [HttpGet("Phase/Detail/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseDetailById(Guid id)
        {
            var detail = await phaseService.GetDetailById(id);
            if (detail is not null)
                return Ok(detail);
            else
                return NotFound();
        }

        [HttpPost("Phase/Detail")]
        [SwaggerOperation("CreateWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkOrderPhaseDetail request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.CreateDetail(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkOrderPhaseDetailById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/Detail/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkOrderPhaseDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await phaseService.UpdateDetail(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("Phase/Detail/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await phaseService.RemoveDetail(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
        #endregion

        #region PhaseBillOfMaterials
        [HttpGet("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseBillOfMaterialsItemById(Guid id)
        {
            var item = await phaseService.GetBillOfMaterialsById(id);
            if (item is not null)
                return Ok(item);
            else
                return NotFound();
        }

        [HttpPost("Phase/BillOfMaterials")]
        [SwaggerOperation("CreateWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkOrderPhaseBillOfMaterials request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.CreateBillOfMaterials(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkOrderPhaseBillOfMaterialsItemById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkOrderPhaseBillOfMaterials request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await phaseService.UpdateBillOfMaterials(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseBillOfMaterials(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await phaseService.RemoveBillOfMaterials(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
        #endregion

        #region DetailedWorkOrder
        [HttpGet("Detailed/ByWorkcenter/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkcenterId(Guid id)
        {
            var entities = detailedWorkOrderService.GetByWorkcenterId(id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrder/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderId(Guid id)
        {
            var entities = detailedWorkOrderService.GetByWorkOrderId(id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrderPhase/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderPhaseId(Guid id)
        {
            var entities = detailedWorkOrderService.GetByWorkOrderPhaseId(id);
            return Ok(entities);
        }
        #endregion
    }
}
