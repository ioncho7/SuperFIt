using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperFit.Data;
using SuperFit.Models;

namespace SuperFit.Controllers;

[Authorize] // само логнати могат да поръчват
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromProduct(int productId, int quantity)
    {
        if (quantity < 1)
            return BadRequest();

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound();

        if (product.StockQty < quantity)
        {
            TempData["Msg"] = "Няма достатъчна наличност за избраното количество.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        var userId = _userManager.GetUserId(User)!;

        var order = new Order
        {
            UserId = userId,
            CreatedOn = DateTime.UtcNow,
            Status = OrderStatus.Pending,
        };

        var item = new OrderItem
        {
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price
        };

        order.Items.Add(item);
        order.TotalPrice = item.UnitPrice * item.Quantity;

        // намаляваме наличността
        product.StockQty -= quantity;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // ✅ УВЕДОМЛЕНИЕ КЪМ АДМИНА (в ContactMessages)
        var user = await _userManager.GetUserAsync(User);
        var email = user?.Email ?? "неизвестен";

        _context.ContactMessages.Add(new ContactMessage
        {
            Name = user.DisplayName ?? user.Email,
            Email = email,
            Subject = "Нова поръчка",
            Message = $"Потребител: {email}\nПоръчка №: {order.Id}\nПродукт: {product.Name}\nКоличество: {quantity}\nСума: {order.TotalPrice:0.00} €",
            SentOn = DateTime.UtcNow,

            OrderId = order.Id,
            IsOrderNotification = true
        });

        await _context.SaveChangesAsync();


        TempData["Msg"] = "Поръчката е създадена успешно!";
        return RedirectToAction("MyOrders");
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = _userManager.GetUserId(User)!;

        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();

        return View(orders);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null) return NotFound();

        // ако искаш да позволиш изтриване само на Pending:
        // if (order.Status != OrderStatus.Pending) return Forbid();

        _context.OrderItems.RemoveRange(order.Items);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        TempData["Msg"] = "Поръчката беше изтрита.";
        return RedirectToAction(nameof(MyOrders));
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        order.Status = OrderStatus.Approved;

        await _context.SaveChangesAsync();

        TempData["MsgAdmin"] = $"Поръчка №{order.Id} беше одобрена.";
        return RedirectToAction(nameof(AdminOrders));
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminOrderDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveFromMessage(int orderId, int messageId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return NotFound();

        order.Status = OrderStatus.Approved;
        await _context.SaveChangesAsync();

        TempData["Msg"] = $"Поръчка №{order.Id} беше одобрена.";

        // (по желание) може да сменим subject-а за да се вижда като обработена
        var msg = await _context.ContactMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (msg != null)
        {
            msg.Subject = "Поръчка одобрена";
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "ContactMessages", new { id = messageId });
    }


}
