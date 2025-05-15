using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class TransService : ITransService
{

    private readonly string? _connectionString;
    public TransService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }
    public async Task TransactionAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.CommandText = "SELECT Price FROM DBO.PRODUCT WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", orderTransaction.IdProduct);
            
            var res = (decimal?)await command.ExecuteScalarAsync(cancellationToken);
            if (res is null)
                throw new NullReferenceException("Product not found");
            decimal price = (decimal)res;
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM DBO.WAREHOUSE WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", orderTransaction.IdWarehouse);
            res = (int?)await command.ExecuteScalarAsync(cancellationToken);
            if (res is null)
                throw new NullReferenceException("Warehouse not found");
            if(orderTransaction.Ammount <= 0 )
                throw new Exception("Ammount must be greater than 0");
            
            command.Parameters.Clear();
            
            command.CommandText = "SELECT IdOrder FROM DBO.[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt <= @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", orderTransaction.IdProduct);
            command.Parameters.AddWithValue("@Amount", orderTransaction.Ammount);
            command.Parameters.AddWithValue("@CreatedAt", orderTransaction.CreatedAt);

            int? idOrder = null;
            await using (SqlDataReader idOrd = await command.ExecuteReaderAsync(cancellationToken))
            {
                if(await idOrd.ReadAsync(cancellationToken))
                    idOrder = idOrd.GetInt32(idOrd.GetOrdinal("IdOrder"));
                else
                    throw new NullReferenceException("No order with matching data found");  
            }
            
            
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM DBO.Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            
            res = (int?)await command.ExecuteScalarAsync(cancellationToken);
            if (res is not null)
                throw new Exception("Order already fulfilled");
            
            command.Parameters.Clear();
            command.CommandText = "UPDATE DBO.[Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);

            await command.ExecuteNonQueryAsync(cancellationToken);
            
            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO DBO.Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            command.Parameters.AddWithValue("@IdWarehouse", orderTransaction.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", orderTransaction.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", orderTransaction.Ammount);
            command.Parameters.AddWithValue("@Price", price * orderTransaction.Ammount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync();

        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task ProcedureAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IdProduct", orderTransaction.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", orderTransaction.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", orderTransaction.Ammount);
        command.Parameters.AddWithValue("@CreatedAt", orderTransaction.CreatedAt);
        
        await command.ExecuteNonQueryAsync(cancellationToken);
        
    }
}