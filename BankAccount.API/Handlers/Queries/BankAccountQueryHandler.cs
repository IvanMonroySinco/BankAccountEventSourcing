using BankAccount.Domain.Aggregates;
using Marten;

namespace BankAccount.API.Handlers;

public class BankAccountQueryHandler : IBankAccountQueryHandler
{
      private readonly IDocumentStore _store;   
      
      public BankAccountQueryHandler(IDocumentStore store)                                                                                                                                                                        
      {                                                                                                                                                                                                                             
          _store = store;                                                                                                                                                                                                           
      }  
                                                                                                                                                                                             
      public async Task<BankAccountAggregate?> GetAccountAsync(Guid accountId)                                                                                                                                                      
      {                                                                                                                                                                                                                             
          await using var session = _store.QuerySession();                                                                                                                                                                          
          return await session.Events.AggregateStreamAsync<BankAccountAggregate>(accountId);                                                                                                                                        
      }                                                                                                                                                                                                                             
                                                                                                                                                                                                                                   
      public async Task<List<object>> GetAccountEventsAsync(Guid accountId)                                                                                                                                                         
      {                                                                                                                                                                                                                             
          await using var session = _store.QuerySession();                                                                                                                                                                          
          var events = await session.Events.FetchStreamAsync(accountId);                                                                                                                                                            
          return events.Select(e => e.Data).ToList();                                                                                                                                                                               
      }     
      
      public async Task<decimal> GetBalanceAsync(Guid accountId)                                                                                                                                                                    
      {                                                                                                                                                                                                                             
          await using var session = _store.QuerySession();                                                                                                                                                                          
          var account = await session.Events.AggregateStreamAsync<BankAccountAggregate>(accountId);                                                                                                                                 
          return account?.Balance ?? 0;                                                                                                                                                                                             
      }     
}