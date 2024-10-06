using System;
using DropStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DropStockAPI.Controllers;

[Authorize] // Only Admin and Manager roles can access these endpoints  (Roles = "Admin,Manager")
[ApiController]
[Route("api/[controller]")]
[EnableCors("EnableCors")]
public class CustomerController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all customers with optional pagination and search
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 100,
        [FromQuery] string? searchQuery = null)
    {
        int skip = (page - 1) * limit;

        var query = _context.CustomerModels.AsQueryable();

        // Apply search filtering if search query is provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(c => EF.Functions.ILike(c.firstname + " " + c.lastname, $"%{searchQuery}%"));
        }

        // Count total records before pagination
        var totalRecords = query.Count();

        // Paginate results
        var customers = query
            .OrderBy(c => c.customerid)
            .Skip(skip)
            .Take(limit)
            .ToList();

        // Return the total number of records and the paginated list of customers
        return Ok(new { Total = totalRecords, Customers = customers });
    }

    // Get a single customer by ID
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult GetCustomer(int id)
    {
        var customer = _context.CustomerModels.Find(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    // Create a new customer
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerModel>> CreateCustomer([FromBody] CustomerModel customer)
    {
        // Validate the incoming customer model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.CustomerModels.Add(customer);
        await _context.SaveChangesAsync();

        // Return the newly created customer with its ID
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.customerid }, customer);
    }

    // Update an existing customer
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerModel>> UpdateCustomer(int id, [FromBody] CustomerModel customer)
    {
        var existingCustomer = _context.CustomerModels.FirstOrDefault(c => c.customerid == id);

        if (existingCustomer == null)
        {
            return NotFound();
        }

        // Validate the incoming customer model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Update customer fields
        existingCustomer.firstname = customer.firstname;
        existingCustomer.lastname = customer.lastname;
        existingCustomer.address = customer.address;
        existingCustomer.phone = customer.phone;
        existingCustomer.email = customer.email;
        existingCustomer.staffid = customer.staffid;

        await _context.SaveChangesAsync();

        return Ok(existingCustomer);
    }

    // Delete a customer by ID
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult DeleteCustomer(int id)
    {
        var customer = _context.CustomerModels.Find(id);

        if (customer == null)
        {
            return NotFound();
        }

        _context.CustomerModels.Remove(customer);
        _context.SaveChanges();

        return Ok(customer);
    }
}