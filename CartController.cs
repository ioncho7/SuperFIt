using Microsoft.AspNetCore.Mvc;
using SuperFit.Data;
using SuperFit.Helper;
using SuperFit.Models;
using System.Security.Claims;

namespace SuperFit.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            return View(cart);
        }


        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.Products.Find(productId);

            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }


            var order = new Order
            {
                UserId = userId,
                CreatedOn = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = cart.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.Price
                }).ToList()
            };

            order.TotalPrice = order.Items.Sum(x => x.Quantity * x.UnitPrice);

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);

                if (product != null)
                {
                    product.StockQty -= item.Quantity;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var message = new ContactMessage
            {
                Name = user.DisplayName ?? user.Email,
                Email = user?.Email ?? "no-email",
                Subject = "Нова поръчка",
                Message = $"Потребител направи поръчка #{order.Id}\nОбща сума: {order.TotalPrice} €\nБрой продукти: {order.Items.Count}",
                SentOn = DateTime.UtcNow
            };

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("MyOrders", "Orders");
        }

        [HttpPost]
        public IActionResult Increase(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                item.Quantity++;

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                quantity = item?.Quantity,
                total = cart.Sum(x => x.Price * x.Quantity)
            });
        }

        [HttpPost]
        public IActionResult Decrease(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                    cart.Remove(item);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                removed = item == null || item.Quantity <= 0,
                quantity = item?.Quantity ?? 0,
                total = cart.Sum(x => x.Price * x.Quantity)
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                cart.Remove(item);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                removed = true,
                total = cart.Sum(x => x.Price * x.Quantity)
            });
        }

    }
}