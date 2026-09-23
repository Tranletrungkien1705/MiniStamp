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

public enum StampStatus { Generated = 0, Activated = 1, Void = 2, Broken = 3 }

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

// ── Hộp (đóng gói N tem con vào 1 hộp — nghiệp vụ Map_IDInBox của EQR) ─
public class Box : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string BoxNo { get; set; } = "";       // mã hộp (duy nhất trong tenant)
    public int ProductId { get; set; }
    public int Quantity { get; set; }              // số tem đã đóng vào hộp
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // đóng thùng: hộp thuộc thùng nào (null = chưa đóng thùng)
    public int? CartonId { get; set; }
    public DateTime? CartonedAt { get; set; }

    public Product Product { get; set; } = null!;
    public Carton? Carton { get; set; }
    public List<Stamp> Stamps { get; set; } = [];
}

// ── Thùng (đóng gói N hộp vào 1 thùng — nghiệp vụ Map_Can của EQR) ────
public class Carton : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CanNo { get; set; } = "";        // mã thùng (duy nhất trong tenant)
    public int ProductId { get; set; }
    public int BoxCount { get; set; }              // số hộp đã đóng vào thùng
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product Product { get; set; } = null!;
    public List<Box> Boxes { get; set; } = [];
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

    // đóng gói: tem thuộc hộp nào (null = chưa đóng hộp)
    public int? BoxId { get; set; }
    public DateTime? BoxedAt { get; set; }

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
    public Box? Box { get; set; }
}

// ── Phiếu tem rách/vỡ (NG) — nghiệp vụ InvF_BrokenStamp của EQR ──────
// Ghi nhận các tem bị lỗi trong quá trình sản xuất/đóng gói để loại khỏi
// vòng đời (đánh dấu StampStatus.Broken). 1 phiếu = header + N dòng tem.
public class BrokenStamp : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string BsNo { get; set; } = "";        // mã phiếu (duy nhất trong tenant)
    public int ProductId { get; set; }
    public int Quantity { get; set; }              // số tem lỗi đã ghi nhận
    public string Status { get; set; } = "PENDING"; // PENDING / APPROVED
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public MiniStamp.Models.Product Product { get; set; } = null!;
    public List<BrokenStampLine> Lines { get; set; } = [];
}

// ── Dòng chi tiết phiếu tem lỗi (1 tem rách/vỡ) ──────────────────────
public class BrokenStampLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int BrokenStampId { get; set; }
    public string QrId { get; set; } = "";        // mã tem bị lỗi
    public DateTime BrokenAt { get; set; } = DateTime.Now;

    public BrokenStamp BrokenStamp { get; set; } = null!;
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
