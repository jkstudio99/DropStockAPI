using DropStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DropStockAPI.Controllers
{
    // [Authorize(Roles = UserRolesModel.Admin + "," + UserRolesModel.Manager)]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("MultipleOrigins")]
    public class CategoryController : ControllerBase
    {

        // สร้าง Object ของ ApplicationDbContext
        private readonly ApplicationDbContext _context;

        // ฟังก์ชันสร้าง Constructor รับค่า ApplicationDbContext
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CRUD Category
        // ฟังก์ชันสำหรับการดึงข้อมูล Category ทั้งหมด
        // GET /api/Category
        [HttpGet]
        public ActionResult<CategoryModel> GetCategories()
        {
            // LINQ stand for "Language Integrated Query"
            var categories = _context.CategoryModels.ToList(); // select * from category

            // ส่งข้อมูลกลับไปให้ Client เป็น JSON
            return Ok(categories);
        }

        // ฟังก์ชันสำหรับการดึงข้อมูล Category ตาม ID
        // GET /api/Category/1
        [HttpGet("{id}")]
        public ActionResult<CategoryModel> GetCategory(int id)
        {
            // LINQ สำหรับการดึงข้อมูลจากตาราง Categories ตาม ID
            var category = _context.CategoryModels.Find(id); // select * from category where id = 1

            // ถ้าไม่พบข้อมูล
            if (category == null)
            {
                return NotFound();
            }

            // ส่งข้อมูลกลับไปให้ Client เป็น JSON
            return Ok(category);
        }

        // ฟังก์ชันสำหรับการเพิ่มข้อมูล Category
        // POST /api/Category
        // [Authorize(Roles = UserRolesModel.Admin + "," + UserRolesModel.Manager)]
        [HttpPost]
        public ActionResult<CategoryModel> AddCategory([FromBody] CategoryModel category)
        {
            // เพิ่มข้อมูลลงในตาราง Categories
            _context.CategoryModels.Add(category); // insert into category values (...)
            _context.SaveChanges(); // commit

            // ส่งข้อมูลกลับไปให้ Client เป็น JSON
            return Ok(category);
        }

        // ฟังก์ชันสำหรับการแก้ไขข้อมูล Category
        // PUT /api/Category/1
        [HttpPut("{id}")]
        public ActionResult<CategoryModel> UpdateCategory(int id, [FromBody] CategoryModel category)
        {
            // ค้นหาข้อมูล Category ตาม ID
            var cat = _context.CategoryModels.Find(id); // select * from category where id = 1

            // ถ้าไม่พบข้อมูลให้ return NotFound
            if (cat == null)
            {
                return NotFound();
            }

            // แก้ไขข้อมูล Category
            cat.categoryname = category.categoryname; // update category set categoryname = '...' where id = 1
            cat.categorystatus = category.categorystatus; // update category set categorystatus = '...' where id = 1

            // commit
            _context.SaveChanges();

            // ส่งข้อมูลกลับไปให้ Client เป็น JSON
            return Ok(cat);
        }

        // ฟังก์ชันสำหรับการลบข้อมูล Category
        // DELETE /api/Category/1
        [HttpDelete("{id}")]
        public ActionResult<CategoryModel> DeleteCategory(int id)
        {
            // ค้นหาข้อมูล Category ตาม ID
            var cat = _context.CategoryModels.Find(id); // select * from category where id = 1

            // ถ้าไม่พบข้อมูลให้ return NotFound
            if (cat == null)
            {
                return NotFound();
            }

            // ลบข้อมูล Category
            _context.CategoryModels.Remove(cat); // delete from category where id = 1
            _context.SaveChanges(); // commit

            // ส่งข้อมูลกลับไปให้ Client เป็น JSON
            return Ok(cat);
        }

    }
}
