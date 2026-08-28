using Microsoft.AspNetCore.Mvc;
using MiniStamp.Data;
using MiniStamp.Models;
using MiniStamp.Services;

namespace MiniStamp.Controllers;

/// <summary>
/// API JSON cho SPA React (admin). DTO phẳng. Dashboard cache Redis 30s theo tenant (X-Cache).
/// Verify/activate/spin xuyên tenant theo QrId (người tiêu dùng) — trang /Verify (Razor) + QR PNG giữ nguyên.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiV1Controller(IStampService svc, ICache cache, ITenantContext tenant) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var key = $"stamp:dash:{tenant.OrgId}";
        var hit = await cache.GetAsync<DashDto>(key);
        if (hit != null) { Response.Headers["X-Cache"] = "HIT"; return Ok(hit); }
        var d = await svc.DashboardAsync();
        var dto = new DashDto(d.Products, d.Batches, d.Stamps, d.Activated, d.Scans,
            d.ByProduct.Select(x => new ByProductDto(x.Product, x.Count)).ToList());
        await cache.SetAsync(key, dto, TimeSpan.FromSeconds(30));
        Response.Headers["X-Cache"] = "MISS";
        return Ok(dto);
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products()
        => Ok((await svc.ProductsAsync()).Select(p => new { p.Id, p.Code, p.Name, p.Manufacturer, p.Description, p.WarrantyMonths }));

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest(new { error = "Cần tên sản phẩm." });
        var id = await svc.CreateProductAsync(new Product { Name = r.Name.Trim(), Code = r.Code ?? "", Manufacturer = r.Manufacturer, Description = r.Description, WarrantyMonths = r.WarrantyMonths <= 0 ? 12 : r.WarrantyMonths });
        return Ok(new { id });
    }

    [HttpGet("batches")]
    public async Task<IActionResult> Batches()
        => Ok((await svc.BatchesAsync()).Select(b => new { b.Id, b.Code, b.ProductId, product = b.Product?.Name, b.LotNo, b.MfgDate, b.Quantity, b.CreatedAt }));

    [HttpGet("batches/{id:int}")]
    public async Task<IActionResult> Batch(int id)
    {
        var b = await svc.GetBatchAsync(id);
        if (b == null) return NotFound(new { error = "Không tìm thấy lô." });
        return Ok(new
        {
            b.Id, b.Code, product = b.Product?.Name, b.LotNo, b.MfgDate, b.Quantity,
            activated = b.Stamps.Count(s => s.Status == StampStatus.Activated),
            stamps = b.Stamps.Take(200).Select(s => new { s.Id, s.QrId, s.Pin, status = (int)s.Status, statusText = Ui.Status(s.Status).text, s.ScanCount })
        });
    }

    [HttpPost("batches")]
    public async Task<IActionResult> Generate([FromBody] BatchReq r)
    {
        if (r.ProductId <= 0) return BadRequest(new { error = "Cần chọn sản phẩm." });
        var id = await svc.GenerateBatchAsync(new StampBatch { ProductId = r.ProductId, LotNo = r.LotNo ?? "", MfgDate = r.MfgDate == default ? DateTime.Today : r.MfgDate, CreatedBy = "api" }, r.Quantity);
        return Ok(new { id });
    }

    [HttpGet("stamps")]
    public async Task<IActionResult> Stamps([FromQuery] string? q, [FromQuery] int? batchId)
        => Ok((await svc.StampsAsync(q, batchId)).Select(s => new
        {
            s.Id, s.QrId, s.Pin, product = s.Product?.Name, batch = s.Batch?.Code,
            status = (int)s.Status, statusText = Ui.Status(s.Status).text, s.ScanCount, s.ActivatedPhone, s.WarrantyEnd, s.PrizeWon
        }));

    [HttpGet("rewards")]
    public async Task<IActionResult> Rewards()
        => Ok((await svc.RewardsAsync()).Select(r => new { r.Id, r.Name, r.Weight, r.Stock, r.IsLose }));

    // ── Consumer (xuyên tenant theo QrId) ──
    [HttpGet("verify/{code}")]
    public async Task<IActionResult> Verify(string code)
    {
        var v = await svc.VerifyAsync(code, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new
        {
            v.Found, v.Genuine, v.Title, v.Message, v.Warnings,
            product = v.Product == null ? null : new { v.Product.Name, v.Product.Manufacturer, v.Product.WarrantyMonths },
            batch = v.Batch == null ? null : new { v.Batch.LotNo, v.Batch.MfgDate },
            stamp = v.Stamp == null ? null : new { v.Stamp.ScanCount, v.Stamp.Status, activated = v.Stamp.ActivatedAt, warrantyEnd = v.Stamp.WarrantyEnd, hasSpun = v.Stamp.HasSpun, prize = v.Stamp.PrizeWon }
        });
    }

    [HttpPost("verify/{code}/activate")]
    public async Task<IActionResult> Activate(string code, [FromBody] ActivateReq r)
    {
        var (ok, msg) = await svc.ActivateAsync(code, r.Phone ?? "");
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, error = msg });
    }

    [HttpPost("verify/{code}/spin")]
    public async Task<IActionResult> Spin(string code)
    {
        var (ok, prize) = await svc.SpinAsync(code);
        return ok ? Ok(new { ok, prize }) : BadRequest(new { ok, error = prize });
    }
}

public record DashDto(int Products, int Batches, int Stamps, int Activated, int Scans, List<ByProductDto> ByProduct);
public record ByProductDto(string Product, int Count);

public class ProductReq { public string Name { get; set; } = ""; public string? Code { get; set; } public string? Manufacturer { get; set; } public string? Description { get; set; } public int WarrantyMonths { get; set; } = 12; }
public class BatchReq { public int ProductId { get; set; } public string? LotNo { get; set; } public DateTime MfgDate { get; set; } public int Quantity { get; set; } = 100; }
public class ActivateReq { public string? Phone { get; set; } }
