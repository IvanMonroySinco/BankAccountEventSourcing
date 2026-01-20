namespace BankAccount.API.DTOs;

public record CreateAccountRequest(
    string AccountHolder,
    decimal InitialBalance
);

public record DepositRequest(
    decimal Amount,
    string Description
);

public record WithdrawRequest(
    decimal Amount,
    string Description
);

public record CloseAccountRequest(
    string Reason
);

public record AccountResponse(
    Guid Id,
    string AccountHolder,
    decimal Balance,
    bool IsClosed,
    int Version
);

public record TransactionResponse(
    string Type,
    decimal Amount,
    string Description,
    DateTime Timestamp
);

public record AccountDetailsResponse(
    Guid Id,
    string AccountHolder,
    decimal Balance,
    bool IsClosed,
    int Version,
    List<TransactionResponse> RecentTransactions
);

public record CreateAccountResponse(
    Guid AccountId,
    string Message
);

public record OperationResponse(
    bool Success,
    string Message
);