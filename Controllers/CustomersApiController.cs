using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public CustomersApiController(
        ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _context.Customers
            .Include(c => c.User)
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.UserId,
                Name = c.User.FullName,
                Email = c.User.Email,
                c.Phone
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.UserId,
                Name = c.User.FullName,
                Email = c.User.Email,
                c.Phone
            })
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        CreateCustomerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = request.Email
            .Trim()
            .ToLower();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (existingUser)
        {
            return Conflict(new
            {
                message = "Email is already registered."
            });
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Customer");

        if (role == null)
        {
            return BadRequest(new
            {
                message = "Customer role does not exist."
            });
        }

        var user = new User
        {
            FullName = request.Name.Trim(),
            Email = email,
            RoleId = role.Id
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var customer = new Customer
        {
            UserId = user.Id,
            Phone = request.Phone.Trim()
        };

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            new
            {
                customer.Id,
                customer.UserId,
                Name = user.FullName,
                Email = user.Email,
                customer.Phone
            });
    }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Email { get; set; } = "";

    public string Password { get; set; } = "";
}