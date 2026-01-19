namespace BankAccount.Domain.Events;

public abstract record BankAccountEvent
{
    public Guid AccountId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record AccountOpened : BankAccountEvent
{
    public string AccountHolder { get; init; } = string.Empty;
    public decimal InitialBalance { get; init; }

    public AccountOpened() { }

    public AccountOpened(Guid accountId, string accountHolder, decimal initialBalance)
    {
        AccountId = accountId;
        AccountHolder = accountHolder;
        InitialBalance = initialBalance;
        Timestamp = DateTime.UtcNow;
    }
}

public record MoneyDeposited : BankAccountEvent
{
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;

    public MoneyDeposited() { }

    public MoneyDeposited(Guid accountId, decimal amount, string description)
    {
        AccountId = accountId;
        Amount = amount;
        Description = description;
        Timestamp = DateTime.UtcNow;
    }
}

public record MoneyWithdrawn : BankAccountEvent
{
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;

    public MoneyWithdrawn() { }

    public MoneyWithdrawn(Guid accountId, decimal amount, string description)
    {
        AccountId = accountId;
        Amount = amount;
        Description = description;
        Timestamp = DateTime.UtcNow;
    }
}

public record AccountClosed : BankAccountEvent
{
    public string Reason { get; init; } = string.Empty;

    public AccountClosed() { }

    public AccountClosed(Guid accountId, string reason)
    {
        AccountId = accountId;
        Reason = reason;
        Timestamp = DateTime.UtcNow;
    }
}
