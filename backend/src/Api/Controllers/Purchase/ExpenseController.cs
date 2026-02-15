using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController(IExpenseService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Expenses request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [HttpGet]
        public IActionResult GetBetweenDatesAndType(DateTime startTime, DateTime endTime, Guid? expenseTypeId)
        {
            var entities = service.GetBetweenDatesAndType(startTime, endTime, expenseTypeId);
            return Ok(entities);
        }

        [Route("ExpenseType/{id:guid}")]
        [HttpGet]        
        public IActionResult GetByType(Guid id)
        {
            var expenses = service.GetByType(id);
            return Ok(expenses);
        }

        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetAllConsolidation(DateTime startTime, DateTime endTime, string? type = "", string? typeDetail = "")
        {
            var entities = service.GetConsolidatedBetweenDates(startTime, endTime);
            
            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(typeDetail))
                entities = entities.Where(c => c.Type == type && c.TypeDetail == typeDetail);
            else if (!string.IsNullOrWhiteSpace(type))
                entities = entities.Where(c => c.Type == type);
            
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetExpenseById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Expenses request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.UpdateExpense(request);
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

            var response = await service.RemoveExpense(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

    }
}
