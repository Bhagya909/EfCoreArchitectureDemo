using Application.DTOs.Orders;
using Application.Interfaces.Repositories;
using Domain.Entities.Orders;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly RetailDbContext _context;

    private static readonly
        Func<RetailDbContext, string, IAsyncEnumerable<int>>
        _existsByEmailCompiledQuery =
            EF.CompileAsyncQuery(
                (RetailDbContext context, string email) =>
                    context.Customers
                        .AsNoTracking()
                        .Where(c => c.Email == email)
                        .Select(c => 1));

    private static readonly
        Func<RetailDbContext, int, IAsyncEnumerable<int>>
        _existsByIdCompiledQuery =
            EF.CompileAsyncQuery(
                (RetailDbContext context, int id) =>
                    context.Customers
                        .AsNoTracking()
                        .Where(c => c.Id == id)
                        .Select(c => 1));

    private static readonly
        Func<RetailDbContext, int, IAsyncEnumerable<CustomerDetailResponseDto>>
        _getByIdWithOrderCountCompiledQuery =
            EF.CompileAsyncQuery(
                (RetailDbContext context, int id) =>
                    context.Customers
                        .AsNoTracking()
                        .Where(c => c.Id == id)
                        .Select(c => new CustomerDetailResponseDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Email = c.Email,
                            CreatedAt = c.CreatedAt,
                            TotalOrders = c.Orders.Count()
                        }));

    public CustomerRepository(RetailDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();

        await foreach (var _ in
            _existsByEmailCompiledQuery(_context, normalized))
        {
            return true;
        }

        return false;
    }

    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CustomerResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }

    public async Task<CustomerDetailResponseDto?>
        GetByIdWithOrderCountAsync(int id)
    {
        await foreach (var customer in
            _getByIdWithOrderCountCompiledQuery(_context, id))
        {
            return customer;
        }

        return null;
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        await foreach (var _ in
            _existsByIdCompiledQuery(_context, id))
        {
            return true;
        }

        return false;
    }
}
