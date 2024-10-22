using DropStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DropStockAPI.Controllers
{
    [Authorize] // Only Admin and Manager roles can access these endpoints
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsDropStock")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all payments with optional pagination and search
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetPayments(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 100,
            [FromQuery] string? searchQuery = null)
        {
            int skip = (page - 1) * limit;

            var query = _context.PaymentModels.AsQueryable();

            // Apply search filtering if a search query is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => EF.Functions.ILike(p.paymenttype, $"%{searchQuery}%"));
            }

            // Count total records before pagination
            var totalRecords = query.Count();

            // Paginate results
            var payments = query
                .OrderBy(p => p.billnumber)
                .Skip(skip)
                .Take(limit)
                .ToList();

            // Return the total number of records and the paginated list of payments
            return Ok(new { Total = totalRecords, Payments = payments });
        }

        // Get a single payment by Bill Number
        [HttpGet("{billnumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetPayment(int billnumber)
        {
            var payment = _context.PaymentModels.Find(billnumber);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        // Create a new payment
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentModel>> CreatePayment([FromBody] PaymentModel payment)
        {
            // Validate the incoming payment model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PaymentModels.Add(payment);
            await _context.SaveChangesAsync();

            // Return the newly created payment with its Bill Number
            return CreatedAtAction(nameof(GetPayment), new { billnumber = payment.billnumber }, payment);
        }

        // Update an existing payment
        [HttpPut("{billnumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentModel>> UpdatePayment(int billnumber, [FromBody] PaymentModel payment)
        {
            var existingPayment = _context.PaymentModels.FirstOrDefault(p => p.billnumber == billnumber);

            if (existingPayment == null)
            {
                return NotFound();
            }

            // Validate the incoming payment model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update payment fields
            existingPayment.paymenttype = payment.paymenttype;
            existingPayment.otherdetail = payment.otherdetail;

            await _context.SaveChangesAsync();

            return Ok(existingPayment);
        }

        // Delete a payment by Bill Number
        [HttpDelete("{billnumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult DeletePayment(int billnumber)
        {
            var payment = _context.PaymentModels.Find(billnumber);

            if (payment == null)
            {
                return NotFound();
            }

            _context.PaymentModels.Remove(payment);
            _context.SaveChanges();

            return Ok(payment);
        }
    }
}
