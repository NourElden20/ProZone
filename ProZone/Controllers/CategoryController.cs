using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProZone.Models;
using ProZone.Models.VMs;

namespace ProZone.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryController(MongoDbService mongoDbService)
        {           
            _categories = mongoDbService.Database.GetCollection<Category>("categories");
        }

        public async Task<IActionResult> CreateCategory()
        {          
            return View();
        }
        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var category = new Category()
            {
                Name = new LocalizedName { en=vm.NameEn,ar=vm.NameAr}
            };
            await _categories.InsertOneAsync(category);
            return RedirectToAction("Categories");
            
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> Categories()
        {
            var categories = await _categories.Find(_ => true).ToListAsync();
            return View(categories);
        }

        

      
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _categories.DeleteOneAsync(c => c.Id == MongoDB.Bson.ObjectId.Parse(id));

            if (result.DeletedCount == 0)
                return NotFound();

            return RedirectToAction("Categories");
        }
    }
}
