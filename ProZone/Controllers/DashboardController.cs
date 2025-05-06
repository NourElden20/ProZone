using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProZone.Models;

namespace ProZone.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<ProductItem> _productItemCollection;
        private readonly IMongoCollection<ApplicationUser> _mongoCollection;

        // Constructor to inject MongoDB collections
        public DashboardController(MongoDbService mongoDbService)
        {
            _orderCollection = mongoDbService.Database.GetCollection<Order>("orders");  // Collection for orders
            _productItemCollection = mongoDbService.Database.GetCollection<ProductItem>("productItems");  // Collection for products
            _mongoCollection = mongoDbService.Database.GetCollection<ApplicationUser>("users");

        }
      
        public async Task<IActionResult> Index()
        {
            // Count the number of users in the 'users' collection
            var userCount = (int)_mongoCollection.Count(_ => true);

            // Count the number of delivered orders in the 'orders' collection
            var ordersDelivered = (int)_orderCollection.Count(o => o.Status == "Confirmed");

            // Count the number of products in the 'productItems' collection
            var productCount = (int)_productItemCollection.Count(_ => true);

            var revenueResult = await _orderCollection.Aggregate()
     .Match(o => o.Status == "Confirmed")
     .Project(o => new
     {
         TotalAmountDecimal = o.TotalAmount // Assuming TotalAmount is a string or can be directly handled as a numeric type
     })
     .Group(
         key => 1,
         g => new
         {
             TotalRevenue = g.Sum(o => o.TotalAmountDecimal) // Make sure TotalAmount is a numeric value here
         })
     .FirstOrDefaultAsync();




            var totalRevenue = revenueResult?.TotalRevenue ?? 0;

            // Fetch a list of orders (for example, the latest 10 orders)
            var ordersList = _orderCollection.Find(o => true)
                                             .SortByDescending(o => o.CreatedAt)  // Sort by order date in descending order
                                               // Limit to the latest 10 orders
                                             .ToList();

            var model = new DashboardModel
            {
                UserCount = userCount,
                OrdersDelivered = ordersDelivered,
                ProductCount = productCount,
                TotalRevenue = totalRevenue,
                Orders = ordersList  // Add orders list to the model
            };
            return View(model);
        }
        // Action to change the order state (confirming an order)
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(string orderId)  // Assuming orderId is a string (GUID)
        {
            // Fetch the order from the database
            var order = await _orderCollection.Find(o => o.Id.ToString() == orderId).FirstOrDefaultAsync();

            if (order != null)
            {
                // Update the order state to "Confirmed"
                var updateDefinition = Builders<Order>.Update.Set(o => o.Status, "Confirmed");

                // Update the order in the collection
                await _orderCollection.UpdateOneAsync(o => o.Id.ToString() == orderId, updateDefinition);

                TempData["SuccessMessage"] = $"Order {orderId} confirmed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Order {orderId} not found!";
            }

            return RedirectToAction("Index");
        }

    }
}
