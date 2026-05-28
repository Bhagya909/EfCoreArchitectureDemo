using Application.DTOs.Orders;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

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
