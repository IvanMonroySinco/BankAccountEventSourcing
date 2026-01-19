using BankAccount.Domain.Events;

namespace BankAccount.Domain.Aggregates;

public class BankAccountAggregate
{
    public Guid Id { get; private set; }
    public string AccountHolder { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public bool IsClosed { get; private set; }
    public int Version { get; private set; }
    
    private readonly List<BankAccountEvent> _uncommittedEvents = [];
    public IReadOnlyList<BankAccountEvent> UncommittedEvents => _uncommittedEvents;

    public BankAccountAggregate() { }
    
    public static BankAccountAggregate Open(Guid accountId, string accountHolder, decimal initialBalance)
    {
        if (AccountHolderIsEmpty(accountHolder))
            throw new ArgumentException("Account holder cannot be empty");

        if (InitialBalanceLessThanZero(initialBalance))
            throw new ArgumentException("Initial balance cannot be negative");

        var account = new BankAccountAggregate();
        var @event = new AccountOpened(accountId, accountHolder, initialBalance);
        
        account.Apply(@event); 
        account._uncommittedEvents.Add(@event);
        
        return account;
    }

    private static bool InitialBalanceLessThanZero(decimal initialBalance) => 
        initialBalance < 0;

    private static bool AccountHolderIsEmpty(string accountHolder) => 
        string.IsNullOrWhiteSpace(accountHolder);

    public void Deposit(decimal amount, string description)
    {
        if (IsClosed)
            throw new InvalidOperationException("Cannot deposit to a closed account");

        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");

        var @event = new MoneyDeposited(Id, amount, description);
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }
    
    public void Withdraw(decimal amount, string description)
    {
        if (IsClosed)
            throw new InvalidOperationException("Cannot withdraw from a closed account");

        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");

        if (Balance < amount)
            throw new InvalidOperationException($"Insufficient funds. Current balance: {Balance}");

        var @event = new MoneyWithdrawn(Id, amount, description);
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }

    public void Close(string reason)
    {
        if (IsClosed)
            throw new InvalidOperationException("Account is already closed");

        var @event = new AccountClosed(Id, reason);
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }
    
    public void Apply(AccountOpened @event)
    {
        Id = @event.AccountId;
        AccountHolder = @event.AccountHolder;
        Balance = @event.InitialBalance;
        Version++;
    }

    public void Apply(MoneyDeposited @event)
    {
        Balance += @event.Amount;
        Version++;
    }

    public void Apply(MoneyWithdrawn @event)
    {
        Balance -= @event.Amount;
        Version++;
    }

    public void Apply(AccountClosed @event)
    {
        IsClosed = true;
        Version++;
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }
    
    
}