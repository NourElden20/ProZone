using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ProZone.Models;
using ProZone.Models.VMs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProZone.Controllers
{
    public class ProductItemController : Controller
    {
        private readonly IMongoCollection<ProductItem> _productItemCollection;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMongoCollection<Product> _mongoCollection;

        private readonly string imagePath;

        public ProductItemController(MongoDbService mongoDbService, IWebHostEnvironment webHostEnvironment)
        {
            _productItemCollection = mongoDbService.Database.GetCollection<ProductItem>("productItems");
            _webHostEnvironment = webHostEnvironment;
            this.imagePath = $"{_webHostEnvironment.WebRootPath}/Images/";
            _mongoCollection = mongoDbService.Database.GetCollection<Product>("products");
        }

        [HttpGet("ProductItem/Index/{productId}")]
        public async Task<IActionResult> Index(ObjectId productId)   
        {
            var items = await _productItemCollection.Find(x => x.ProductId == productId).ToListAsync();
            ViewBag.ProductId = productId;
            return View(items);
        }

        [HttpGet("ProductItem/Create/{productId}")]
        public IActionResult Create(ObjectId productId)
        {
            var model = new ProductItemViewModel { ProductId = productId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductItemViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var productItem = new ProductItem()
            {
                VariationName = vm.OptionValue,
                ProductId = vm.ProductId,
                Price = vm.Price,
                StockQuantity = vm.StockQuantity
            };
            var product = await _mongoCollection.Find(p => p.Id == vm.ProductId).FirstOrDefaultAsync();
            if (productItem.Price < product.MinPrice)
                product.MinPrice = productItem.Price;
            // Apply the update
            await _mongoCollection.ReplaceOneAsync(p => p.Id == vm.ProductId, product);
            var random = new Random();
            int randomNumber = random.Next(1000, 10000); // Generates a number from 1000 to 9999
            productItem.SKU = $"{product.Name.en}-{productItem.VariationName}-{randomNumber}";

            productItem.Photo = await SaveCover(vm.Photo);
            await _productItemCollection.InsertOneAsync(productItem);

            return RedirectToAction("Index", new { productId = product.Id});
        }

    
      
        public async Task<IActionResult> Delete(ObjectId id, ObjectId productId)
        {
            var productItem = await _productItemCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            await DeleteCover(productItem.Photo);
            await _productItemCollection.DeleteOneAsync(i => i.Id == id);
            return RedirectToAction("Index", new { productId = productItem.ProductId });
        }
        private async Task<string> SaveCover(IFormFile cover)
        {
            if (cover == null)
                return "NoCover";
            var coverName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(cover.FileName)}";
            var path = Path.Combine(imagePath, coverName);
            using var stream = System.IO.File.Create(path);
            await cover.CopyToAsync(stream);
            return coverName;
        }
        private async Task<bool> DeleteCover(string cover)
        {
            if (cover == null)
                return false;
            var path = Path.Combine(imagePath, cover);
            System.IO.File.Delete(path);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            return true;
        }
    }
}
