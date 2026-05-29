using Application.DTOs.Orders;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
[Tags("Retail Workflow - Customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Lists active customers registered in the retail system.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(List<CustomerResponseDto>), 200)]
    public async Task<ActionResult<
        List<CustomerResponseDto>>> GetAll()
    {
        var customers = await _customerService
            .GetAllCustomersAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Gets customer details, including how many orders the customer has placed.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(CustomerDetailResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CustomerDetailResponseDto>>
        GetById(int id)
    {
        var customer = await _customerService
            .GetCustomerByIdAsync(id);

        if (customer is null)
            return NotFound(new
            {
                message =
                    $"Customer with id {id} " +
                    $"was not found."
            });

        return Ok(customer);
    }

    /// <summary>
    /// Creates a customer account using a unique normalized email address.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(CustomerResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<CustomerResponseDto>>
        Create([FromBody] CreateCustomerDto dto)
    {
        var customer = await _customerService
            .CreateCustomerAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            customer);
    }

    /// <summary>
    /// Updates the customer display name while preserving their email identity.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(CustomerResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CustomerResponseDto>>
        Update(
            int id,
            [FromBody] UpdateCustomerDto dto)
    {
        var updated = await _customerService
            .UpdateCustomerAsync(id, dto);

        if (updated is null)
            return NotFound(new
            {
                message =
                    $"Customer with id {id} " +
                    $"was not found."
            });

        return Ok(updated);
    }

    /// <summary>
    /// Archives a customer with soft delete so historical orders remain available.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _customerService
            .SoftDeleteCustomerAsync(id);

        if (!deleted)
            return NotFound(new
            {
                message =
                    $"Customer with id {id} " +
                    $"was not found."
            });

        return NoContent();
    }

    /// <summary>
    /// Lists orders placed by a specific customer.
    /// </summary>
    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(
        typeof(List<OrderResponseDto>), 200)]
    public async Task<ActionResult<
        List<OrderResponseDto>>>
        GetOrders(int id)
    {
        var orders = await _customerService
            .GetCustomerOrdersAsync(id);

        return Ok(orders);
    }
}
