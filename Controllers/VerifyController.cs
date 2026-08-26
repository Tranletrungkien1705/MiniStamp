using Microsoft.AspNetCore.Mvc;
using MiniStamp.Services;

namespace MiniStamp.Controllers;

/// <summary>Trang tra cứu tem CÔNG KHAI (người tiêu dùng quét QR) — không cần đăng nhập, xuyên tenant theo QrId.</summary>
public class VerifyController(IStampService svc) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return View((VerifyResult?)null);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await svc.VerifyAsync(code, ip);
        ViewBag.Code = code;
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> Activate(string code, string phone)
    {
        var (ok, msg) = await svc.ActivateAsync(code, phone);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index), new { code });
    }

    [HttpPost]
    public async Task<IActionResult> Spin(string code)
    {
        var (ok, prize) = await svc.SpinAsync(code);
        TempData[ok ? "Success" : "Error"] = ok ? $"🎉 Kết quả quay thưởng: {prize}" : prize;
        return RedirectToAction(nameof(Index), new { code });
    }
}
