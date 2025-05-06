using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ProZone.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

[Authorize]
public class CartController : Controller
{
    private readonly IMongoCollection<Cart> _cartCollection;
    private readonly IMongoCollection<ProductItem> _productItemCollection;

    // Constructor: Initialize MongoDB collection for "carts"
    public CartController(MongoDbService mongoDbService)
    {
        _cartCollection = mongoDbService.Database.GetCollection<Cart>("carts");
        _productItemCollection = mongoDbService.Database.GetCollection<ProductItem>("productItems");
    }

    // Helper method to get currently authenticated user's ID
    private string GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userIdClaim;  // Assuming the UserId is stored as ObjectId
    }



    // POST: /Cart/AddToCart
    // POST: /Cart/AddToCart
    [HttpPost]
    public async Task<IActionResult> AddToCart(ObjectId productItemId)
    {
        var userId = GetUserId();

        // Retrieve the product item from the product collection
        var productItem = await _productItemCollection.Find(c => c.Id == productItemId).FirstOrDefaultAsync();
        if (productItem == null)
        {
            return NotFound("Product not found");
        }

        // Check if there's enough stock available
        if (productItem.StockQuantity < 1)
        {
            return Conflict("Sorry, this product is out of stock.");
        }

        // Check if user already has a cart
        var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

        if (cart == null)
        {
            // If no cart exists, create a new cart and add the item
            cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>
            {
                new CartItem
                {
                    ProductItemId = productItemId,
                    SKU = productItem.SKU,
                    Quantity = 1,
                    Price = productItem.Price,
                    Photo = productItem.Photo
                }
            }
            };

            await _cartCollection.InsertOneAsync(cart);
        }
        else
        {
            // If cart exists, check if item already exists (same product and variant)
            var existingItem = cart.CartItems.FirstOrDefault(x =>
                x.ProductItemId == productItemId && x.SKU == productItem.SKU);

            if (existingItem != null)
            {
                // Check if enough stock is available for the requested increase in quantity
                if (existingItem.Quantity + 1 > productItem.StockQuantity)
                {
                    return Conflict("Not enough stock available.");
                }

                // If item exists, increment its quantity
                existingItem.Quantity++;
            }
            else
            {
                // If not, add it as a new item
                cart.CartItems.Add(new CartItem
                {
                    ProductItemId = productItemId,
                    SKU = productItem.SKU,
                    Quantity = 1,
                    Price = productItem.Price,
                    Photo = productItem.Photo
                });
            }

            // Replace the cart with updated item list
            await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        // Redirect back to cart view after adding
        return RedirectToAction("Index", "Cart");
    }


    // GET: /Cart/Index
    [HttpGet("/Cart")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

        if (cart == null)
            return View(new List<CartItemWithStock>());

        var result = new List<CartItemWithStock>();

        foreach (var item in cart.CartItems)
        {
            var productItem = await _productItemCollection.Find(p => p.Id == item.ProductItemId).FirstOrDefaultAsync();

            result.Add(new CartItemWithStock
            {
                CartItem = item,
                StockQuantity = productItem?.StockQuantity ?? 0
            });
        }

        return View(result);
    }


    // POST: /Cart/DeleteItem
    [HttpPost]
    public async Task<IActionResult> DeleteItem(ObjectId productItemId)
    {
        var userId = GetUserId();
        var productItem = await _productItemCollection.Find(c => c.Id == productItemId).FirstOrDefaultAsync();

        // Fetch user's cart
        var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

        if (cart != null)
        {
            // Remove specific item (by productId and variant) from the cart
            cart.CartItems.RemoveAll(x => x.ProductItemId == productItemId && x.SKU == productItem.SKU);

            // Update cart document after deletion
            await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/EmptyCart
    [HttpPost]
    public async Task<IActionResult> EmptyCart()
    {
        var userId = GetUserId();

        // Delete entire cart document for the user
        await _cartCollection.DeleteOneAsync(c => c.UserId == userId);

        return RedirectToAction("Index");
    }

   
    // POST: /Cart/UpdateQuantity
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(ObjectId productItemId, int quantity)
    {
        var userId = GetUserId();
        var productItem = await _productItemCollection.Find(c => c.Id == productItemId).FirstOrDefaultAsync();

        // Retrieve user's cart
        var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

        if (productItem == null)
        {
            return NotFound("Product not found");
        }

        // Check if the requested quantity is available in stock
        if (quantity > productItem.StockQuantity)
        {
            return Conflict("Not enough stock available.");
        }

        if (cart != null)
        {
            // Find the specific item and update its quantity
            var item = cart.CartItems.FirstOrDefault(x =>
                x.ProductItemId == productItemId && x.SKU == productItem.SKU);

            if (item != null)
            {
                item.Quantity = quantity;

                // Save changes back to database
                await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            }
        }

        return Ok();
    }

}
