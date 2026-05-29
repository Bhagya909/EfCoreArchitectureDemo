using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
using Domain.Entities.Orders;

namespace Application.Services.Orders;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChangeLogService _changeLogService;

    public CustomerService(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IChangeLogService changeLogService)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _changeLogService = changeLogService;
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(
        CreateCustomerDto dto)
    {
        CustomerValidator.ValidateCreateCustomer(dto);

        var emailExists = await _customerRepository
            .ExistsByEmailAsync(dto.Email);

        if (emailExists)
            throw new InvalidOperationException(
                $"Customer with email '{dto.Email}' " +
                $"already exists.");

        var customer = new Customer(dto.Name, dto.Email);

        await _customerRepository.AddAsync(customer);

        await _changeLogService.LogAsync(
            actionType: "CUSTOMER_CREATED",
            entityName: "Customer",
            referenceId: customer.Id,
            description:
                $"Customer '{customer.Name}' " +
                $"registered with email {customer.Email}.",
            rawData:
                $"Name={dto.Name}; Email={dto.Email}");

        await _unitOfWork.SaveChangesAsync();

        return customer.ToResponseDto();
    }

    public async Task<CustomerDetailResponseDto?> GetCustomerByIdAsync(
        int id)
    {
        return await _customerRepository
            .GetByIdWithOrderCountAsync(id);
    }

    public async Task<List<CustomerResponseDto>>
        GetAllCustomersAsync()
    {
        var customers = await _customerRepository
            .GetAllAsync();

        return customers;
    }

    public async Task<CustomerResponseDto?> UpdateCustomerAsync(
        int id,
        UpdateCustomerDto dto)
    {
        CustomerValidator.ValidateUpdateCustomer(dto);

        var customer = await _customerRepository
            .GetByIdAsync(id);

        if (customer is null)
            return null;

        customer.UpdateName(dto.Name);

        await _changeLogService.LogAsync(
            actionType: "CUSTOMER_UPDATED",
            entityName: "Customer",
            referenceId: customer.Id,
            description:
                $"Customer '{customer.Name}' " +
                $"name updated.",
            rawData:
                $"NewName={dto.Name}");

        await _unitOfWork.SaveChangesAsync();

        return customer.ToResponseDto();
    }

    public async Task<bool> SoftDeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository
            .GetByIdAsync(id);

        if (customer is null)
            return false;

        customer.SoftDelete();

        await _changeLogService.LogAsync(
            actionType: "CUSTOMER_DELETED",
            entityName: "Customer",
            referenceId: customer.Id,
            description:
                $"Customer '{customer.Name}' " +
                $"with email {customer.Email} " +
                $"was soft deleted.");

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<OrderResponseDto>>
        GetCustomerOrdersAsync(int id)
    {
        var customerExists = await _customerRepository
            .ExistsByIdAsync(id);

        if (!customerExists)
            throw new KeyNotFoundException(
                $"Customer {id} not found.");

        var orders = await _orderRepository
            .GetByCustomerIdAsync(id);

        return orders
            .Select(o => o.ToResponseDto())
            .ToList();
    }
}
