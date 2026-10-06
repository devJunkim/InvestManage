using InvestManage.Application.Accounts;
using InvestManage.Contracts.Accounts;
using InvestManage.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;
using ContractAccountType = InvestManage.Contracts.Accounts.InvestmentAccountType;
using DomainAccountType = InvestManage.Domain.Accounts.InvestmentAccountType;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/investment-accounts")]
public sealed class InvestmentAccountsController(InvestmentAccountService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvestmentAccountResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<InvestmentAccountResponse>>> List(
        [FromQuery] Guid? userId = null,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var accounts = await service.ListAsync(userId, includeArchived, cancellationToken);
        return Ok(accounts.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvestmentAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvestmentAccountResponse>> GetById(
        Guid id,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var account = await service.GetAsync(id, includeArchived, cancellationToken);
        return Ok(ToResponse(account));
    }

    [HttpPost]
    [ProducesResponseType<InvestmentAccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvestmentAccountResponse>> Create(
        CreateInvestmentAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await service.CreateAsync(
            new CreateInvestmentAccountCommand(
                request.UserId,
                request.Name,
                (DomainAccountType)(int)request.Type,
                request.CurrencyCode),
            cancellationToken);
        var response = ToResponse(account);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<InvestmentAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvestmentAccountResponse>> Update(
        Guid id,
        UpdateInvestmentAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await service.UpdateAsync(
            id,
            new UpdateInvestmentAccountCommand(
                request.Name,
                (DomainAccountType)(int)request.Type,
                request.CurrencyCode),
            cancellationToken);

        return Ok(ToResponse(account));
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await service.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        await service.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }

    private static InvestmentAccountResponse ToResponse(InvestmentAccount account) =>
        new(
            account.Id,
            account.UserId,
            account.Name,
            (ContractAccountType)(int)account.Type,
            account.CurrencyCode,
            account.IsArchived);
}
