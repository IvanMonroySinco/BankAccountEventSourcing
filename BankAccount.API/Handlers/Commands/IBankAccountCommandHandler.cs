using BankAccount.Domain.Commands;

namespace BankAccount.API.Handlers;

public interface IBankAccountCommandHandler
{
    Task<Guid> HandleAsync(OpenAccount command);
    Task HandleAsync(DepositMoney command);
    Task HandleAsync(WithdrawMoney command);
    Task HandleAsync(CloseAccount command);
}