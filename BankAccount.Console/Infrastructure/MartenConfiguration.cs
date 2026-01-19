using JasperFx;
using JasperFx.Events;

namespace BankAccount.Console.Infrastructure;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;

public static class MartenConfiguration
{
    public static IDocumentStore ConfigureDocumentStore(string connectionString)
    {
        return DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Events.StreamIdentity = StreamIdentity.AsGuid;

        });
    } 
}