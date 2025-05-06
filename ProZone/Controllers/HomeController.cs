using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProZone.Models;
using System.Diagnostics;

namespace ProZone.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoCollection<Product> _mongoCollection;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string imagePath;


        public HomeController(MongoDbService mongoDbService, IWebHostEnvironment webHostEnvironment)
        {
            _mongoCollection = mongoDbService.Database.GetCollection<Product>("products");
            _webHostEnvironment = webHostEnvironment;
            this.imagePath = $"{_webHostEnvironment.WebRootPath}/Images/";

        }

        public async Task<IActionResult> Index()
        {
            var products = await _mongoCollection.Find(_ => true).ToListAsync();
            ViewBag.path = imagePath;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
