using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStamp.Data;
using MiniStamp.Models;

namespace MiniStamp.Controllers;

public class OrgController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orgs = await db.Orgs.IgnoreQueryFilters().OrderBy(o => o.CreatedAt).ToListAsync();
        Request.Cookies.TryGetValue(TenantContext.CookieName, out var curKey);
        ViewBag.CurrentKey = curKey ?? TenantContext.DefaultApiKey;
        return View(orgs);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên tổ chức."; return RedirectToAction(nameof(Index)); }
        var org = new Org { Name = name.Trim(), ApiKey = "stp_" + Guid.NewGuid().ToString("N") };
        db.Orgs.Add(org);
        await db.SaveChangesAsync();
        SetCookies(org.ApiKey, org.Name);
        TempData["Success"] = $"Đã tạo & chuyển sang nhà sản xuất \"{org.Name}\".";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(string apiKey)
    {
        var org = await db.Orgs.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.ApiKey == apiKey);
        if (org == null) { TempData["Error"] = "Không tìm thấy tổ chức."; return RedirectToAction(nameof(Index)); }
        SetCookies(org.ApiKey, org.Name);
        TempData["Success"] = $"Đang xem nhà sản xuất \"{org.Name}\".";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Reset()
    {
        Response.Cookies.Delete(TenantContext.CookieName);
        Response.Cookies.Delete("org_name");
        TempData["Success"] = "Đã về nhà sản xuất mặc định (Demo Stamp).";
        return RedirectToAction("Index", "Home");
    }

    private void SetCookies(string apiKey, string name)
    {
        var opt = new CookieOptions { HttpOnly = false, IsEssential = true, Expires = DateTimeOffset.UtcNow.AddDays(30) };
        Response.Cookies.Append(TenantContext.CookieName, apiKey, opt);
        Response.Cookies.Append("org_name", name, opt);
    }
}
