using System.Text.Json;
using BankAccount.Console.Handlers;
using BankAccount.Console.Infrastructure;
using BankAccount.Domain.Commands;

var connectionString = "Host=localhost;Database=bankaccount_db;Username=postgres;Password=postgres";
var store = MartenConfiguration.ConfigureDocumentStore(connectionString);

var serviceCommand = new BankAccountCommandHandler(store);
var serviceQuery = new BankAccountQueryHandler(store);

Console.WriteLine("=== Bank Account Event Sourcing Started ===");

try
{
// 1. Crear una cuenta
    var accountId = Guid.NewGuid();
    Console.WriteLine("📝 Creando cuenta para Ivan con saldo inicial de $1000...");
            
    await serviceCommand.HandleAsync(new OpenAccount(
        accountId,
        "Ivan Monroy",
        1000m
    ));
    Console.WriteLine($"✅ Cuenta creada: {accountId}\n");

    // 2. Depositar dinero
    Console.WriteLine("💵 Depositando $500...");
    await serviceCommand.HandleAsync(new DepositMoney(
        accountId,
        500m,
        "Salario mensual"
    ));
    Console.WriteLine("✅ Depósito realizado\n");

    // 3. Retirar dinero
    Console.WriteLine("💸 Retirando $300...");
    await serviceCommand.HandleAsync(new WithdrawMoney(
        accountId,
        300m,
        "Pago de arriendo"
    ));
    Console.WriteLine("✅ Retiro realizado\n");

    // 4. Otro depósito
    Console.WriteLine("💵 Depositando $200...");
    await serviceCommand.HandleAsync(new DepositMoney(
        accountId,
        200m,
        "Freelance project"
    ));
    Console.WriteLine("✅ Depósito realizado\n");

    var account = await serviceQuery.GetAccountAsync(accountId);
    Console.WriteLine("📊 Estado actual de la cuenta:");
    Console.WriteLine($"   Titular: {account!.AccountHolder}");
    Console.WriteLine($"   Saldo: ${account.Balance}");
    Console.WriteLine($"   Versión: {account.Version}");
    Console.WriteLine($"   Cerrada: {account.IsClosed}\n");

    // 6. Ver historial completo de eventos
    var events = await serviceQuery.GetAccountEventsAsync(accountId);
    Console.WriteLine("📜 Historial de eventos:");
    foreach (var evt in events)
    {
        Console.WriteLine($"   • {evt.GetType().Name}: {JsonSerializer.Serialize(evt)}");
    }
    
    // 7. Intentar retirar más de lo disponible (debe fallar)
    Console.WriteLine("❌ Intentando retirar $5000 (debe fallar)...");
    try
    {
        await serviceCommand.HandleAsync(new WithdrawMoney(
            accountId,
            5000m,
            "Compra costosa"
        ));
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error esperado: {ex.Message}");
    }
    
    // 8. Cerrar la cuenta
    Console.WriteLine("🔒 Cerrando la cuenta...");
    await serviceCommand.HandleAsync(new CloseAccount(
        accountId,
        "Cliente se mudó de país"
    ));
    Console.WriteLine("✅ Cuenta cerrada");

    // Verificar estado final
    account = await serviceQuery.GetAccountAsync(accountId);
    Console.WriteLine("📊 Estado final:");
    Console.WriteLine($"   Cerrada: {account!.IsClosed}");
    Console.WriteLine($"   Saldo final: ${account.Balance}");
    
    
}
catch (Exception e)
{
    Console.WriteLine($"❌ Error: {e.Message}");
}

