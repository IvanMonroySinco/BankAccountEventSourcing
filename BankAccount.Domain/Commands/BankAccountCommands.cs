namespace BankAccount.Domain.Commands;

public record OpenAccount(
    Guid AccountId,
    string AccountHolder,
    decimal InitialBalance
);

public record DepositMoney(
    Guid AccountId,
    decimal Amount,
    string Description
);

public record WithdrawMoney(
    Guid AccountId,
    decimal Amount,
    string Description
);

public record CloseAccount(
    Guid AccountId,
    string Reason
);