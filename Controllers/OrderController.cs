using DropStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DropStockAPI.Controllers
{
    [Authorize] // Only Admin and Manager roles can access these endpoints
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsDropStock")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all orders with optional pagination and search
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 100,
            [FromQuery] string? searchQuery = null)
        {
            int skip = (page - 1) * limit;

            var query = _context.OrderModels.AsQueryable();

            // Apply search filtering if a search query is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(o => EF.Functions.ILike(o.ordername, $"%{searchQuery}%"));
            }

            // Count total records before pagination
            var totalRecords = query.Count();

            // Paginate results
            var orders = query
                .OrderByDescending(o => o.orderid)
                .Skip(skip)
                .Take(limit)
                .Select(o => new
                {
                    o.orderid,
                    o.ordername,
                    o.orderprice,
                    o.orderstatus,
                    o.orderdetails,
                    o.createddate,
                    o.modifieddate
                })
                .ToList();

            // Return the total number of records and the paginated list of orders
            return Ok(new { Total = totalRecords, Orders = orders });
        }

        // Get a single order by ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetOrder(int id)
        {
            var order = _context.OrderModels
                .Select(o => new
                {
                    o.orderid,
                    o.ordername,
                    o.orderprice,
                    o.orderstatus,
                    o.orderdetails,
                    o.createddate,
                    o.modifieddate
                })
                .FirstOrDefault(o => o.orderid == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // Create a new order
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrderModel>> CreateOrder([FromBody] OrderModel order)
        {
            // Validate the incoming order model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set the creation date to current time
            order.createddate = DateTime.UtcNow;

            _context.OrderModels.Add(order);
            await _context.SaveChangesAsync();

            // Return the newly created order with its ID
            return CreatedAtAction(nameof(GetOrder), new { id = order.orderid }, order);
        }

        // Update an existing order
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrderModel>> UpdateOrder(int id, [FromBody] OrderModel order)
        {
            var existingOrder = _context.OrderModels.FirstOrDefault(o => o.orderid == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            // Validate the incoming order model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update order fields
            existingOrder.ordername = order.ordername;
            existingOrder.orderprice = order.orderprice;
            existingOrder.orderstatus = order.orderstatus;
            existingOrder.orderdetails = order.orderdetails;
            existingOrder.customerid = order.customerid;
            existingOrder.modifieddate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingOrder);
        }

        // Delete an order by ID
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult DeleteOrder(int id)
        {
            var order = _context.OrderModels.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.OrderModels.Remove(order);
            _context.SaveChanges();

            return Ok(order);
        }
    }
}
