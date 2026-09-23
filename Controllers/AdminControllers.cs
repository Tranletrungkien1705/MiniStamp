using Microsoft.AspNetCore.Mvc;
using MiniStamp.Models;
using MiniStamp.Services;

namespace MiniStamp.Controllers;

public class HomeController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Dash = await svc.DashboardAsync();
        return View();
    }
}

public class ProductController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ProductsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? code, string? manufacturer, int warrantyMonths, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên sản phẩm."; return View(); }
        await svc.CreateProductAsync(new Product { Name = name.Trim(), Code = code ?? "", Manufacturer = manufacturer, WarrantyMonths = warrantyMonths, Description = description });
        TempData["Success"] = "Đã tạo sản phẩm.";
        return RedirectToAction(nameof(Index));
    }
}

public class BatchController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BatchesAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string lotNo, DateTime mfgDate, int quantity)
    {
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        var id = await svc.GenerateBatchAsync(new StampBatch { ProductId = productId, LotNo = lotNo ?? "", MfgDate = mfgDate == default ? DateTime.Today : mfgDate, CreatedBy = "web" }, quantity);
        TempData["Success"] = $"Đã sinh {quantity} tem QR.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var b = await svc.GetBatchAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }
}

public class StampController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, int? batchId)
    {
        ViewBag.Q = q; ViewBag.BatchId = batchId;
        return View(await svc.StampsAsync(q, batchId));
    }

    // Ảnh QR của 1 tem (mã hóa link tra cứu công khai)
    public IActionResult Qr(string code, int px = 6)
    {
        var url = $"{Request.Scheme}://{Request.Host}/Verify?code={code}";
        return File(QrService.PngBytes(url, px), "image/png");
    }
}
