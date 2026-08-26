namespace MiniStamp.Models;

// ── Multi-tenant ─────────────────────────────────────────────────────
public class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
public interface IOrgOwned { Guid OrgId { get; set; } }

public enum StampStatus { Generated = 0, Activated = 1, Void = 2 }

// ── Sản phẩm ─────────────────────────────────────────────────────────
public class Product : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public int WarrantyMonths { get; set; } = 12;
}

// ── Lô tem (1 lần sinh tem cho 1 SP + lô SX) ─────────────────────────
public class StampBatch : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public int ProductId { get; set; }
    public string LotNo { get; set; } = "";
    public DateTime MfgDate { get; set; } = DateTime.Today;
    public int Quantity { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product Product { get; set; } = null!;
    public List<Stamp> Stamps { get; set; } = [];
}

// ── Tem (mỗi con tem = 1 QR duy nhất toàn cục) ───────────────────────
public class Stamp : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string QrId { get; set; } = "";        // mã QR duy nhất TOÀN CỤC (người tiêu dùng quét)
    public string Pin { get; set; } = "";         // mã PIN cào (xác thực phụ)
    public int BatchId { get; set; }
    public int ProductId { get; set; }
    public StampStatus Status { get; set; } = StampStatus.Generated;

    // kích hoạt bảo hành (người dùng cuối)
    public DateTime? ActivatedAt { get; set; }
    public string? ActivatedPhone { get; set; }
    public DateTime? WarrantyEnd { get; set; }

    // chống giả: đếm số lần quét + mốc
    public int ScanCount { get; set; }
    public DateTime? FirstScanAt { get; set; }
    public DateTime? LastScanAt { get; set; }

    // quay thưởng
    public bool HasSpun { get; set; }
    public string? PrizeWon { get; set; }

    public StampBatch Batch { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// ── Nhật ký quét (truy vết) ──────────────────────────────────────────
public class ScanLog : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int StampId { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.Now;
    public string? Ip { get; set; }
    public string Result { get; set; } = "";   // Genuine / Suspicious / ...
}

// ── Quà quay thưởng ──────────────────────────────────────────────────
public class LotteryReward : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = "";
    public int Weight { get; set; } = 1;    // trọng số xác suất
    public int Stock { get; set; } = 100;
    public bool IsLose { get; set; }        // ô "Chúc bạn may mắn lần sau"
}
