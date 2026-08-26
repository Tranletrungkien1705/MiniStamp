using Microsoft.EntityFrameworkCore;
using MiniStamp.Data;
using MiniStamp.Models;

namespace MiniStamp.Services;

public record StampDash(int Products, int Batches, int Stamps, int Activated, int Scans,
    List<(string Product, int Count)> ByProduct);

/// <summary>Kết quả tra cứu công khai 1 con tem.</summary>
public record VerifyResult(bool Found, bool Genuine, string Title, string Message,
    Stamp? Stamp, Product? Product, StampBatch? Batch, List<string> Warnings);

public interface IStampService
{
    // admin
    Task<List<Product>> ProductsAsync();
    Task<Product?> GetProductAsync(int id);
    Task<int> CreateProductAsync(Product p);
    Task<List<StampBatch>> BatchesAsync();
    Task<StampBatch?> GetBatchAsync(int id);
    Task<int> GenerateBatchAsync(StampBatch batch, int quantity);
    Task<List<Stamp>> StampsAsync(string? q, int? batchId);
    Task<List<LotteryReward>> RewardsAsync();
    Task<StampDash> DashboardAsync();
    // consumer (công khai, xuyên tenant theo QrId)
    Task<VerifyResult> VerifyAsync(string qrId, string? ip);
    Task<(bool ok, string msg)> ActivateAsync(string qrId, string phone);
    Task<(bool ok, string prize)> SpinAsync(string qrId);
}

public class StampService(AppDbContext db) : IStampService
{
    // ── ADMIN ────────────────────────────────────────────────────────
    public Task<List<Product>> ProductsAsync() => db.Products.OrderBy(p => p.Code).ToListAsync();
    public Task<Product?> GetProductAsync(int id) => db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreateProductAsync(Product p)
    {
        if (string.IsNullOrWhiteSpace(p.Code)) p.Code = $"SP{await db.Products.CountAsync() + 1:D3}";
        db.Products.Add(p);
        await db.SaveChangesAsync();
        return p.Id;
    }

    public Task<List<StampBatch>> BatchesAsync() =>
        db.Batches.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<StampBatch?> GetBatchAsync(int id) =>
        db.Batches.Include(x => x.Product).Include(x => x.Stamps).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<int> GenerateBatchAsync(StampBatch batch, int quantity)
    {
        quantity = Math.Clamp(quantity, 1, 5000);
        batch.Code = $"LOT{DateTime.Now:yyMMddHHmm}{await db.Batches.CountAsync() + 1:D2}";
        batch.Quantity = quantity;
        for (int i = 0; i < quantity; i++)
            batch.Stamps.Add(new Stamp
            {
                ProductId = batch.ProductId,
                QrId = NewQrId(),
                Pin = Random.Shared.Next(100000, 999999).ToString(),
                Status = StampStatus.Generated
            });
        db.Batches.Add(batch);
        await db.SaveChangesAsync();
        return batch.Id;
    }

    public async Task<List<Stamp>> StampsAsync(string? q, int? batchId)
    {
        var query = db.Stamps.Include(s => s.Product).Include(s => s.Batch).AsQueryable();
        if (batchId.HasValue) query = query.Where(s => s.BatchId == batchId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(s => s.QrId.Contains(q) || s.Pin == q);
        return await query.OrderByDescending(s => s.Id).Take(500).ToListAsync();
    }

    public Task<List<LotteryReward>> RewardsAsync() => db.Rewards.OrderByDescending(r => r.Weight).ToListAsync();

    public async Task<StampDash> DashboardAsync()
    {
        var stamps = await db.Stamps.Include(s => s.Product).ToListAsync();
        var byProduct = stamps.GroupBy(s => s.Product.Name).Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Item2).Take(6).ToList();
        return new StampDash(
            await db.Products.CountAsync(),
            await db.Batches.CountAsync(),
            stamps.Count,
            stamps.Count(s => s.Status == StampStatus.Activated),
            await db.ScanLogs.CountAsync(),
            byProduct);
    }

    // ── CONSUMER (công khai) ─────────────────────────────────────────
    public async Task<VerifyResult> VerifyAsync(string qrId, string? ip)
    {
        qrId = (qrId ?? "").Trim();
        var s = await db.Stamps.IgnoreQueryFilters()
            .Include(x => x.Product).Include(x => x.Batch)
            .FirstOrDefaultAsync(x => x.QrId == qrId);

        if (s == null)
            return new VerifyResult(false, false, "KHÔNG XÁC THỰC ĐƯỢC",
                "Mã tem không tồn tại trong hệ thống — sản phẩm có thể là HÀNG GIẢ.", null, null, null, []);

        // ghi nhận lượt quét (đếm + log) — chống giả bằng số lần/địa điểm quét bất thường
        s.ScanCount++;
        s.FirstScanAt ??= DateTime.Now;
        s.LastScanAt = DateTime.Now;
        db.ScanLogs.Add(new ScanLog { OrgId = s.OrgId, StampId = s.Id, Ip = ip, Result = "Scan" });

        var warnings = new List<string>();
        if (s.Status == StampStatus.Void) warnings.Add("Tem đã bị thu hồi/vô hiệu.");
        if (s.ScanCount > 20) warnings.Add($"Tem này đã được quét {s.ScanCount} lần — bất thường, cảnh giác hàng giả sao chép mã.");
        if (s.ActivatedAt != null) warnings.Add($"Tem đã được kích hoạt bảo hành ngày {s.ActivatedAt:dd/MM/yyyy}.");

        var genuine = s.Status != StampStatus.Void;
        await db.SaveChangesAsync();

        return new VerifyResult(true, genuine,
            genuine ? "SẢN PHẨM CHÍNH HÃNG" : "TEM ĐÃ VÔ HIỆU",
            genuine ? "Tem hợp lệ do nhà sản xuất phát hành." : "Tem này đã bị thu hồi.",
            s, s.Product, s.Batch, warnings);
    }

    public async Task<(bool ok, string msg)> ActivateAsync(string qrId, string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return (false, "Cần số điện thoại.");
        var s = await db.Stamps.IgnoreQueryFilters().Include(x => x.Product).Include(x => x.Batch)
            .FirstOrDefaultAsync(x => x.QrId == qrId.Trim());
        if (s == null) return (false, "Mã tem không tồn tại.");
        if (s.Status == StampStatus.Void) return (false, "Tem đã vô hiệu.");
        if (s.ActivatedAt != null) return (false, $"Tem đã kích hoạt bảo hành ngày {s.ActivatedAt:dd/MM/yyyy}.");

        s.ActivatedAt = DateTime.Now;
        s.ActivatedPhone = phone.Trim();
        s.Status = StampStatus.Activated;
        s.WarrantyEnd = DateTime.Today.AddMonths(s.Product?.WarrantyMonths ?? 12);
        await db.SaveChangesAsync();
        return (true, $"Kích hoạt bảo hành thành công — hết hạn {s.WarrantyEnd:dd/MM/yyyy}.");
    }

    public async Task<(bool ok, string prize)> SpinAsync(string qrId)
    {
        var s = await db.Stamps.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.QrId == qrId.Trim());
        if (s == null) return (false, "Mã tem không tồn tại.");
        if (s.Status == StampStatus.Void) return (false, "Tem đã vô hiệu.");
        if (s.HasSpun) return (false, $"Tem này đã quay thưởng: {s.PrizeWon}.");

        var rewards = await db.Rewards.IgnoreQueryFilters().Where(r => r.OrgId == s.OrgId && (r.IsLose || r.Stock > 0)).ToListAsync();
        if (rewards.Count == 0) return (false, "Chương trình chưa cấu hình quà.");

        var total = rewards.Sum(r => r.Weight);
        var roll = Random.Shared.Next(0, total);
        LotteryReward? picked = null;
        foreach (var r in rewards) { if (roll < r.Weight) { picked = r; break; } roll -= r.Weight; }
        picked ??= rewards[0];

        s.HasSpun = true;
        s.PrizeWon = picked.Name;
        if (!picked.IsLose && picked.Stock > 0) picked.Stock--;
        await db.SaveChangesAsync();
        return (true, picked.Name);
    }

    private static string NewQrId()
        => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();   // 12 ký tự hex — ngắn gọn cho QR
}
