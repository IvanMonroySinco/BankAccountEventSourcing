using BankAccount.Domain.Aggregates;

namespace BankAccount.API.Handlers;

public interface IBankAccountQueryHandler
{
    Task<BankAccountAggregate?> GetAccountAsync(Guid accountId);
    Task<List<object>> GetAccountEventsAsync(Guid accountId);
}