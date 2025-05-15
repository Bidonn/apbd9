using Microsoft.AspNetCore.Mvc;

namespace Tutorial9.Controllers;
using Tutorial9.Services;
using Tutorial9.Model;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    
    private readonly ITransService _transService;

    public WarehouseController(ITransService transService)
    {
        _transService = transService;
    }

    [HttpPut("put")]
    public async Task<IActionResult> TransactionAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken)
    {
        try
        {
            await _transService.TransactionAsync(orderTransaction, cancellationToken);
            return Ok("Everything went smoothly...");
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine(e);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
        
    }
    
    [HttpPut("procedure")]
    public async Task<IActionResult> ProductAsync(OrderTransactionDTO orderTransaction, CancellationToken cancellationToken)
    {
        try
        {
            await _transService.ProcedureAsync(orderTransaction, cancellationToken);
            return Ok("Everything went smoothly...");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}