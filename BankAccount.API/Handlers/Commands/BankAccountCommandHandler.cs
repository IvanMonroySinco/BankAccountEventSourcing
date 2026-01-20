using BankAccount.Domain.Aggregates;
using BankAccount.Domain.Commands;
using Marten;

namespace BankAccount.API.Handlers;

public class BankAccountCommandHandler : IBankAccountCommandHandler
{
    private readonly IDocumentStore _store;

    public BankAccountCommandHandler(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Guid> HandleAsync(OpenAccount command)
    {
        await using var session = _store.LightweightSession();

        var account = BankAccountAggregate.Open(
            command.AccountId,
            command.AccountHolder,
            command.InitialBalance
        );

        session.Events.StartStream<BankAccountAggregate>(
            command.AccountId,
            account.UncommittedEvents.ToArray()
        );
        await session.SaveChangesAsync();

        return command.AccountId;
    }

    public async Task HandleAsync(DepositMoney command)
    {
        await using var session = _store.LightweightSession();

        var account = await GetStreamByAccountId(command.AccountId, session);
        ThrowExceptionWhenAccountNotFound(command.AccountId, account);

        account?.Deposit(command.Amount, command.Description);
        AppendEvent(command.AccountId, session, account);

        await session.SaveChangesAsync();
    }

    public async Task HandleAsync(WithdrawMoney command)
    {
        await using var session = _store.LightweightSession();

        var account = await GetStreamByAccountId(command.AccountId, session);
        ThrowExceptionWhenAccountNotFound(command.AccountId, account);

        account?.Withdraw(command.Amount, command.Description);
        AppendEvent(command.AccountId, session, account);

        await session.SaveChangesAsync();
    }

    public async Task HandleAsync(CloseAccount command)
    {
        await using var session = _store.LightweightSession();

        var account = await GetStreamByAccountId(command.AccountId, session);
        ThrowExceptionWhenAccountNotFound(command.AccountId, account);

        account?.Close(command.Reason);
        AppendEvent(command.AccountId, session, account);
        
        await session.SaveChangesAsync();
    }

    private static void AppendEvent(Guid accountId, IDocumentSession session, BankAccountAggregate? account) =>
        session.Events.Append(
            accountId,
            account?.UncommittedEvents.ToArray<object>() ?? []
        );

    private static void ThrowExceptionWhenAccountNotFound(Guid accountId, BankAccountAggregate? account)
    {
        if (account == null)
            throw new InvalidOperationException($"Account {accountId} not found");
    }

    private static async Task<BankAccountAggregate?> GetStreamByAccountId(Guid accountId, IDocumentSession session) => 
        await session.Events.AggregateStreamAsync<BankAccountAggregate>(accountId);
}
