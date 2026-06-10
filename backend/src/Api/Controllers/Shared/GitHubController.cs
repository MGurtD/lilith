using Application.Contracts.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
public class GitHubController(IGitHubProxyService service) : ControllerBase
{
    [HttpPost("draft-issues")]
    public async Task<IActionResult> CreateDraftIssue(CreateDraftIssueRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await service.CreateDraftIssue(request);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }
}
