using InvestManage.Application.Investments;
using InvestManage.Contracts.Investments;
using InvestManage.Domain.Investments;
using Microsoft.AspNetCore.Mvc;
using ContractInvestmentType = InvestManage.Contracts.Investments.InvestmentType;
using DomainInvestmentType = InvestManage.Domain.Investments.InvestmentType;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/investment-items")]
public sealed class InvestmentItemsController(InvestmentItemService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvestmentItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<InvestmentItemResponse>>> Search(
        [FromQuery] string? search = null,
        [FromQuery] ContractInvestmentType? type = null,
        [FromQuery] string? currencyCode = null,
        [FromQuery] string? provider = null,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var items = await service.SearchAsync(
            new InvestmentItemSearch(
                search,
                type.HasValue ? (DomainInvestmentType?)(int)type.Value : null,
                currencyCode,
                provider,
                includeArchived),
            cancellationToken);

        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvestmentItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvestmentItemResponse>> GetById(
        Guid id,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, includeArchived, cancellationToken);
        return Ok(ToResponse(item));
    }

    [HttpPost]
    [ProducesResponseType<InvestmentItemResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvestmentItemResponse>> Create(
        CreateInvestmentItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(
            new CreateInvestmentItemCommand(
                request.Code,
                request.Name,
                (DomainInvestmentType)(int)request.Type,
                request.CurrencyCode,
                request.Provider,
                request.PricePrecision,
                request.Notes),
            cancellationToken);
        var response = ToResponse(item);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<InvestmentItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvestmentItemResponse>> Update(
        Guid id,
        UpdateInvestmentItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await service.UpdateAsync(
            id,
            new UpdateInvestmentItemCommand(
                request.Code,
                request.Name,
                (DomainInvestmentType)(int)request.Type,
                request.CurrencyCode,
                request.Provider,
                request.PricePrecision,
                request.Notes),
            cancellationToken);

        return Ok(ToResponse(item));
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

    internal static InvestmentItemResponse ToResponse(InvestmentItem item) =>
        new(
            item.Id,
            item.Code,
            item.Name,
            (ContractInvestmentType)(int)item.Type,
            item.CurrencyCode,
            item.Provider,
            item.PricePrecision,
            item.Notes,
            item.IsArchived);
}
