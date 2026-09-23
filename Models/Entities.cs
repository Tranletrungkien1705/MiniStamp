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

// ── Ghép cặp tem (Map_StampPair) — nghiệp vụ EQR ─────────────────────
// Nghiệp vụ EQR (WAS_Map_StampPair_Add_New20220415, file Template.cs):
// ghép 1 tem CHÍNH (MainQrId) với 1 tem PHỤ (SubQrId) thành 1 cặp 1:1.
// Dùng cho tem đôi (tem chính + tem phụ dán kèm) — mỗi tem chỉ được
// thuộc tối đa 1 cặp. Ràng buộc EQR:
//  - Danh sách đầu vào không được trùng tem chính (IDNoInputNotUnique).
//  - Danh sách đầu vào không được trùng tem phụ (BoxNoInputNotUnique).
//  - Cả 2 tem phải tồn tại trong hệ thống (IDNoNotExistInInvGen).
//  - Tem chính/phụ chưa được ghép cặp trước đó (ExistIDNoInOtherBox).
public class StampPair : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string MainQrId { get; set; } = "";   // tem chính (IDNo) — duy nhất trong tenant
    public string SubQrId { get; set; } = "";    // tem phụ (BoxNo) — duy nhất trong tenant
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
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

    // kích hoạt bảo hành bằng PIN (WarrantyDateStartFromPIN_Activate) — nghiệp vụ EQR
    public string? WarrantyNo { get; set; }          // WarrantyNo — số phiếu bảo hành (cấp lần đầu, giữ nguyên khi kích hoạt lại)
    public int WarrantyCount { get; set; }            // WarrantyCount — số lần kích hoạt bảo hành
    public string? WarrantyStartIp { get; set; }      // IPAddress — IP người kích hoạt
    public string? WarrantyLat { get; set; }          // MapLatitude — vĩ độ lúc kích hoạt
    public string? WarrantyLong { get; set; }         // MapLongitude — kinh độ lúc kích hoạt

    // chống giả: đếm số lần quét + mốc
    public int ScanCount { get; set; }
    public DateTime? FirstScanAt { get; set; }
    public DateTime? LastScanAt { get; set; }

    // xuất kho theo tem (Inv_VerifiedIDInOut) — tem đã xuất bán cho khách nào
    public int? ShipmentId { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? CustomerCode { get; set; }

    // kích hoạt bán hàng (Inv_InvVerifiedID_ActivateSales) — tem đã bán cho đại lý/khách
    public bool FlagSales { get; set; }              // FlagSales = '1' khi đã kích hoạt bán hàng
    public DateTime? SalesDTime { get; set; }        // SalesDTime — mốc kích hoạt bán hàng
    public int? SalesActivationId { get; set; }      // phiếu kích hoạt bán hàng đã ghi nhận tem này

    // quay thưởng
    public bool HasSpun { get; set; }
    public string? PrizeWon { get; set; }

    public StampBatch Batch { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Box? Box { get; set; }
    public Shipment? Shipment { get; set; }
    public SalesActivation? SalesActivation { get; set; }
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

// ── Kích hoạt bán hàng (Inv_InvVerifiedID_ActivateSales) ─────────────
// Nghiệp vụ EQR (worker LIVE WAS_Inv_InvVerifiedID_ActivateSales_New20210614,
// file zTemp.cs): bước (6b) vòng đời tem — đại lý/NPP xác nhận đã BÁN tem.
// Khác với "xuất kho theo tem" (Inv_VerifiedIDInOut): phiếu này KHÔNG gắn
// vận chuyển/đơn hàng nguồn, mà tự sinh 1 phiếu xuất nội bộ (RefType=INVOUT,
// tiền tố PXKHT) trỏ về 1 khách hàng bán mặc định, rồi đánh dấu từng tem
// FlagSales='1' + SalesDTime + CustomerCode.
// Ràng buộc EQR: phải có ít nhất 1 tem; mọi tem phải tồn tại; tem đã vô hiệu
// (Void)/rách-vỡ (Broken) bị từ chối; tem đã kích hoạt bán hàng trước đó bị
// từ chối (không kích hoạt trùng).
public class SalesActivation : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string SaNo { get; set; } = "";          // mã phiếu kích hoạt bán hàng (duy nhất trong tenant)
    public string RefNoSys { get; set; } = "";      // RefNoSys — mã phiếu xuất nội bộ tự sinh (PXKHT...)
    public string RefType { get; set; } = "INVOUT";  // RefType — luôn INVOUT (đúng EQR)
    public int ProductId { get; set; }
    public string? CustomerCode { get; set; }        // khách hàng bán (mặc định của hệ thống)
    public string? CustomerName { get; set; }
    public DateTime SalesDTime { get; set; } = DateTime.Now;  // mốc kích hoạt bán hàng
    public int Quantity { get; set; }                // số tem đã kích hoạt bán
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product Product { get; set; } = null!;
    public List<SalesActivationLine> Lines { get; set; } = [];
}

// ── Dòng chi tiết phiếu kích hoạt bán hàng (1 tem đã bán) ────────────
public class SalesActivationLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SalesActivationId { get; set; }
    public string QrId { get; set; } = "";          // mã tem đã kích hoạt bán
    public DateTime SalesDTime { get; set; } = DateTime.Now;

    public SalesActivation SalesActivation { get; set; } = null!;
}

// ── Tem trung tính / tem nghi vấn (Inv_InventoryNeutralID) — nghiệp vụ EQR ──
// Nghiệp vụ EQR (worker LIVE WAS_Inv_InventoryNeutralID_InsertSuspectID,
// file zTemp.cs): phát hiện các tem bị XUẤT KHO NHIỀU LẦN (nghi vấn trùng/
// thất lạc) rồi đánh dấu vào bảng trung tính để rà soát. Ràng buộc EQR:
//  - Chỉ xét các lần xuất trong khoảng thời gian (CreateDTimeUTC <= mốc).
//  - Bỏ qua phiếu cho phép sửa (FlagAllowModify='1') và tem đã hủy phiếu xuất.
//  - Tem xuất > 1 lần ở các phiếu khác nhau ⇒ nghi vấn (FlagNeutral='0').
//  - Tem đã có trong bảng trung tính thì không thêm lại (chống trùng).
// FlagNeutral: '0' = nghi vấn (suspect), '1' = đã xác nhận trung tính.
public class NeutralStamp : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string QrId { get; set; } = "";        // IDNo — mã tem nghi vấn
    public bool FlagNeutral { get; set; }           // FlagNeutral = '1'/'0' (false = nghi vấn)
    public int OutCount { get; set; }               // số lần tem đã bị xuất kho (phát hiện)
    public string? Note { get; set; }               // ghi chú xử lý
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; set; }       // mốc xác nhận trung tính
    public string? ResolvedBy { get; set; }
}

// ── Yêu cầu xuất kho (InvF_ReqInvOut) — nghiệp vụ EQR ────────────────
// Nghiệp vụ EQR (worker LIVE WAS_InvF_ReqInvOut_Save / _Approve,
// file InventoryForm.cs): phiếu YÊU CẦU xuất kho do người dùng lập (theo
// mặt hàng + số lượng), chờ duyệt rồi mới gắn với 1 phiếu xuất kho theo tem
// (Inv_VerifiedIDInOut). Vòng đời: PENDING → APPROVE (có thể bỏ duyệt về PENDING).
// Ràng buộc EQR:
//  - ReqInvOutNo bắt buộc (mã hệ thống sinh).
//  - RefNo (số yêu cầu) bắt buộc + duy nhất trong tenant.
//  - Phải có ít nhất 1 dòng mặt hàng; mọi mặt hàng phải tồn tại.
//  - Chỉ phiếu PENDING mới sửa/duyệt được; chỉ phiếu APPROVE mới bỏ duyệt được.
//  - Khi duyệt: IVerifiedIDInOutNo (phiếu xuất theo tem) không được trùng phiếu khác.
public class ReqInvOut : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReqInvOutNo { get; set; } = "";   // mã yêu cầu xuất kho (duy nhất trong tenant)
    public string RefNo { get; set; } = "";          // số yêu cầu (người dùng nhập, duy nhất trong tenant)
    public string? InvCode { get; set; }              // mã kho
    public string? InvOutType { get; set; }           // mã loại xuất kho
    public DateTime InvOutDate { get; set; } = DateTime.Today;  // ngày xuất
    public string? TransportType { get; set; }        // loại phương tiện
    public string? VehicleNumber { get; set; }        // biển số xe
    public string ReqStatus { get; set; } = "PENDING"; // PENDING / APPROVE
    public string? CustomerCodeSys { get; set; }      // mã khách hàng (mã hệ thống)
    public string? ReceiveAddress { get; set; }       // địa điểm nhận hàng
    public string? IVerifiedIDInOutNo { get; set; }   // phiếu xuất kho theo tem gắn khi duyệt
    public string? QRCodeOS { get; set; }
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }

    public List<ReqInvOutDtl> Lines { get; set; } = [];
}

// ── Dòng chi tiết yêu cầu xuất kho (1 mặt hàng + số lượng) ───────────
public class ReqInvOutDtl : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ReqInvOutId { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";    // mã mặt hàng (Product.Code)
    public string? UnitCode { get; set; }              // đơn vị tính
    public int Qty { get; set; }                       // số lượng
    public int QtyId { get; set; }                     // số bao
    public bool FlagDiscount { get; set; }             // cờ khuyến mãi
    public string? Remark { get; set; }
    public string? QRCodeOS { get; set; }

    public ReqInvOut ReqInvOut { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// ── Loại Block (Mst_BlockType) — master data cho nghiệp vụ Map_Block ──
// Nghiệp vụ EQR (bảng Mst_BlockType, worker Mst_BlockType_CheckDB, file
// Master.cs): danh mục loại Block quy định BlockSize (số tem/hộp tối đa
// trong 1 block). BlockType là khóa (Ma stamp, Ma Box, Ma Can, Ma Pack,
// Ma Pallet, Ma Container...). FlagActive = '1'/'0'.
public class BlockType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // BlockType — mã loại block (duy nhất trong tenant)
    public string Name { get; set; } = "";        // BlockTypeName — tên loại block
    public int BlockSize { get; set; }             // BlockSize — số đơn vị tối đa trong 1 block
    public bool IsActive { get; set; } = true;     // FlagActive = '1'/'0'
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── Block (Map_Block) — nghiệp vụ EQR ────────────────────────────────
// Nghiệp vụ EQR (worker LIVE WAS_Map_Block_Add_New20220701 →
// Map_Block_AddX_New20230213, file InvGen.cs): gom các tem/hộp cùng
// (ShiftCode, LotCode, ProductCode, OrgID, InvCode, BlockType,
// BlockLocalID) thành 1 Block logic (thường = 1 pallet/lô đóng gói).
// Mỗi Block có BoxNo (mã block), Qty (BlockSize của loại block) và
// QtyVerified (số tem thực tế đã gom). Ràng buộc EQR:
//  - BlockType phải tồn tại trong Mst_BlockType (Mst_BlockType_CheckDB).
//  - BlockType rỗng/BAOLE ⇒ không tạo block (chỉ map tem lẻ).
//  - Mọi tem đưa vào phải tồn tại trong hệ thống.
public class Block : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string BlockNo { get; set; } = "";      // BoxNo — mã block (duy nhất trong tenant)
    public int ProductId { get; set; }
    public string BlockType { get; set; } = "";    // BlockType — loại block (khóa Mst_BlockType)
    public string? BlockLocalID { get; set; }       // BlockLocalID — mã cục bộ của block
    public string? ShiftCode { get; set; }          // ShiftCode — ca sản xuất
    public string? LotCode { get; set; }            // LotCode — mã lô
    public int Qty { get; set; }                    // Qty — BlockSize của loại block
    public int QtyVerified { get; set; }            // QtyVerified — số tem thực tế đã gom
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product Product { get; set; } = null!;
    public List<BlockLine> Lines { get; set; } = [];
}

// ── Dòng chi tiết Block (1 tem đã gom vào block) ─────────────────────
public class BlockLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int BlockId { get; set; }
    public string QrId { get; set; } = "";         // mã tem đã gom vào block
    public DateTime AddedAt { get; set; } = DateTime.Now;

    public Block Block { get; set; } = null!;
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

// ── Phiếu xuất kho thành phẩm (InvF_InventoryOutFG) ──────────────────
// Nghiệp vụ EQR (WAS_InvF_InventoryOutFG_Save / _Approve, file InventoryForm.cs):
// ghi nhận 1 phiếu xuất kho thành phẩm theo MST (người nộp thuế) — KHÁC với
// "xuất kho theo tem" (Inv_VerifiedIDInOut): phiếu này xuất theo mặt hàng (PartCode)
// + số lượng, không gắn từng con tem.
// Ràng buộc: IF_InvOutFGNo bắt buộc + duy nhất; InvFOutType phải là
// OUTTHUONGMAI hoặc OUTENDCUS; ít nhất 1 dòng mặt hàng; phiếu ở PENDING mới sửa/duyệt được.
public class InventoryOutFG : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string InvOutFGNo { get; set; } = "";   // IF_InvOutFGNo — mã phiếu xuất (duy nhất trong tenant)
    public string? Mst { get; set; }                 // MST — mã người nộp thuế (đơn vị xuất)
    public string FormOutType { get; set; } = "KHONGMAVACH"; // MAVACH / KHONGMAVACH
    public string? InvOutType { get; set; }          // loại xuất kho
    public string? InvCode { get; set; }             // mã kho
    public string? PmType { get; set; }
    public string InvFOutType { get; set; } = "OUTTHUONGMAI"; // OUTTHUONGMAI / OUTENDCUS

    // vận chuyển
    public string? PlateNo { get; set; }             // biển số xe
    public string? MoocNo { get; set; }              // số mooc
    public string? DriverName { get; set; }
    public string? DriverPhoneNo { get; set; }

    // đối tượng nhận
    public string? AgentCode { get; set; }           // mã đại lý
    public string? CustomerName { get; set; }

    public string Status { get; set; } = "PENDING";  // PENDING / APPROVE / CANCEL
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }

    public List<InventoryOutFGDtl> Lines { get; set; } = [];
}

// ── Dòng chi tiết phiếu xuất kho thành phẩm (1 mặt hàng + số lượng) ───
// vd: PartCode = SP001, Qty = 500. SerialNo (InstSerial) lưu kèm danh sách
// serial xuất (nếu có) dưới dạng text phân tách dòng.
public class InventoryOutFGDtl : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int InventoryOutFGId { get; set; }
    public string PartCode { get; set; } = "";        // mã mặt hàng (Product.Code)
    public int ProductId { get; set; }
    public int Qty { get; set; }                     // số lượng xuất
    public string? SerialNo { get; set; }            // danh sách serial (mỗi dòng 1 serial)

    public InventoryOutFG InventoryOutFG { get; set; } = null!;
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

// ── Danh mục địa điểm GS1 (Mst_GLN) — master data truy xuất nguồn gốc ─
// Nghiệp vụ EQR (WAS_Mst_GLN_Create_New20210408 / _Update_New20210408 /
// _Delete_New20210409 / _Get, file eTEMTruyXuat/eTemNN.cs): danh mục địa điểm
// chuẩn GS1 (Global Location Number) — nhà máy, kho, trang trại, điểm bán…
// dùng cho trường GLNOrgCode ở sự kiện truy xuất và GlnCode ở nguồn gốc.
// Ràng buộc EQR:
//  - GLNCode bắt buộc + duy nhất (Mst_GLN_CheckDB_MstGLNExist khi tạo).
//  - GLNName bắt buộc.
//  - OrgID bắt buộc và phải tồn tại (Mst_Org_CheckDB).
//  - Khi sửa: bản ghi phải tồn tại; nếu đổi tên thì tên không được rỗng.
//  - Khi xóa: bản ghi phải tồn tại.
public class Gs1Location : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // GLNCode — mã địa điểm GS1 (duy nhất trong tenant)
    public string Name { get; set; } = "";        // GLNName — tên địa điểm
    public string? GpsLat { get; set; }            // GPSLat — vĩ độ
    public string? GpsLong { get; set; }           // GPSLong — kinh độ
    public string? OrgCode { get; set; }           // OrgID — đơn vị/tổ chức sở hữu địa điểm
    public string? Remark { get; set; }
    public bool IsActive { get; set; } = true;     // FlagActive
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

// ── Hóa đơn điện tử (Invoice_Invoice) — nghiệp vụ bước (8) vòng đời tem ─
// EQR: hóa đơn điện tử kế thừa hệ TVAN (Invoice_Invoice + Dtl), cấp số từ
// "mẫu hóa đơn" (Invoice_TempInvoice) có dải số StartInvoiceNo..EndInvoiceNo.
// Vòng đời: PENDING → APPROVED (duyệt) → ISSUED (cấp số & phát hành) → CANCEL (hủy).
// Ràng buộc EQR (Invoice_Invoice_SaveX / _Approved / _Cancel):
//  - InvoiceCode bắt buộc + duy nhất.
//  - Chỉ phiếu PENDING mới sửa/duyệt được; đã cấp số (InvoiceNo) thì không xóa.
//  - Ngày hóa đơn không được ở tương lai.
//  - Tổng thanh toán = tiền hàng + VAT.
//  - Cấp số: lấy số kế tiếp trong dải của mẫu; hết dải ⇒ từ chối (InvalidQtyIssueRemain).
public class Invoice : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string InvoiceCode { get; set; } = "";   // mã hóa đơn nội bộ (duy nhất trong tenant)
    public string? InvoiceNo { get; set; }            // số hóa đơn được cấp (chỉ sau khi phát hành)
    public string Status { get; set; } = "PENDING";  // PENDING / APPROVED / ISSUED / CANCEL

    // mẫu hóa đơn + dải số (Invoice_TempInvoice)
    public string TInvoiceCode { get; set; } = "";    // mã mẫu hóa đơn
    public long InvoiceNoStart { get; set; }          // StartInvoiceNo của mẫu
    public long InvoiceNoEnd { get; set; }            // EndInvoiceNo của mẫu

    // đơn hàng nguồn
    public string? RefNo { get; set; }                // số tham chiếu
    public string? Mst { get; set; }                  // MST người nộp thuế (đơn vị phát hành)
    public string? PaymentMethodCode { get; set; }    // hình thức thanh toán

    // bên mua (NNT = người nộp thuế / khách hàng)
    public string? CustomerNntCode { get; set; }
    public string? CustomerNntName { get; set; }
    public string? CustomerNntAddress { get; set; }
    public string? CustomerNntPhone { get; set; }
    public string? CustomerNntEmail { get; set; }
    public string? CustomerMst { get; set; }          // MST bên mua

    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public decimal TotalValInvoice { get; set; }      // tiền hàng (chưa VAT)
    public decimal TotalValVat { get; set; }          // tiền thuế VAT
    public decimal TotalValPmt { get; set; }          // tổng thanh toán = tiền hàng + VAT

    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }

    public List<InvoiceDtl> Lines { get; set; } = [];
}

// ── Dòng chi tiết hóa đơn (Invoice_InvoiceDtl) — 1 mặt hàng ───────────
public class InvoiceDtl : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public string PartCode { get; set; } = "";       // mã mặt hàng (Product.Code)
    public string? UnitName { get; set; }              // đơn vị tính
    public int Qty { get; set; }                       // số lượng
    public decimal UnitPrice { get; set; }             // đơn giá
    public decimal Amount { get; set; }                // thành tiền = Qty * UnitPrice

    public Invoice Invoice { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// ── Kích hoạt bảo hành bằng PIN (WarrantyDateStartFromPIN_Activate) ──
// Nghiệp vụ EQR (worker LIVE WAS_WarrantyDateStartFromPIN_Activate_New20250520,
// file Report.cs): người tiêu dùng quét QR + nhập mã PIN cào trên tem để kích
// hoạt bảo hành. Mỗi lần kích hoạt ghi 1 bản ghi lịch sử (tương ứng
// Inv_InventoryVerifiedIDHist của EQR). Ràng buộc EQR:
//  - Tem phải tồn tại; PIN phải khớp (so khớp PIN hoặc MD5(IDNo|PIN)).
//  - Lần kích hoạt ĐẦU TIÊN: cấp WarrantyNo mới + ghi WarrantyDateStart.
//  - Kích hoạt LẠI: giữ nguyên WarrantyNo/WarrantyDateStart, chỉ tăng WarrantyCount
//    và cập nhật SĐT/IP/vị trí.
//  - Tem đã vô hiệu (Void)/rách-vỡ (Broken) → từ chối.
public class WarrantyActivation : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string QrId { get; set; } = "";          // IDNo — mã tem được kích hoạt
    public string Pin { get; set; } = "";            // PIN đã nhập (đã khớp)
    public string? PhoneNoUser { get; set; }          // SĐT người kích hoạt
    public string? IpAddress { get; set; }            // IP người kích hoạt
    public string? MapLatitude { get; set; }          // vĩ độ
    public string? MapLongitude { get; set; }         // kinh độ
    public string WarrantyNo { get; set; } = "";     // số phiếu bảo hành
    public DateTime WarrantyDateStart { get; set; }   // ngày bắt đầu bảo hành
    public bool IsFirstActivate { get; set; }         // true = kích hoạt lần đầu
    public int WarrantyCount { get; set; }            // số lần kích hoạt sau lần này
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── Vòng đời tem theo kỳ tháng (InvIVID_LifeCircleIDNoByPeriod) ──────
// Nghiệp vụ EQR (worker LIVE WAS_InvIVID_LifeCircleIDNoByPeriod_Add,
// file Template.cs): chốt 1 bản ghi "vòng đời tem" cho 1 KỲ THÁNG, gồm
// 4 chỉ số đếm tem trong tháng + chênh lệch so với kỳ trước:
//  - QtyVerifiedID: tem đã ghép sản phẩm (FlagMap='1', MapIDDTimeUTC trong kỳ).
//  - QtySales:      tem đã kích hoạt bán hàng (FlagSales='1', InvOutDTime trong kỳ).
//  - QtyWarranty:   tem đã kích hoạt bảo hành (FlagPIN='1', WarrantyDateStart trong kỳ).
//  - QtySearch:     tem đã được tra cứu (SearchCount != 0, SearchLastDTimeUTC trong kỳ).
// Delta* = chỉ số kỳ này − chỉ số kỳ trước (kỳ trước = PeriodMonth lớn nhất < kỳ này).
// Ràng buộc EQR: PeriodMonth chuẩn hoá về ngày đầu tháng (yyyy-MM-01);
// mỗi kỳ chỉ chốt 1 lần (chống trùng theo OrgId + PeriodMonth).
public class StampLifecyclePeriod : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public DateTime PeriodMonth { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1); // kỳ tháng (ngày đầu tháng)
    public int QtyVerifiedID { get; set; }   // tem đã ghép sản phẩm trong kỳ
    public int QtySales { get; set; }        // tem đã kích hoạt bán hàng trong kỳ
    public int QtyWarranty { get; set; }     // tem đã kích hoạt bảo hành trong kỳ
    public int QtySearch { get; set; }       // tem đã được tra cứu trong kỳ
    public int DeltaVerifiedID { get; set; } // chênh lệch so với kỳ trước
    public int DeltaSales { get; set; }
    public int DeltaWarranty { get; set; }
    public int DeltaSearch { get; set; }
    public string? Remark { get; set; }
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

// ── Truy xuất nguồn gốc theo chuẩn GS1 (module eTEM TruyXuat) ────────
// Mst_CTE (Critical Tracking Event) — loại sự kiện truy xuất: Trồng,
// Thu hoạch, Đóng gói, Vận chuyển… Mỗi CTE gồm nhiều KDE (qua CTE_KDE).
// Ràng buộc EQR: CTECode bắt buộc + duy nhất; phải gắn 1 template hiển thị
// (TplVECode) để người tiêu dùng xem được sự kiện.
public class TraceEventType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // CTECode — vd HARVEST
    public string Name { get; set; } = "";        // CTEName — vd "Thu hoạch"
    public string? TplVECode { get; set; }         // template hiển thị (Mst_TplViewEvent)
    public string? TplVEDetail { get; set; }       // mô tả template
    public bool IsActive { get; set; } = true;     // FlagActive
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<TraceEventTypeKde> Kdes { get; set; } = [];
}

// ── Mst_KDE (Key Data Element) — trường dữ liệu của 1 sự kiện ────────
// vd: nhiệt độ, số lô, người thực hiện, khối lượng…
// FlagKey = '1' ⇒ KDE này là "khóa" định danh sự kiện (dùng để gộp/khớp
// sự kiện trùng). FlagList = '1' ⇒ KDE dạng danh sách (mỗi sự kiện chỉ
// được có tối đa 1 KDE list — ràng buộc AllowOnlyOneListPerEvent của EQR).
public class TraceKde : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // KDECode — vd TEMPERATURE
    public string Name { get; set; } = "";        // KDEName — vd "Nhiệt độ"
    public string? DataType { get; set; }          // kiểu dữ liệu — vd TEXT / NUMBER / DATE
    public bool IsKey { get; set; }                // FlagKey — KDE khóa định danh sự kiện
    public bool IsList { get; set; }               // FlagList — KDE dạng danh sách
    public bool IsActive { get; set; } = true;     // FlagActive
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── CTE_KDE — gán KDE nào thuộc CTE nào (bảng nối) ───────────────────
// Ràng buộc EQR: mọi cặp (CTECode, KDECode) khi lưu sự kiện phải tồn tại
// trong bảng nối này (CTECode_KDECodeNotFound).
public class TraceEventTypeKde : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TraceEventTypeId { get; set; }
    public int TraceKdeId { get; set; }
    public bool IsKey { get; set; }                // FlagKey (bản sao theo CTE)
    public bool IsList { get; set; }               // FlagList (bản sao theo CTE)

    public TraceEventType TraceEventType { get; set; } = null!;
    public TraceKde TraceKde { get; set; } = null!;
}

// ── Event_Event — sự kiện truy xuất THỰC TẾ gắn với tem/lô ───────────
// Nghiệp vụ EQR (WAS_Event_Event_Save_New20210922): 1 sự kiện = header
// (CTECode, UIStyleCode, GLNOrgCode, Remark) + N dòng KDE (Event_EventSpec).
// Ràng buộc: CTECode + UIStyleCode bắt buộc; mọi KDE phải thuộc CTE;
// tối đa 1 KDE list; các KDE khóa (FlagKey) bắt buộc có giá trị và phải
// đủ số lượng khóa; sự kiện trùng khóa ⇒ cập nhật (UPDATE) thay vì tạo mới.
public class TraceEvent : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string EventNo { get; set; } = "";      // mã sự kiện (duy nhất trong tenant)
    public string CteCode { get; set; } = "";      // loại sự kiện
    public string UIStyleCode { get; set; } = "";  // kiểu hiển thị
    public string? GLNOrgCode { get; set; }        // mã địa điểm GS1
    public string? TplVECode { get; set; }         // template hiển thị (suy ra từ CTE)
    public string? TplVEDetail { get; set; }
    public string Status { get; set; } = "ACTIVE"; // EventStatus
    public string? Remark { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public List<TraceEventSpec> Specs { get; set; } = [];
}

// ── Event_EventSpec — 1 dòng KDE của sự kiện (KDECode + KDEValue) ────
public class TraceEventSpec : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TraceEventId { get; set; }
    public string CteCode { get; set; } = "";
    public string KdeCode { get; set; } = "";
    public string KdeValue { get; set; } = "";

    public TraceEvent TraceEvent { get; set; } = null!;
}
