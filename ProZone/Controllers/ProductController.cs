using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using MongoDB.Bson;
using MongoDB.Driver;
using ProZone.Models;
using ProZone.Models.VMs;
using SharpCompress.Common;
using System.IO;

using System.Threading.Tasks;

namespace ProZone.Controllers
{
    public class ProductController : Controller
    {
        private readonly IMongoCollection<Product> _mongoCollection;
        private readonly IMongoCollection<ProductItem> _mongoItemCollection;
        private readonly IMongoCollection<Category> _mongoCatCollection;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string imagePath;

        public ProductController(MongoDbService mongoDbService, IWebHostEnvironment webHostEnvironment)
        {
            _mongoCollection = mongoDbService.Database.GetCollection<Product>("products");
            _mongoCatCollection = mongoDbService.Database.GetCollection<Category>("categories");
            _mongoItemCollection = mongoDbService.Database.GetCollection<ProductItem>("productItems");
            _webHostEnvironment = webHostEnvironment;
            this.imagePath = $"{_webHostEnvironment.WebRootPath}/Images/";
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _mongoCollection.Find(_ => true).ToListAsync();
            var productItemsCount = await _mongoItemCollection.EstimatedDocumentCountAsync();
            ViewBag.path = this.imagePath;
            ViewBag.count = productItemsCount;
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            var categoriesFromDb = await _mongoCatCollection.Find(_ => true).ToListAsync();

            ProductViewModel productView = new()
            {
                categories = categoriesFromDb.Select(cat => new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name.en // or `.Ar`, depending on your current culture or preference
                }).ToList()
            };

            return View(productView);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            var product = new Product()
            {
                Name = new LocalizedName() { en = vm.NameEn, ar = vm.NameAr },
                Description = new LocalizedDescription() { en = vm.DescriptionEn ?? "", ar = vm.DescriptionAr ?? "" },
            };
            var categories = await _mongoCatCollection.Find(_ => true).ToListAsync();
            var selectListCategories = categories.Select(cat => new SelectListItem
            {
                Value = cat.Id.ToString(),
                Text = cat.Name.en 
            }).ToList();
            var cover = await SaveCover(vm.Photo);

            product.Photo = cover;
            await _mongoCollection.InsertOneAsync(product);
            return RedirectToAction("products");
        }
        [HttpGet]
        public async Task<IActionResult> Details(ObjectId id)
        {
            var product = await _mongoCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }
            var productItems = await _mongoItemCollection.Find(p => p.ProductId == id).ToListAsync();
            ViewBag.ProductItems = productItems;
            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(ObjectId id)
        {
            var product = await _mongoCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }
            var categoriesFromDb = await _mongoCatCollection.Find(_ => true).ToListAsync();

            EditProductViewModel productView = new()
            {
                NameAr = product.Name.ar,
                NameEn = product.Name.en,
                DescriptionEn = product.Description.en,
                DescriptionAr = product.Description.ar,
                Photo = product.Photo,
                categories = categoriesFromDb.Select(cat => new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name.en // or `.Ar`, depending on your current culture or preference
                }).ToList()
            };

            return View(productView);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ObjectId id, ProductViewModel vm)
        {
            var product = await _mongoCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Delete old photo
            await DeleteCover(product.Photo);

            // Update values
            product.Name = new LocalizedName { en = vm.NameEn, ar = vm.NameAr };
            product.Description = new LocalizedDescription { en = vm.DescriptionEn ?? "", ar = vm.DescriptionAr ?? "" };

            var cover = await SaveCover(vm.Photo);
            product.Photo = cover;

            
            // Apply the update
            await _mongoCollection.ReplaceOneAsync(p => p.Id == id, product);

            return RedirectToAction("Products");
        }

        public async Task<IActionResult> Delete(ObjectId id)
        {
            var product = await _mongoCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
            var productItems = await _mongoItemCollection.Find(p => p.ProductId == id).ToListAsync();
            if(productItems != null)
            {
                foreach (var item in productItems)
                {
                    await DeleteCover(item.Photo);
                    await _mongoItemCollection.DeleteOneAsync(p => p.Id == item.Id);
                }
            }
            await _mongoCollection.DeleteOneAsync(p => p.Id == id);
            await DeleteCover(product.Photo);

            return RedirectToAction("products");
        }
        private async Task<string> SaveCover(IFormFile cover)
        {
            if (cover == null)
                return "NoCover";
            var coverName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(cover.FileName)}";
            var path = Path.Combine(imagePath,coverName);
            using var stream = System.IO.File.Create(path);
            await cover.CopyToAsync(stream);
            return coverName;
        }
        private async Task<bool> DeleteCover(string cover)
        {
            if (cover == null)
                return false;
            var path = Path.Combine(imagePath, cover);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            return true;
        }
       
    }
}
