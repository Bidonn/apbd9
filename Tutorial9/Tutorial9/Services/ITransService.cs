using Tutorial9.Model;

namespace Tutorial9.Services;

public interface ITransService
{
    Task TransactionAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken);
    Task ProcedureAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken);
}