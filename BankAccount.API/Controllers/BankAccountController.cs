using BankAccount.API.DTOs;
using BankAccount.API.Handlers;
using BankAccount.Domain.Commands;
using BankAccount.Domain.Events;
using Microsoft.AspNetCore.Mvc;

namespace BankAccount.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountCommandHandler _commandHandler;
    private readonly IBankAccountQueryHandler _queryHandler;
    private readonly ILogger<BankAccountController> _logger;

    public BankAccountController(IBankAccountCommandHandler commandHandler, ILogger<BankAccountController> logger,
        IBankAccountQueryHandler queryHandler)
    {
        _commandHandler = commandHandler;
        _logger = logger;
        _queryHandler = queryHandler;
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountResponse>> CreateAccount(
        [FromBody] CreateAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Starting account creation");
            var accountId = Guid.NewGuid();
            var command = new OpenAccount(accountId, request.AccountHolder, request.InitialBalance);

            await _commandHandler.HandleAsync(command);

            return new CreateAccountResponse(
                accountId,
                "Account created successfully"
            );
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Invalid request for creating account");
            return NotFound(new OperationResponse(false, e.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountResponse>> GetAccount(Guid id)
    {
        try
        {
            _logger.LogInformation("Starting account retrieval");
            var account = await _queryHandler.GetAccountAsync(id);

            return Ok(new AccountResponse(
                account!.Id,
                account.AccountHolder,
                account.Balance,
                account.IsClosed,
                account.Version
            ));
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Invalid request for getting account");
            return NotFound(new OperationResponse(false, e.Message));
        }
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<ActionResult<OperationResponse>> Deposit(
        Guid id,
        [FromBody] DepositRequest request)
    {
        try
        {
            _logger.LogInformation("Depositing {Amount} to account {AccountId}", request.Amount, id);
            var command = new DepositMoney(id, request.Amount, request.Description);
            await _commandHandler.HandleAsync(command);

            return Ok(new OperationResponse(true, $"Deposit of {request.Amount:C} completed successfully"));
        }
        catch (Exception e)
        {
            _logger.LogWarning("Account {AccountId} not found for deposit", id);
            return NotFound(new OperationResponse(false, e.Message));
        }
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<OperationResponse>> Withdraw(
        Guid id,
        [FromBody] WithdrawRequest request)
    {
        try
        {
            _logger.LogInformation("Withdrawal of {Amount} from account {AccountId}", request.Amount, id);
            var command = new WithdrawMoney(id, request.Amount, request.Description);
            await _commandHandler.HandleAsync(command);

            return Ok(new OperationResponse(true, $"Withdrawal of {request.Amount:C} completed successfully"));
        }
        catch (Exception e)
        {
            _logger.LogWarning("Account {AccountId} not found for deposit", id);
            return NotFound(new OperationResponse(false, e.Message));
        }
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<OperationResponse>> CloseAccount(
        Guid id,
        [FromBody] CloseAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Closing account {AccountId}", id);

            var command = new CloseAccount(id, request.Reason);
            await _commandHandler.HandleAsync(command);

            return Ok(new OperationResponse(true, "Account closed successfully"));
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Invalid close operation for account {AccountId}", id);
            return BadRequest(new OperationResponse(false, e.Message));
        }
    }

    [HttpGet("{id:guid}/events")]
    public async Task<ActionResult<List<TransactionResponse>>> GetAccountEvents(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving events for account {AccountId}", id);

            var events = await _queryHandler.GetAccountEventsAsync(id);
            if (events.Count == 0)
                return NotFound(new { message = $"No events found for account {id}" });

            var transactions = events.Select(MapEventToTransaction).ToList();
            return Ok(transactions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving events for account {AccountId}", id);
            return NotFound(new { message = $"Events for account {id} not found" });
        }
    }


    private static TransactionResponse MapEventToTransaction(object evt)
    {
        return evt switch
        {
            AccountOpened e => new TransactionResponse(
                "AccountOpened",
                e.InitialBalance,
                $"Account opened for {e.AccountHolder}",
                e.Timestamp
            ),
            MoneyDeposited e => new TransactionResponse(
                "Deposit",
                e.Amount,
                e.Description,
                e.Timestamp
            ),
            MoneyWithdrawn e => new TransactionResponse(
                "Withdrawal",
                -e.Amount,
                e.Description,
                e.Timestamp
            ),
            AccountClosed e => new TransactionResponse(
                "AccountClosed",
                0,
                e.Reason,
                e.Timestamp
            ),
            _ => new TransactionResponse(
                "Unknown",
                0,
                "Unknown event type",
                DateTime.MinValue
            )
        };
    }
}