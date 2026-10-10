using InvestManage.Application.Transactions;
using InvestManage.Contracts.Transactions;
using Microsoft.AspNetCore.Mvc;
using ContractTransactionType = InvestManage.Contracts.Transactions.TransactionType;
using DomainTransactionType = InvestManage.Domain.Transactions.TransactionType;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/investment-accounts/{accountId:guid}/investments/{investmentItemId:guid}/transactions")]
public sealed class TransactionsController(TransactionService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Create(
        Guid accountId,
        Guid investmentItemId,
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(
            new CreateTransactionCommand(
                accountId,
                investmentItemId,
                (DomainTransactionType)(int)request.Type,
                request.TradeDate,
                request.SettlementDate,
                request.Quantity,
                request.UnitPrice,
                request.Fees,
                request.CurrencyCode,
                request.Notes),
            cancellationToken);
        var transaction = result.Transaction;
        var response = new TransactionResponse(
            transaction.Id,
            result.AccountId,
            result.InvestmentItemId,
            (ContractTransactionType)(int)transaction.Type,
            transaction.TradeDate,
            transaction.SettlementDate,
            transaction.Quantity,
            transaction.UnitPrice,
            transaction.Fees,
            transaction.CurrencyCode,
            transaction.Notes,
            transaction.CreatedAtUtc);

        return Created($"api/v1/transactions/{transaction.Id:D}", response);
    }
}
