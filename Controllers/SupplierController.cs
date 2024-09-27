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
    [EnableCors("MultipleOrigins")]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all suppliers with optional pagination and search
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetSuppliers(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 100,
            [FromQuery] string? searchQuery = null)
        {
            int skip = (page - 1) * limit;

            var query = _context.SupplierModels.AsQueryable();

            // Apply search filtering if a search query is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(s => EF.Functions.ILike(s.name, $"%{searchQuery}%"));
            }

            // Count total records before pagination
            var totalRecords = query.Count();

            // Paginate results
            var suppliers = query
                .OrderBy(s => s.supplierid)
                .Skip(skip)
                .Take(limit)
                .ToList();

            // Return the total number of records and the paginated list of suppliers
            return Ok(new { Total = totalRecords, Suppliers = suppliers });
        }

        // Get a single supplier by ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetSupplier(int id)
        {
            var supplier = _context.SupplierModels.Find(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return Ok(supplier);
        }

        // Create a new supplier
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SupplierModel>> CreateSupplier([FromBody] SupplierModel supplier)
        {
            // Validate the incoming supplier model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SupplierModels.Add(supplier);
            await _context.SaveChangesAsync();

            // Return the newly created supplier with its ID
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.supplierid }, supplier);
        }

        // Update an existing supplier
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SupplierModel>> UpdateSupplier(int id, [FromBody] SupplierModel supplier)
        {
            var existingSupplier = _context.SupplierModels.FirstOrDefault(s => s.supplierid == id);

            if (existingSupplier == null)
            {
                return NotFound();
            }

            // Validate the incoming supplier model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update supplier fields
            existingSupplier.name = supplier.name;
            existingSupplier.address = supplier.address;
            existingSupplier.phone = supplier.phone;
            existingSupplier.email = supplier.email;
            existingSupplier.otherdetail = supplier.otherdetail;

            await _context.SaveChangesAsync();

            return Ok(existingSupplier);
        }

        // Delete a supplier by ID
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult DeleteSupplier(int id)
        {
            var supplier = _context.SupplierModels.Find(id);

            if (supplier == null)
            {
                return NotFound();
            }

            _context.SupplierModels.Remove(supplier);
            _context.SaveChanges();

            return Ok(supplier);
        }
    }
}
