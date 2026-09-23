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

    // xuất kho theo tem (Inv_VerifiedIDInOut) — tem đã xuất bán cho khách nào
    public int? ShipmentId { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? CustomerCode { get; set; }

    // quay thưởng
    public bool HasSpun { get; set; }
    public string? PrizeWon { get; set; }

    public StampBatch Batch { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Box? Box { get; set; }
    public Shipment? Shipment { get; set; }
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

// ── Phiếu xuất kho theo tem (Inv_VerifiedIDInOut) — nghiệp vụ
//    Inv_InvVerifiedID_OutGenInAndOut của EQR (xuất-ghép tem theo phiếu).
//    1 phiếu = header (vận chuyển/khách/đơn hàng nguồn) + N dòng tem đã xuất.
public class Shipment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ShipmentNo { get; set; } = "";     // mã phiếu xuất (IVerifiedIDInOutNo, duy nhất trong tenant)
    public string Status { get; set; } = "PENDING";   // PENDING / SHIPPED / CANCEL

    // đơn hàng nguồn
    public string? RefNoSys { get; set; }
    public string? RefNo { get; set; }
    public string? RefType { get; set; }

    // vận chuyển
    public string? PlateNo { get; set; }              // biển số xe
    public string? MoocNo { get; set; }               // số mooc
    public string? DriverName { get; set; }
    public string? DriverPhoneNo { get; set; }
    public string? TransportType { get; set; }
    public string? ReceivePlace { get; set; }         // nơi nhận

    // khách hàng
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }

    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ShippedAt { get; set; }
    public string? ShippedBy { get; set; }

    public List<ShipmentLine> Lines { get; set; } = [];
}

// ── Dòng chi tiết phiếu xuất (1 tem đã xuất) ─────────────────────────
public class ShipmentLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ShipmentId { get; set; }
    public int ProductId { get; set; }
    public string QrId { get; set; } = "";           // mã tem đã xuất
    public DateTime ShippedAt { get; set; } = DateTime.Now;

    public Shipment Shipment { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// ── Nhật ký quét (truy vết) ──────────────────────────────────────────
public class InventoryInFG : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string InvInNo { get; set; } = "";       // mã phiếu nhập (duy nhất trong tenant)
    public string Status { get; set; } = "PENDING";  // PENDING / APPROVE / CANCEL
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }

    public List<InventoryInFGDtl> Lines { get; set; } = [];
}

// ── Dòng chi tiết phiếu nhập kho (1 sản phẩm + số lượng + ngày SX) ────
public class InventoryInFGDtl : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int InventoryInFGId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }                     // số lượng nhập
    public DateTime ProductionDate { get; set; } = DateTime.Today;

    public InventoryInFG InventoryInFG { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// ── Hạn sử dụng (Mst_ProductLife) — danh mục master data ─────────────
// Nghiệp vụ EQR: mỗi sản phẩm có 1 "hạn sử dụng" (ProductLifeCode).
// ProductLifeValue = số đơn vị (ngày/tuần/tháng), ProductLifeValueByDay = quy đổi ra ngày.
public class ProductLife : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // ProductLifeCode — vd 1MONTH
    public string Name { get; set; } = "";        // ProductLifeName — vd "1 tháng"
    public string Type { get; set; } = "DAY";     // DAY / WEEK / MONTH
    public int Value { get; set; } = 1;            // ProductLifeValue
    public int ValueByDay { get; set; } = 1;       // ProductLifeValueByDay (quy đổi ra ngày)
}

// ── Kích hoạt thông tin sản xuất (InvF_ProductionActive) ─────────────
// Nghiệp vụ EQR: ghi nhận 1 lần kích hoạt sản xuất cho 1 lô tem — gắn
// nguồn gốc (Origin), hạn sử dụng (ProductLifeCode) và dải serial tem vào/ra.
// Ràng buộc: RefNo bắt buộc + duy nhất; Origin bắt buộc; sản phẩm + hạn dùng phải tồn tại;
// Ngày hết hạn = Ngày SX + ProductLifeValue - 1; chỉ xóa trong 72h kể từ khi tạo.
public class ProductionActive : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaNo { get; set; } = "";        // IF_PANo — mã phiếu kích hoạt (duy nhất trong tenant)
    public string RefNo { get; set; } = "";       // số tham chiếu (bắt buộc, duy nhất)
    public string Origin { get; set; } = "";      // nguồn gốc (tên trang trại / mã nguồn gốc)
    public int ProductId { get; set; }
    public int QtyPlan { get; set; }               // số lượng kế hoạch
    public DateTime ProductDate { get; set; } = DateTime.Today;   // ngày sản xuất
    public DateTime ExpiryDate { get; set; } = DateTime.Today;    // ngày hết hạn (suy ra)
    public int ProductLifeId { get; set; }         // hạn sử dụng đã chọn
    public string ListSerialInManufacture { get; set; } = "";   // dải serial tem vào SX
    public string ListSerialOutManufacture { get; set; } = "";  // dải serial tem ra SX
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product Product { get; set; } = null!;
    public ProductLife ProductLife { get; set; } = null!;
}

// ── Danh mục Nguồn gốc (Mst_NguonGoc) — master data ──────────────────
// Nghiệp vụ EQR: danh mục nguồn gốc (trang trại/vùng trồng) dùng cho trường
// "Nguồn gốc" ở màn Kích hoạt thông tin sản xuất, kèm thông tin chứng chỉ
// (VietGAP/GlobalGAP…) và hỗ trợ Auto Complete.
// Ràng buộc: Code bắt buộc + duy nhất; Name bắt buộc; DateStart <= DateEnd;
// DisplayName do server tự dựng = "<Code> (<CertCode> <CertNo>)" (rỗng chứng chỉ ⇒ = Code);
// chặn xóa nếu đã được dùng làm Origin của 1 phiếu kích hoạt SX.
public class OriginCatalog : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";            // NguonGocCode — vd CP1
    public string Name { get; set; } = "";            // NguonGocName — tên trang trại
    public string DisplayName { get; set; } = "";     // chuỗi gợi ý (server tự dựng)
    public string? CertificateCode { get; set; }       // loại chứng chỉ — vd VietGAP
    public string? CertificateNo { get; set; }         // số chứng chỉ — vd 1030
    public string? CertificateName { get; set; }       // tên đầy đủ chứng chỉ
    public DateTime? CertificateDateStart { get; set; }
    public DateTime? CertificateDateEnd { get; set; }
    public string? Address { get; set; }               // địa chỉ trang trại
    public string? GlnCode { get; set; }               // GLNCode — mã địa điểm GS1
    public string? Remark { get; set; }
    public bool IsActive { get; set; } = true;         // FlagActive
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── Nhật ký quét (truy vết) ──────────────────────────
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
