using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class WorkcenterShiftController(IWorkcenterShiftService workcenterShiftService) : Controller
{
    [HttpGet("{workcenterShiftId:guid}")]
    public async Task<IActionResult> GetWorkcenterShift(Guid workcenterShiftId)
    {
        var workcenterShift = await workcenterShiftService.GetWorkcenterShift(workcenterShiftId);
        if (workcenterShift is not null)
        {
            return Ok(workcenterShift);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("Current")]
    public async Task<IActionResult> GetCurrentWorkcenterShifts()
    {
        var workcenterShifts = await workcenterShiftService.GetCurrentWorkcenterShifts();
        return Ok(workcenterShifts);
    }

    [HttpGet("Currents")]
    public async Task<IActionResult> GetCurrentsWithDetails()
    {
        var workcenterShifts = await workcenterShiftService.GetCurrentsWithDetails();
        if (workcenterShifts.Count == 0)
        {
            return NotFound();
        }
        return Ok(workcenterShifts);
    }

    [HttpPost("CreateWorkcenterShifts")]
    public async Task<IActionResult> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos)
    {
        var response = await workcenterShiftService.CreateWorkcenterShifts(dtos);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("Operator/In")]
    public async Task<IActionResult> OperatorIn(OperatorInOutRequest request)
    {
        var response = await workcenterShiftService.DetailsService.OperatorIn(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    [HttpPost("Operator/Out")]
    public async Task<IActionResult> OperatorOut(OperatorInOutRequest request)
    {
        var response = await workcenterShiftService.DetailsService.OperatorOut(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("Workcenter/ChangeStatus")]
    public async Task<IActionResult> WorkOrderPhaseOut(WorkcenterChangeStatusRequest request)
    {
        var response = await workcenterShiftService.DetailsService.ChangeWorkcenterStatus(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("WorkOrderPhase/In")]
    public async Task<IActionResult> WorkOrderPhaseIn(WorkOrderPhaseOutRequest request)
    {
        var response = await workcenterShiftService.DetailsService.WorkOrderPhaseIn(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("WorkOrderPhaseAndStatus/In")]
    public async Task<IActionResult> WorkOrderPhaseAndStatusIn(WorkOrderPhaseAndStatusInRequest request)
    {
        var response = await workcenterShiftService.DetailsService.WorkOrderPhaseAndStatusIn(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("WorkOrderPhase/Out")]
    public async Task<IActionResult> WorkOrderPhaseOut(WorkOrderPhaseOutRequest request)
    {
        var response = await workcenterShiftService.DetailsService.WorkOrderPhaseOut(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("WorkOrderPhase/Quantities")]
    public async Task<IActionResult> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
    {
        var response = await workcenterShiftService.DetailsService.UpdateWorkcenterShiftDetailQuantities(dto);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("DisableWorkcenterShift/{workcenterShiftId:guid}")]
    public async Task<IActionResult> DisableWorkcenterShift(Guid workcenterShiftId)
    {
        var response = await workcenterShiftService.DisableWorkcenterShift(workcenterShiftId);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("DisableWorkcenterShiftDetail/{workcenterShiftDetailId:guid}")]
    public async Task<IActionResult> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId)
    {
        var response = await workcenterShiftService.DetailsService.DisableWorkcenterShiftDetail(workcenterShiftDetailId);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("Historical")]
    public async Task<IActionResult> GetWorkcenterShiftHistorical([FromBody] WorkcenterShiftHistoricRequest request)
    {
        var response = await workcenterShiftService.GetWorkcenterShiftHistorical(request);
        return Ok(response);
    }


}

