using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkMasterController(IWorkMasterService service, IWorkMasterPhaseService phaseService, IMetricsService costsService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(WorkMaster request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
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
        public async Task<IActionResult> GetAll()
        {
            var entities = await service.GetAll();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkMaster request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.Update(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpPost("Copy")]
        public async Task<IActionResult> Copy(WorkMasterCopy request)
        {
            var response = await service.Copy(request);
            if (response.Result)
                return Ok(response);
            else
                return Conflict(response);
        }

        [HttpGet("reference/{id:guid}")]
        public async Task<IActionResult> GetWorkMasterByReferenceId(Guid id)
        {
            var entities = await service.GetByReferenceId(id);
            return Ok(entities);
        }

        [HttpGet("Cost/{id:guid}")]
        public async Task<IActionResult> GetWorkMasterCostById(Guid id, int? quantity)
        {
            var workMaster = await service.GetByIdForCostCalculation(id);
            if (workMaster == null) return NotFound();

            var result = await costsService.GetWorkmasterMetrics(workMaster, quantity);

            if (result.Result && result.Content is ProductionMetrics workMasterCosts)
                return Ok(new GenericResponse(true, workMasterCosts.TotalCost()));
            else
                return NotFound(result);
        }
        [HttpGet("Costs/{id:guid}")]
        public async Task<IActionResult> GetWorkMasterCostsById(Guid id, int? quantity)
        {
            var workMaster = await service.GetByIdForCostCalculation(id);
            if (workMaster == null) return NotFound();

            var result = await costsService.GetWorkmasterMetrics(workMaster, quantity);

            if (result.Result && result.Content is ProductionMetrics workMasterCosts)
                return Ok(new GenericResponse(true, workMasterCosts));
            else
                return NotFound(result);

        }

        #region Phases
        [HttpGet("Phase/{id:guid}")]
        [SwaggerOperation("GetWorkMasterPhaseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseById(Guid id)
        {
            var phase = await phaseService.GetById(id);
            if (phase is not null)
                return Ok(phase);
            else
                return NotFound();
        }

        [HttpPost("Phase")]
        [SwaggerOperation("CreateWorkMasterPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhase(WorkMasterPhase request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.Create(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkMasterPhaseById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhase(Guid Id, WorkMasterPhase request)
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
        [SwaggerOperation("DeleteWorkMasterPhase")]
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
        [SwaggerOperation("GetWorkMasterPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseDetailById(Guid id)
        {
            var detail = await phaseService.GetDetailById(id);
            if (detail is not null)
                return Ok(detail);
            else
                return NotFound();
        }

        [HttpPost("Phase/Detail")]
        [SwaggerOperation("CreateWorkMasterPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkMasterPhaseDetail request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.CreateDetail(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkMasterPhaseDetailById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/Detail/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkMasterPhaseDetail request)
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
        [SwaggerOperation("DeleteWorkMasterPhaseDetail")]
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
        [SwaggerOperation("GetWorkMasterPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseBillOfMaterialsItemById(Guid id)
        {
            var item = await phaseService.GetBillOfMaterialsById(id);
            if (item is not null)
                return Ok(item);
            else
                return NotFound();
        }

        [HttpPost("Phase/BillOfMaterials")]
        [SwaggerOperation("CreateWorkMasterPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkMasterPhaseBillOfMaterials request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await phaseService.CreateBillOfMaterials(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetWorkMasterPhaseBillOfMaterialsItemById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkMasterPhaseBillOfMaterials request)
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
        [SwaggerOperation("DeleteWorkMasterPhaseBillOfMaterials")]
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

    }
}
