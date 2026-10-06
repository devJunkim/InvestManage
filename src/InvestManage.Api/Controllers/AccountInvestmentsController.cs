using InvestManage.Application.Investments;
using InvestManage.Contracts.Investments;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/investment-accounts/{accountId:guid}/investments")]
public sealed class AccountInvestmentsController(InvestmentItemService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvestmentItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<InvestmentItemResponse>>> List(
        Guid accountId,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var items = await service.ListForAccountAsync(
            accountId,
            includeArchived,
            cancellationToken);
        return Ok(items.Select(InvestmentItemsController.ToResponse).ToList());
    }

    [HttpPost("{investmentItemId:guid}")]
    [ProducesResponseType<InvestmentAssignmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<InvestmentAssignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvestmentAssignmentResponse>> Assign(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken)
    {
        var result = await service.AssignToAccountAsync(
            accountId,
            investmentItemId,
            cancellationToken);
        var response = new InvestmentAssignmentResponse(
            result.Assignment.Id,
            result.Assignment.InvestmentAccountId,
            result.Assignment.InvestmentItemId);

        return result.Created
            ? CreatedAtAction(nameof(List), new { accountId }, response)
            : Ok(response);
    }
}
