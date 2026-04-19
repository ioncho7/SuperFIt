using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperFit.Data;
using SuperFit.Models;

namespace SuperFit.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;

    public ContactController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactMessage());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Index(ContactMessage model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.ContactMessages.Add(model);
        await _context.SaveChangesAsync();

        ViewBag.Success = true;
        return View(new ContactMessage());
    }
}
