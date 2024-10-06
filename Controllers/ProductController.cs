using DropStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DropStockAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("EnableCors")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly Cloudinary _cloudinary;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env, Cloudinary cloudinary)
        {
            _context = context;
            _env = env;
            _cloudinary = cloudinary;
        }

        // GET /api/Product
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 100,
            [FromQuery] string? searchQuery = null,
            [FromQuery] int? selectedCategory = null
        )
        {
            int skip = (page - 1) * limit;

            var query = _context.ProductModels
                .Join(
                    _context.CategoryModels,
                    p => p.categoryid,
                    c => c.categoryid,
                    (p, c) => new
                    {
                        p.productid,
                        p.productname,
                        p.unitprice,
                        p.unitinstock,
                        p.productpicture,
                        p.categoryid,
                        p.createddate,
                        p.modifieddate,
                        c.categoryname
                    }
                );

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => EF.Functions.ILike(p.productname!, $"%{searchQuery}%"));
            }

            if (selectedCategory.HasValue)
            {
                query = query.Where(p => p.categoryid == selectedCategory.Value);
            }

            var totalRecords = query.Count();

            var products = query
                .OrderByDescending(p => p.productid)
                .Skip(skip)
                .Take(limit)
                .ToList();

            return Ok(new
            {
                Total = totalRecords,
                Products = products
            });
        }

        // GET /api/Product/1
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetProduct(int id)
        {
            var product = _context.ProductModels
                .Join(
                    _context.CategoryModels,
                    p => p.categoryid,
                    c => c.categoryid,
                    (p, c) => new
                    {
                        p.productid,
                        p.productname,
                        p.unitprice,
                        p.unitinstock,
                        p.productpicture,
                        p.categoryid,
                        p.createddate,
                        p.modifieddate,
                        c.categoryname
                    }
                )
                .FirstOrDefault(p => p.productid == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: /api/Product
        [HttpPost]
        public async Task<ActionResult<ProductModel>> CreateProduct([FromForm] ProductModel product, IFormFile? image)
        {
            _context.ProductModels.Add(product);

            if (image != null)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill"),
                    Folder = "da-net8"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    product.productpicture = uploadResult.Url.ToString();
                }
                else
                {
                    return BadRequest("Image upload failed.");
                }
            }
            else
            {
                product.productpicture = "noimg.jpg";
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.productid }, product);
        }

        // PUT: /api/Product/1
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductModel>> UpdateProduct(int id, [FromForm] ProductModel product, IFormFile? image)
        {
            var existingProduct = _context.ProductModels.FirstOrDefault(p => p.productid == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.productname = product.productname;
            existingProduct.unitprice = product.unitprice;
            existingProduct.unitinstock = product.unitinstock;
            existingProduct.categoryid = product.categoryid;
            existingProduct.modifieddate = DateTime.Now;

            if (image != null)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    existingProduct.productpicture = uploadResult.Url.ToString();
                }
                else
                {
                    return BadRequest("Image upload failed.");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(existingProduct);
        }

        // DELETE /api/Product/1
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ProductModel> DeleteProduct(int id)
        {
            var product = _context.ProductModels.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.productpicture) && product.productpicture != "noimg.jpg")
            {
                // If necessary, you can add code here to delete the image from Cloudinary
                // Example: _cloudinary.DestroyAsync(new DeletionParams("public_id_of_the_image"));
            }

            _context.ProductModels.Remove(product);
            _context.SaveChanges();

            return Ok(product);
        }
    }
}
