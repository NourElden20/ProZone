using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ProZone.Models;  // Assuming this is where the models are defined
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProZone.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<ProductItem> _productItemCollection;

        // Constructor to inject MongoDB collections
        public OrderController(MongoDbService mongoDbService)
        {
            _orderCollection = mongoDbService.Database.GetCollection<Order>("orders");  // Collection for orders
            _cartCollection = mongoDbService.Database.GetCollection<Cart>("carts");  // Collection for carts
            _productCollection = mongoDbService.Database.GetCollection<Product>("products");  // Collection for products
            _productItemCollection = mongoDbService.Database.GetCollection<ProductItem>("productItems");  // Collection for products
        }

        [HttpPost]
        public async Task<IActionResult> Create(string userId)
        {
            // 1. Retrieve the cart for the user.
            var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

            if (cart == null || cart.CartItems.Count == 0)
            {
                return BadRequest("Cart is empty!");
            }

            // 2. Calculate the total amount of the order.
            decimal totalAmount = cart.CartItems.Sum(item => item.Price * item.Quantity);

            // 3. Update SoldTimes and ProductItem stock.
            foreach (var item in cart.CartItems)
            {
                // Get the ProductItem
                var productItem = await _productItemCollection.Find(pi => pi.Id == item.ProductItemId).FirstOrDefaultAsync();
                if (productItem == null) continue;

                // Get the Product
                var product = await _productCollection.Find(p => p.Id == productItem.ProductId).FirstOrDefaultAsync();
                if (product == null) continue;

                // Update SoldTimes
                product.SoldTimes += item.Quantity;

                // Update SKU for tracking
                item.SKU = productItem.SKU;

                // 🔽 Decrease stock of the product item
                productItem.StockQuantity -= item.Quantity;
                if (productItem.StockQuantity < 0) productItem.StockQuantity = 0; // optional: avoid negative stock

                // Save changes to Product and ProductItem
                await _productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                await _productItemCollection.ReplaceOneAsync(pi => pi.Id == productItem.Id, productItem);
            }

            // 4. Create the order object.
            var order = new Order
            {
                UserId = userId,
                Items = cart.CartItems,
                TotalAmount = totalAmount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            // 5. Insert the order into the Orders collection.
            await _orderCollection.InsertOneAsync(order);

            // 6. Clear the cart by deleting it.
            await _cartCollection.DeleteOneAsync(c => c.UserId == userId);

            // 7. Redirect the user to the main page
            return RedirectToAction("Index", "Home");
        }


        // PUT: Update order status (e.g., to "Approved", "Delivered", etc.)
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string newStatus)
        {
            // 1. Find the order by ID.
            var order = await _orderCollection.Find(o => o.Id == new ObjectId(orderId)).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // 2. Update the order status.
            order.Status = newStatus;
            await _orderCollection.ReplaceOneAsync(o => o.Id == order.Id, order);

            return Ok(new { OrderId = order.Id.ToString(), Status = order.Status });
        }

        // DELETE: Cancel order (only if status is not "Delivered")
        [HttpPost("CancelOrder/{orderId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            // 1. Find the order by ID.
            var order = await _orderCollection.Find(o => o.Id == new ObjectId(orderId)).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // 2. Check if the order has already been delivered (cannot cancel if delivered).
            if (order.Status == "Delivered")
            {
                return BadRequest("Order has already been delivered and cannot be canceled.");
            }

            // 3. Set the order status to "Canceled".
            order.Status = "Canceled";

            // 4. Update the order status in the database.
            await _orderCollection.ReplaceOneAsync(o => o.Id == order.Id, order);

            return RedirectToAction("GetOrders", "Order");
        }

        // GET: Get a user's orders
        [HttpGet]
        public async Task<IActionResult> GetOrders(string userId)
        {
            // 1. Retrieve all orders for a user.
            var orders = await _orderCollection.Find(o => o.UserId == userId).SortByDescending(o => o.CreatedAt).ToListAsync();

            // 2. Return the list of orders.
            return View(orders);
        }

        // GET: Get an order by its ID
        [HttpGet("Details")]
        public async Task<IActionResult> OrderDetails(string orderId)
        {
            // 1. Find the order by ID.
            var order = await _orderCollection.Find(o => o.Id == new ObjectId(orderId)).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // 2. Return the order details.
            return View(order);
        }
    }
}
