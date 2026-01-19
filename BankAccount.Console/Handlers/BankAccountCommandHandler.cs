using BankAccount.Domain.Aggregates;
using BankAccount.Domain.Commands;
using Marten;

namespace BankAccount.Console.Handlers;

public class BankAccountCommandHandler
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

        var account = await session.Events.AggregateStreamAsync<BankAccountAggregate>(command.AccountId);

        if (account == null)
            throw new InvalidOperationException($"Account {command.AccountId} not found");

        account.Deposit(command.Amount, command.Description);
        session.Events.Append(
            command.AccountId,
            account.UncommittedEvents.ToArray<object>()
        );

        await session.SaveChangesAsync();
    }

    public async Task HandleAsync(WithdrawMoney command)
    {
        await using var session = _store.LightweightSession();

        var account = await session.Events.AggregateStreamAsync<BankAccountAggregate>(command.AccountId);

        if (account == null)
            throw new InvalidOperationException($"Account {command.AccountId} not found");

        account.Withdraw(command.Amount, command.Description);
        session.Events.Append(
            command.AccountId,
            account.UncommittedEvents.ToArray<object>()
        );

        await session.SaveChangesAsync();
    }

    public async Task HandleAsync(CloseAccount command)
    {
        await using var session = _store.LightweightSession();

        var account = await session.Events.AggregateStreamAsync<BankAccountAggregate>(command.AccountId);

        if (account == null)
            throw new InvalidOperationException($"Account {command.AccountId} not found");

        account.Close(command.Reason);
        session.Events.Append(
            command.AccountId,
            account.UncommittedEvents.ToArray<object>()
        );

        await session.SaveChangesAsync();
    }
}
