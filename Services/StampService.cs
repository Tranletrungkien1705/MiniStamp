using Microsoft.EntityFrameworkCore;
using MiniStamp.Data;
using MiniStamp.Models;

namespace MiniStamp.Services;

public record StampDash(int Products, int Batches, int Stamps, int Activated, int Scans,
    List<(string Product, int Count)> ByProduct);

/// <summary>Kết quả tra cứu công khai 1 con tem.</summary>
public record VerifyResult(bool Found, bool Genuine, string Title, string Message,
    Stamp? Stamp, Product? Product, StampBatch? Batch, List<string> Warnings);

/// <summary>Kết quả đóng gói tem vào hộp (nghiệp vụ Map_IDInBox).</summary>
public record PackResult(bool Ok, string Message, int BoxId, string BoxNo, int Packed);

/// <summary>Kết quả đóng gói hộp vào thùng (nghiệp vụ Map_Can).</summary>
public record CartonResult(bool Ok, string Message, int CartonId, string CanNo, int Packed);

/// <summary>Kết quả gán tem trực tiếp vào thùng (nghiệp vụ Inv_InventoryVerifiedID_UpdCan).</summary>
public record AssignCanResult(bool Ok, string Message, int CartonId, string CanNo, int Assigned);

/// <summary>Kết quả khôi phục hộp tem từ lịch sử (nghiệp vụ Map_IDInBox_RestoreBoxNo).</summary>
public record RestoreResult(bool Ok, string Message, int BoxHistoryId, string BoxNo, int Restored, int Flagged);

/// <summary>Kết quả gộp tem vào 1 hộp mới (nghiệp vụ Map_IDInBox_Merge).</summary>
public record MergeBoxResult(bool Ok, string Message, int BoxId, string BoxNo, int Merged);

/// <summary>Kết quả ghi nhận phiếu tem rách/vỡ (nghiệp vụ InvF_BrokenStamp).</summary>
public record BrokenResult(bool Ok, string Message, int BrokenStampId, string BsNo, int Count);

/// <summary>Kết quả tạo/duyệt phiếu nhập kho thành phẩm (nghiệp vụ InvF_InventoryInFG).</summary>
public record InvInResult(bool Ok, string Message, int Id, string InvInNo, int TotalQty);

/// <summary>Kết quả tạo/duyệt phiếu xuất kho thành phẩm (nghiệp vụ InvF_InventoryOutFG).</summary>
public record InvOutResult(bool Ok, string Message, int Id, string InvOutFGNo, int TotalQty);

/// <summary>Kết quả tạo/xuất phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut).</summary>
public record ShipResult(bool Ok, string Message, int Id, string ShipmentNo, int TotalQty);

/// <summary>Kết quả hủy phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut_Cancel).</summary>
public record ShipCancelResult(bool Ok, string Message, int Id, string ShipmentNo, int Released);

/// <summary>Kết quả gộp phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut_Merge).</summary>
public record ShipMergeResult(bool Ok, string Message, int KeptId, string KeptNo, int MergedCount, int MovedStamps);

/// <summary>Kết quả cho phép sửa phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut_UpdFlagAllowModify).</summary>
public record ShipAllowModifyResult(bool Ok, string Message, int Count, List<string> ShipmentNos);

/// <summary>Kết quả kích hoạt thông tin sản xuất (nghiệp vụ InvF_ProductionActive).</summary>
public record PaResult(bool Ok, string Message, int Id, string PaNo, DateTime ExpiryDate);

/// <summary>Kết quả tạo phiên sản xuất (nghiệp vụ InvF_ProductionSession).</summary>
public record PsResult(bool Ok, string Message, int Id, string PsNo, int QtyVerified);

/// <summary>Kết quả thao tác danh mục nguồn gốc (nghiệp vụ Mst_NguonGoc).</summary>
public record OriginResult(bool Ok, string Message, int Id, string Code);

/// <summary>Kết quả thao tác danh mục địa điểm GS1 (nghiệp vụ Mst_GLN).</summary>
public record GlnResult(bool Ok, string Message, int Id, string Code);

/// <summary>Kết quả lưu sự kiện truy xuất GS1 (nghiệp vụ Event_Event_Save).</summary>
public record TraceEventResult(bool Ok, string Message, int Id, string EventNo, string Action);

/// <summary>Kết quả thao tác hóa đơn điện tử (nghiệp vụ Invoice_Invoice).</summary>
public record InvoiceResult(bool Ok, string Message, int Id, string InvoiceCode, string? InvoiceNo);

/// <summary>Kết quả ghép cặp tem (nghiệp vụ Map_StampPair).</summary>
public record PairResult(bool Ok, string Message, int Id, string MainQrId, string SubQrId);

/// <summary>Kết quả kích hoạt bán hàng (nghiệp vụ Inv_InvVerifiedID_ActivateSales).</summary>
public record SalesResult(bool Ok, string Message, int Id, string SaNo, int Count);

/// <summary>Kết quả hoàn tác kích hoạt bán hàng (nghiệp vụ Inv_InventoryVerifiedID_FlagSalesBackStatus).</summary>
public record SalesRevertResult(bool Ok, string Message, int Id, string SaNo, int Released);

/// <summary>Kết quả gom tem vào Block (nghiệp vụ Map_Block).</summary>
public record BlockResult(bool Ok, string Message, int Id, string BlockNo, int QtyVerified);

/// <summary>Kết quả rà soát tem trung tính (nghiệp vụ Inv_InventoryNeutralID).</summary>
public record NeutralResult(bool Ok, string Message, int Detected, int Inserted);

/// <summary>Kết quả thao tác serial bí mật (nghiệp vụ Inv_InventorySecret).</summary>
public record SecretResult(bool Ok, string Message, int Id, int Marked);

/// <summary>Kết quả thao tác yêu cầu xuất kho (nghiệp vụ InvF_ReqInvOut).</summary>
public record ReqInvOutResult(bool Ok, string Message, int Id, string ReqInvOutNo, string Status);

/// <summary>Kết quả chốt vòng đời tem theo kỳ tháng (nghiệp vụ InvIVID_LifeCircleIDNoByPeriod).</summary>
public record LifecycleResult(bool Ok, string Message, int Id, DateTime PeriodMonth,
    int QtyVerifiedID, int QtySales, int QtyWarranty, int QtySearch);

/// <summary>Kết quả kích hoạt bảo hành bằng PIN (nghiệp vụ WarrantyDateStartFromPIN_Activate).</summary>
public record WarrantyResult(bool Ok, string Message, int Id, string QrId, string WarrantyNo,
    DateTime WarrantyDateStart, bool IsFirstActivate, int WarrantyCount);

/// <summary>Kết quả tạo/xuất phiếu xuất kho theo hộp (nghiệp vụ Inv_InventoryVerifiedID_OutByBox).</summary>
public record BoxShipResult(bool Ok, string Message, int Id, string BsNo, int TotalQty);

/// <summary>Kết quả hủy phiếu xuất kho theo hộp (nghiệp vụ Inv_VerifiedIDInOut_Cancel).</summary>
public record BoxShipCancelResult(bool Ok, string Message, int Id, string BsNo, int Released);

/// <summary>Kết quả nhập dãy serial người dùng (nghiệp vụ Inv_StampUser).</summary>
public record StampUserResult(bool Ok, string Message, int Id, string SuiNo, int Imported, int Mapped);

/// <summary>Kết quả kích hoạt tem trắng bởi Trạm bán hàng (nghiệp vụ Inv_InventoryVerifiedID_ActivateByTBH).</summary>
public record TbhResult(bool Ok, string Message, int Id, string TbhNo, string QrId);

/// <summary>Kết quả cập nhật ngày sản xuất cho tem (nghiệp vụ Inv_InventoryVerifiedID_UpdPrdDTime).</summary>
public record PrdDTimeResult(bool Ok, string Message, int Id, string LogNo, int Updated, DateTime ProductionDTime);

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
    // đóng gói tem vào hộp (Map_IDInBox)
    Task<List<Box>> BoxesAsync();
    Task<Box?> GetBoxAsync(int id);
    Task<PackResult> PackBoxAsync(string boxNo, int productId, IEnumerable<string> qrIds, string createdBy);
    // đóng gói hộp vào thùng (Map_Can)
    Task<List<Carton>> CartonsAsync();
    Task<Carton?> GetCartonAsync(int id);
    Task<CartonResult> PackCartonAsync(string canNo, int productId, IEnumerable<string> boxNos, string createdBy);
    // gán tem trực tiếp vào thùng (Inv_InventoryVerifiedID_UpdCan)
    Task<AssignCanResult> AssignStampsToCartonAsync(string canNo, IEnumerable<string> qrIds, string createdBy);
    // khôi phục hộp tem từ lịch sử (Map_IDInBox_RestoreBoxNo)
    Task<List<BoxHistory>> BoxHistoriesAsync(string? boxNo);
    Task<BoxHistory?> GetBoxHistoryAsync(int id);
    Task<RestoreResult> RestoreBoxAsync(int boxHistoryId, string createdBy);
    // gộp tem vào 1 hộp mới (Map_IDInBox_Merge)
    Task<MergeBoxResult> MergeBoxAsync(string boxNo, int productId, IEnumerable<string> qrIds, string createdBy);
    // ghép cặp tem (Map_StampPair)
    Task<List<StampPair>> StampPairsAsync();
    Task<StampPair?> GetStampPairAsync(int id);
    Task<PairResult> PairStampsAsync(string mainQrId, string subQrId, string? remark, string createdBy);
    // phiếu tem rách/vỡ (InvF_BrokenStamp)
    Task<List<BrokenStamp>> BrokenStampsAsync();
    Task<BrokenStamp?> GetBrokenStampAsync(int id);
    Task<BrokenResult> ReportBrokenAsync(string bsNo, int productId, IEnumerable<string> qrIds, string? note, string createdBy);
    // phiếu nhập kho thành phẩm (InvF_InventoryInFG)
    Task<List<InventoryInFG>> InventoryInFGsAsync();
    Task<InventoryInFG?> GetInventoryInFGAsync(int id);
    Task<InvInResult> CreateInventoryInFGAsync(string invInNo, string? remark, IEnumerable<(int ProductId, int Qty, DateTime ProductionDate)> lines, string createdBy);
    Task<InvInResult> ApproveInventoryInFGAsync(int id, string approvedBy);
    // phiếu xuất kho thành phẩm (InvF_InventoryOutFG)
    Task<List<InventoryOutFG>> InventoryOutFGsAsync();
    Task<InventoryOutFG?> GetInventoryOutFGAsync(int id);
    Task<InvOutResult> CreateInventoryOutFGAsync(InventoryOutFG header, IEnumerable<(int ProductId, int Qty, string? SerialNo)> lines, string createdBy);
    Task<InvOutResult> ApproveInventoryOutFGAsync(int id, string approvedBy);
    Task<InvOutResult> DeleteInventoryOutFGAsync(int id);
    // phiếu xuất kho theo tem (Inv_VerifiedIDInOut / OutGenInAndOut)
    Task<List<Shipment>> ShipmentsAsync();
    Task<Shipment?> GetShipmentAsync(int id);
    Task<ShipResult> CreateShipmentAsync(Shipment header, IEnumerable<string> qrIds, string createdBy);
    Task<ShipResult> ShipShipmentAsync(int id, string shippedBy);
    Task<ShipCancelResult> CancelShipmentAsync(int id, string? reason, string cancelledBy);
    Task<ShipMergeResult> MergeShipmentsAsync(string refNoSys, string productCode, string userMoveOrder, string mergedBy);
    Task<ShipAllowModifyResult> AllowModifyShipmentAsync(string refNoSys, string plateNo, int minutes, string by);
    // kích hoạt thông tin sản xuất (InvF_ProductionActive)
    Task<List<ProductLife>> ProductLivesAsync();
    Task<List<ProductionActive>> ProductionActivesAsync();
    Task<ProductionActive?> GetProductionActiveAsync(int id);
    Task<PaResult> CreateProductionActiveAsync(string paNo, string refNo, string origin, int productId,
        int qtyPlan, DateTime productDate, int productLifeId, string listSerialIn, string listSerialOut, string createdBy);
    Task<PaResult> DeleteProductionActiveAsync(int id);
    // phiên sản xuất (InvF_ProductionSession)
    Task<List<ProductionSession>> ProductionSessionsAsync();
    Task<ProductionSession?> GetProductionSessionAsync(int id);
    Task<PsResult> CreateProductionSessionAsync(string psNo, string? orgCode, string? shiftCode, string? lotCode,
        int productId, int qtyInput, IEnumerable<string> qrIds, string? remark, string createdBy);
    // danh mục nguồn gốc (Mst_NguonGoc)
    Task<List<OriginCatalog>> OriginsAsync(string? q);
    Task<OriginCatalog?> GetOriginAsync(int id);
    Task<OriginResult> CreateOriginAsync(OriginCatalog o, string createdBy);
    Task<OriginResult> UpdateOriginAsync(int id, OriginCatalog o);
    Task<OriginResult> DeleteOriginAsync(int id);
    // danh mục địa điểm GS1 (Mst_GLN)
    Task<List<Gs1Location>> Gs1LocationsAsync(string? q);
    Task<Gs1Location?> GetGs1LocationAsync(int id);
    Task<GlnResult> CreateGs1LocationAsync(Gs1Location g, string createdBy);
    Task<GlnResult> UpdateGs1LocationAsync(int id, Gs1Location g);
    Task<GlnResult> DeleteGs1LocationAsync(int id);
    // truy xuất nguồn gốc GS1 (Mst_CTE / Mst_KDE / CTE_KDE / Event_Event)
    Task<List<TraceEventType>> TraceEventTypesAsync();
    Task<TraceEventType?> GetTraceEventTypeAsync(int id);
    Task<List<TraceKde>> TraceKdesAsync();
    Task<List<TraceEvent>> TraceEventsAsync(string? cteCode);
    Task<TraceEvent?> GetTraceEventAsync(int id);
    Task<TraceEventResult> SaveTraceEventAsync(string? eventNo, string cteCode, string uiStyleCode, string? glnOrgCode,
        string? remark, IEnumerable<(string KdeCode, string KdeValue)> specs, string createdBy);
    // hóa đơn điện tử (Invoice_Invoice)
    Task<List<Invoice>> InvoicesAsync();
    Task<Invoice?> GetInvoiceAsync(int id);
    Task<InvoiceResult> CreateInvoiceAsync(Invoice header, IEnumerable<(int ProductId, int Qty, decimal UnitPrice)> lines, string createdBy);
    Task<InvoiceResult> ApproveInvoiceAsync(int id, string approvedBy);
    Task<InvoiceResult> IssueInvoiceAsync(int id, string issuedBy);
    Task<InvoiceResult> CancelInvoiceAsync(int id, string? reason);
    // kích hoạt bán hàng (Inv_InvVerifiedID_ActivateSales)
    Task<List<SalesActivation>> SalesActivationsAsync();
    Task<SalesActivation?> GetSalesActivationAsync(int id);
    Task<SalesResult> ActivateSalesAsync(string saNo, int productId, string? customerCode, string? customerName,
        DateTime salesDTime, IEnumerable<string> qrIds, string? remark, string createdBy);
    Task<SalesRevertResult> RevertSalesActivationAsync(int id, string? reason, string revertedBy);
    // gom tem vào Block (Map_Block)
    Task<List<BlockType>> BlockTypesAsync();
    Task<List<Block>> BlocksAsync();
    Task<Block?> GetBlockAsync(int id);
    Task<BlockResult> CreateBlockAsync(string blockNo, int productId, string blockType, string? blockLocalId,
        string? shiftCode, string? lotCode, IEnumerable<string> qrIds, string? remark, string createdBy);
    // tem trung tính / nghi vấn (Inv_InventoryNeutralID)
    Task<List<NeutralStamp>> NeutralStampsAsync(bool? flagNeutral);
    Task<NeutralStamp?> GetNeutralStampAsync(int id);
    Task<NeutralResult> DetectSuspectStampsAsync(string createdBy);
    Task<NeutralResult> ResolveNeutralStampAsync(int id, string? note, string resolvedBy);
    // tem bí mật / serial ẩn (Inv_InventorySecret)
    Task<List<InventorySecret>> InventorySecretsAsync(string? q, bool? flagUsed);
    Task<InventorySecret?> GetInventorySecretAsync(int id);
    Task<SecretResult> CreateInventorySecretAsync(string serialNo, string? qrSerialNo, string? mst, string? genTimesNo, string? secretNo, string? remark, string createdBy);
    Task<SecretResult> MarkSecretsUsedAsync(IEnumerable<string> serialNos, string? mst, string usedBy);
    // yêu cầu xuất kho (InvF_ReqInvOut)
    Task<List<ReqInvOut>> ReqInvOutsAsync();
    Task<ReqInvOut?> GetReqInvOutAsync(int id);
    Task<ReqInvOutResult> CreateReqInvOutAsync(ReqInvOut header, IEnumerable<(int ProductId, int Qty, string? UnitCode, bool FlagDiscount, string? Remark)> lines, string createdBy);
    Task<ReqInvOutResult> ApproveReqInvOutAsync(int id, string? iVerifiedIDInOutNo, string approvedBy);
    Task<ReqInvOutResult> UnApproveReqInvOutAsync(int id, string unApprovedBy);
    Task<ReqInvOutResult> DeleteReqInvOutAsync(int id);
    // consumer (công khai, xuyên tenant theo QrId)
    Task<VerifyResult> VerifyAsync(string qrId, string? ip);
    Task<(bool ok, string msg)> ActivateAsync(string qrId, string phone);
    Task<(bool ok, string prize)> SpinAsync(string qrId);
    // kích hoạt bảo hành bằng PIN (WarrantyDateStartFromPIN_Activate)
    Task<WarrantyResult> ActivateWarrantyByPinAsync(string qrId, string pin, string? phone, string? ip,
        string? mapLatitude, string? mapLongitude);
    Task<List<WarrantyActivation>> WarrantyActivationsAsync(string? qrId);
    Task<WarrantyActivation?> GetWarrantyActivationAsync(int id);
    // vòng đời tem theo kỳ tháng (InvIVID_LifeCircleIDNoByPeriod)
    Task<List<StampLifecyclePeriod>> LifecyclePeriodsAsync();
    Task<StampLifecyclePeriod?> GetLifecyclePeriodAsync(int id);
    Task<LifecycleResult> SnapshotLifecycleAsync(DateTime periodMonth, string? remark, string createdBy);
    // xuất kho theo hộp (Inv_InventoryVerifiedID_OutByBox)
    Task<List<BoxShipment>> BoxShipmentsAsync();
    Task<BoxShipment?> GetBoxShipmentAsync(int id);
    Task<BoxShipResult> CreateBoxShipmentAsync(BoxShipment header, IEnumerable<(string ScanCode, string StampType)> scans, string createdBy);
    Task<BoxShipResult> ShipBoxShipmentAsync(int id, string shippedBy);
    Task<BoxShipCancelResult> CancelBoxShipmentAsync(int id, string? reason, string cancelledBy);
    // nhập dãy serial người dùng (Inv_StampUser)
    Task<List<StampUser>> StampUsersAsync();
    Task<StampUser?> GetStampUserAsync(int id);
    Task<StampUserResult> ImportStampUsersAsync(string suiNo, IEnumerable<(string IdNoUser, string? Remark)> serials, string createdBy);
    // kích hoạt tem trắng bởi Trạm bán hàng (Inv_InventoryVerifiedID_ActivateByTBH)
    Task<List<TbhActivation>> TbhActivationsAsync();
    Task<TbhActivation?> GetTbhActivationAsync(int id);
    Task<TbhResult> ActivateByTbhAsync(string qrId, string? pin, int productId, string? customerCode, string? areaCode,
        string? proofImagePath, string? proofImagePathName, string? remark, string createdBy);
    // cập nhật ngày sản xuất cho tem (Inv_InventoryVerifiedID_UpdPrdDTime)
    Task<List<StampProductionDateLog>> ProductionDateLogsAsync();
    Task<StampProductionDateLog?> GetProductionDateLogAsync(int id);
    Task<PrdDTimeResult> UpdateProductionDateAsync(DateTime? productionDTime, IEnumerable<string> qrIds, string? remark, string createdBy);
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

    // ── ĐÓNG GÓI TEM VÀO HỘP (Map_IDInBox) ───────────────────────────
    public Task<List<Box>> BoxesAsync() =>
        db.Boxes.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Box?> GetBoxAsync(int id) =>
        db.Boxes.Include(x => x.Product).Include(x => x.Stamps).ThenInclude(s => s.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Đóng gói danh sách tem (theo QrId) vào 1 hộp. Mô phỏng nghiệp vụ Map_IDInBox của EQR:
    /// - Mọi tem phải tồn tại trong hệ thống.
    /// - Tem đã thuộc hộp khác → từ chối (trừ khi cùng hộp đích).
    /// - Nếu toàn bộ tem đã nằm trong đúng hộp này với cùng số lượng → coi như trùng, bỏ qua.
    /// </summary>
    public async Task<PackResult> PackBoxAsync(string boxNo, int productId, IEnumerable<string> qrIds, string createdBy)
    {
        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new PackResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Include(s => s.Box)
            .Where(s => codes.Contains(s.QrId)).ToListAsync();

        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new PackResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        // tem đã thuộc hộp khác
        var inOtherBox = stamps.Where(s => s.BoxId != null && s.Box != null && s.Box.BoxNo != boxNo).ToList();
        if (inOtherBox.Count > 0)
            return new PackResult(false, $"{inOtherBox.Count} tem đã thuộc hộp khác: {string.Join(", ", inOtherBox.Take(10).Select(s => s.QrId))}", 0, "", 0);

        var box = await db.Boxes.FirstOrDefaultAsync(b => b.BoxNo == boxNo);
        if (box == null)
        {
            box = new Box { BoxNo = boxNo, ProductId = productId, CreatedBy = createdBy };
            db.Boxes.Add(box);
            await db.SaveChangesAsync();
        }

        // trùng: tất cả tem đã nằm trong hộp này và số lượng khớp
        var alreadyInBox = stamps.Count(s => s.BoxId == box.Id);
        if (alreadyInBox == stamps.Count && box.Quantity == stamps.Count)
            return new PackResult(true, "Các tem này đã được đóng vào hộp (bỏ qua).", box.Id, box.BoxNo, 0);

        var now = DateTime.Now;
        foreach (var s in stamps) { s.BoxId = box.Id; s.BoxedAt = now; }
        box.Quantity = await db.Stamps.CountAsync(s => s.BoxId == box.Id);

        // Ghi lịch sử đóng hộp (Map_IDInBoxHist) để có thể khôi phục sau này
        var hist = new BoxHistory
        {
            BoxNo = box.BoxNo, FunctionName = "MAP_IDINBOX_ADDX", RefType = "ADD",
            CreateDTimeUTC = now, QtyIDNo = stamps.Count, CreatedBy = createdBy, CreatedAt = now
        };
        foreach (var s in stamps) hist.Lines.Add(new BoxHistoryLine { QrId = s.QrId });
        db.BoxHistories.Add(hist);

        await db.SaveChangesAsync();
        return new PackResult(true, $"Đã đóng {stamps.Count} tem vào hộp {box.BoxNo}.", box.Id, box.BoxNo, stamps.Count);
    }

    // ── KHÔI PHỤC HỘP TEM TỪ LỊCH SỬ (Map_IDInBox_RestoreBoxNo) ─────
    public Task<List<BoxHistory>> BoxHistoriesAsync(string? boxNo)
    {
        var q = db.BoxHistories.Include(x => x.Lines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(boxNo))
        {
            var k = boxNo.Trim();
            q = q.Where(x => x.BoxNo.Contains(k));
        }
        return q.OrderByDescending(x => x.CreateDTimeUTC).Take(300).ToListAsync();
    }

    public Task<BoxHistory?> GetBoxHistoryAsync(int id) =>
        db.BoxHistories.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Khôi phục 1 lần đóng hộp từ lịch sử. Mô phỏng nghiệp vụ
    /// WAS_Map_IDInBox_RestoreBoxNo của EQR (zTemp.1.cs):
    /// - Bản ghi lịch sử phải tồn tại và chưa được khôi phục.
    /// - Hộp đích phải tồn tại.
    /// - Tem đã xuất bán (FlagSales) ⇒ đưa vào bảng tem trung tính (nghi vấn), KHÔNG gắn lại hộp.
    /// - Tem đang trùng ở hộp khác ⇒ đưa vào bảng tem trung tính (nghi vấn), KHÔNG gắn lại hộp.
    /// - Còn lại ⇒ gắn lại vào hộp (BoxId + BoxedAt).
    /// </summary>
    public async Task<RestoreResult> RestoreBoxAsync(int boxHistoryId, string createdBy)
    {
        var hist = await db.BoxHistories.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == boxHistoryId);
        if (hist == null) return new RestoreResult(false, "Không tìm thấy bản ghi lịch sử đóng hộp.", 0, "", 0, 0);
        if (hist.RestoredAt != null)
            return new RestoreResult(false, $"Lần đóng hộp {hist.BoxNo} ({hist.CreateDTimeUTC:dd/MM/yyyy HH:mm}) đã được khôi phục trước đó.", hist.Id, hist.BoxNo, 0, 0);

        var box = await db.Boxes.FirstOrDefaultAsync(b => b.BoxNo == hist.BoxNo);
        if (box == null) return new RestoreResult(false, $"Hộp {hist.BoxNo} không tồn tại.", hist.Id, hist.BoxNo, 0, 0);

        var codes = hist.Lines.Select(l => l.QrId).Distinct().ToList();
        if (codes.Count == 0) return new RestoreResult(false, "Lần đóng hộp này không có tem nào.", hist.Id, hist.BoxNo, 0, 0);

        var stamps = await db.Stamps.Include(s => s.Box).Where(s => codes.Contains(s.QrId)).ToListAsync();

        // tem đã xuất bán (FlagSales) ⇒ nghi vấn
        var sold = stamps.Where(s => s.FlagSales).Select(s => s.QrId).ToHashSet();
        // tem đang trùng ở hộp khác ⇒ nghi vấn
        var dup = stamps.Where(s => s.BoxId != null && s.Box != null && s.Box.BoxNo != hist.BoxNo)
            .Select(s => s.QrId).ToHashSet();
        var flagged = sold.Union(dup).ToHashSet();

        var now = DateTime.Now;
        // ghi tem nghi vấn vào bảng trung tính (chống trùng)
        var existingNeutral = await db.NeutralStamps.IgnoreQueryFilters()
            .Where(x => flagged.Contains(x.QrId)).Select(x => x.QrId).ToListAsync();
        foreach (var qr in flagged.Where(q => !existingNeutral.Contains(q)))
            db.NeutralStamps.Add(new NeutralStamp
            {
                QrId = qr, FlagNeutral = false, OutCount = 1,
                Note = sold.Contains(qr)
                    ? "Khôi phục hộp: tem đã xuất bán — nghi vấn."
                    : "Khôi phục hộp: tem đang trùng ở hộp khác — nghi vấn.",
                CreatedBy = createdBy, CreatedAt = now
            });

        // gắn lại các tem còn lại vào hộp
        var restored = 0;
        foreach (var s in stamps.Where(s => !flagged.Contains(s.QrId)))
        {
            s.BoxId = box.Id;
            s.BoxedAt = now;
            restored++;
        }
        box.Quantity = await db.Stamps.CountAsync(s => s.BoxId == box.Id);

        hist.RestoredAt = now;
        hist.RestoredBy = createdBy;
        await db.SaveChangesAsync();

        return new RestoreResult(true,
            $"Đã khôi phục {restored} tem vào hộp {box.BoxNo}; {flagged.Count} tem nghi vấn đưa vào bảng trung tính.",
            hist.Id, box.BoxNo, restored, flagged.Count);
    }

    // ── GỘP TEM VÀO 1 HỘP MỚI (Map_IDInBox_Merge) ───────────────────
    /// <summary>
    /// Gộp 1 danh sách tem (IDNo) vào 1 hộp MỚI. Mô phỏng nghiệp vụ
    /// WAS_Map_IDInBox_Merge_New20231222 → Map_IDInBox_MergeX_New20231222
    /// (file InvGen.cs) của EQR: EQR sinh 1 BoxNo mới rồi gỡ các tem khỏi
    /// hộp cũ (delete Map_IDInBox theo IDNo) và gắn tất cả vào hộp mới.
    /// Ràng buộc EQR:
    ///  - Danh sách tem không được rỗng.
    ///  - Mọi IDNo phải tồn tại trong kho sinh số (IDNoNotExistInInvGen).
    ///  - Tem đang thuộc hộp KHÁC hộp đích → gỡ khỏi hộp cũ rồi gộp vào hộp mới
    ///    (đúng tinh thần "Clear Map_IDInBox" của EQR).
    ///  - Ghi 1 bản ghi lịch sử (Map_IDInBoxHist, RefType=MERGE) để truy vết.
    /// </summary>
    public async Task<MergeBoxResult> MergeBoxAsync(string boxNo, int productId, IEnumerable<string> qrIds, string createdBy)
    {
        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new MergeBoxResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Include(s => s.Box)
            .Where(s => codes.Contains(s.QrId)).ToListAsync();

        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new MergeBoxResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(boxNo)) boxNo = $"BOX{DateTime.Now:yyMMddHHmmss}";
        boxNo = boxNo.Trim().ToUpperInvariant();

        var box = await db.Boxes.FirstOrDefaultAsync(b => b.BoxNo == boxNo);
        if (box == null)
        {
            box = new Box { BoxNo = boxNo, ProductId = productId, CreatedBy = createdBy };
            db.Boxes.Add(box);
            await db.SaveChangesAsync();
        }

        // trùng: tất cả tem đã nằm trong đúng hộp này và số lượng khớp
        var alreadyInBox = stamps.Count(s => s.BoxId == box.Id);
        if (alreadyInBox == stamps.Count && box.Quantity == stamps.Count)
            return new MergeBoxResult(true, "Các tem này đã được gộp vào hộp (bỏ qua).", box.Id, box.BoxNo, 0);

        var now = DateTime.Now;
        // gỡ tem khỏi hộp cũ (nếu có) rồi gắn vào hộp mới
        var oldBoxIds = stamps.Where(s => s.BoxId != null && s.BoxId != box.Id)
            .Select(s => s.BoxId!.Value).Distinct().ToList();
        foreach (var s in stamps) { s.BoxId = box.Id; s.BoxedAt = now; }

        // cập nhật lại số lượng các hộp cũ bị gỡ tem
        foreach (var oldId in oldBoxIds)
        {
            var oldBox = await db.Boxes.FirstOrDefaultAsync(b => b.Id == oldId);
            if (oldBox != null) oldBox.Quantity = await db.Stamps.CountAsync(s => s.BoxId == oldId);
        }
        box.Quantity = await db.Stamps.CountAsync(s => s.BoxId == box.Id);

        // Ghi lịch sử gộp hộp (Map_IDInBoxHist, RefType=MERGE) để truy vết
        var hist = new BoxHistory
        {
            BoxNo = box.BoxNo, FunctionName = "MAP_IDINBOX_MERGEX", RefType = "MERGE",
            CreateDTimeUTC = now, QtyIDNo = stamps.Count, CreatedBy = createdBy, CreatedAt = now
        };
        foreach (var s in stamps) hist.Lines.Add(new BoxHistoryLine { QrId = s.QrId });
        db.BoxHistories.Add(hist);

        await db.SaveChangesAsync();
        return new MergeBoxResult(true, $"Đã gộp {stamps.Count} tem vào hộp {box.BoxNo}.", box.Id, box.BoxNo, stamps.Count);
    }

    // ── ĐÓNG GÓI HỘP VÀO THÙNG (Map_Can) ────────────────────────────
    public Task<List<Carton>> CartonsAsync() =>
        db.Cartons.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Carton?> GetCartonAsync(int id) =>
        db.Cartons.Include(x => x.Product).Include(x => x.Boxes).ThenInclude(b => b.Product)
            .Include(x => x.Stamps).ThenInclude(s => s.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Đóng gói danh sách hộp (theo BoxNo) vào 1 thùng. Mô phỏng nghiệp vụ Map_Can của EQR:
    /// - Mọi hộp phải tồn tại trong hệ thống.
    /// - Hộp đã thuộc thùng khác → từ chối (trừ khi cùng thùng đích).
    /// - Nếu toàn bộ hộp đã nằm trong đúng thùng này với cùng số lượng → coi như trùng, bỏ qua.
    /// </summary>
    public async Task<CartonResult> PackCartonAsync(string canNo, int productId, IEnumerable<string> boxNos, string createdBy)
    {
        var codes = (boxNos ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new CartonResult(false, "Chưa nhập mã hộp nào.", 0, "", 0);

        var boxes = await db.Boxes.Include(b => b.Carton)
            .Where(b => codes.Contains(b.BoxNo)).ToListAsync();

        var missing = codes.Except(boxes.Select(b => b.BoxNo)).ToList();
        if (missing.Count > 0)
            return new CartonResult(false, $"Không tìm thấy {missing.Count} mã hộp: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        // hộp đã thuộc thùng khác
        var inOtherCarton = boxes.Where(b => b.CartonId != null && b.Carton != null && b.Carton.CanNo != canNo).ToList();
        if (inOtherCarton.Count > 0)
            return new CartonResult(false, $"{inOtherCarton.Count} hộp đã thuộc thùng khác: {string.Join(", ", inOtherCarton.Take(10).Select(b => b.BoxNo))}", 0, "", 0);

        var carton = await db.Cartons.FirstOrDefaultAsync(c => c.CanNo == canNo);
        if (carton == null)
        {
            carton = new Carton { CanNo = canNo, ProductId = productId, CreatedBy = createdBy };
            db.Cartons.Add(carton);
            await db.SaveChangesAsync();
        }

        // trùng: tất cả hộp đã nằm trong thùng này và số lượng khớp
        var alreadyInCarton = boxes.Count(b => b.CartonId == carton.Id);
        if (alreadyInCarton == boxes.Count && carton.BoxCount == boxes.Count)
            return new CartonResult(true, "Các hộp này đã được đóng vào thùng (bỏ qua).", carton.Id, carton.CanNo, 0);

        var now = DateTime.Now;
        foreach (var b in boxes) { b.CartonId = carton.Id; b.CartonedAt = now; }
        carton.BoxCount = await db.Boxes.CountAsync(b => b.CartonId == carton.Id);
        await db.SaveChangesAsync();
        return new CartonResult(true, $"Đã đóng {boxes.Count} hộp vào thùng {carton.CanNo}.", carton.Id, carton.CanNo, boxes.Count);
    }

    /// <summary>
    /// Gán trực tiếp danh sách tem (theo QrId) vào 1 thùng (Can). Mô phỏng nghiệp vụ
    /// WAS_Inv_InventoryVerifiedID_UpdCan của EQR (zTemp.cs) — khác với đóng thùng theo hộp
    /// (Map_Can): ở đây gán từng con tem lẻ vào thùng, không qua hộp. Ràng buộc EQR:
    ///  - Mọi tem phải tồn tại trong hệ thống (InvalidIDNo).
    ///  - Thùng phải tồn tại (CanNoNotExistInInvGen).
    ///  - Tem chưa được gán thùng trước đó (InvalidFlagCan).
    ///  - Gán xong: tem.CartonId + CartonedAt; cập nhật StampCount của thùng.
    /// </summary>
    public async Task<AssignCanResult> AssignStampsToCartonAsync(string canNo, IEnumerable<string> qrIds, string createdBy)
    {
        canNo = (canNo ?? "").Trim().ToUpperInvariant();
        if (canNo.Length == 0) return new AssignCanResult(false, "Cần nhập mã thùng.", 0, "", 0);

        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new AssignCanResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var carton = await db.Cartons.FirstOrDefaultAsync(c => c.CanNo == canNo);
        if (carton == null) return new AssignCanResult(false, $"Thùng {canNo} không tồn tại.", 0, canNo, 0);

        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new AssignCanResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", carton.Id, canNo, 0);

        // tem đã được gán thùng khác (InvalidFlagCan)
        var already = stamps.Where(s => s.CartonId != null && s.CartonId != carton.Id).ToList();
        if (already.Count > 0)
            return new AssignCanResult(false, $"{already.Count} tem đã được gán thùng khác: {string.Join(", ", already.Take(10).Select(s => s.QrId))}", carton.Id, canNo, 0);

        var now = DateTime.Now;
        foreach (var s in stamps) { s.CartonId = carton.Id; s.CartonedAt = now; }
        carton.StampCount = await db.Stamps.CountAsync(s => s.CartonId == carton.Id);
        await db.SaveChangesAsync();
        return new AssignCanResult(true, $"Đã gán {stamps.Count} tem vào thùng {carton.CanNo}.", carton.Id, carton.CanNo, stamps.Count);
    }

    // ── GHÉP CẶP TEM (Map_StampPair) ────────────────────────────────
    public Task<List<StampPair>> StampPairsAsync() =>
        db.StampPairs.OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<StampPair?> GetStampPairAsync(int id) =>
        db.StampPairs.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Ghép 1 tem chính với 1 tem phụ thành 1 cặp 1:1. Mô phỏng nghiệp vụ
    /// WAS_Map_StampPair_Add_New20220415 của EQR (Template.cs):
    /// - Cả 2 tem phải tồn tại trong hệ thống (IDNoNotExistInInvGen).
    /// - Tem chính không được trùng tem phụ.
    /// - Tem chính chưa thuộc cặp nào (ExistIDNoInOtherBox).
    /// - Tem phụ chưa thuộc cặp nào (ExistIDNoInOtherBox).
    /// </summary>
    public async Task<PairResult> PairStampsAsync(string mainQrId, string subQrId, string? remark, string createdBy)
    {
        mainQrId = (mainQrId ?? "").Trim().ToUpperInvariant();
        subQrId = (subQrId ?? "").Trim().ToUpperInvariant();
        if (mainQrId.Length == 0 || subQrId.Length == 0)
            return new PairResult(false, "Cần nhập cả mã tem chính và tem phụ.", 0, mainQrId, subQrId);
        if (mainQrId == subQrId)
            return new PairResult(false, "Tem chính và tem phụ không được trùng nhau.", 0, mainQrId, subQrId);

        var codes = new[] { mainQrId, subQrId };
        var found = await db.Stamps.Where(s => codes.Contains(s.QrId)).Select(s => s.QrId).ToListAsync();
        var missing = codes.Except(found).ToList();
        if (missing.Count > 0)
            return new PairResult(false, $"Không tìm thấy mã tem: {string.Join(", ", missing)}", 0, mainQrId, subQrId);

        // tem đã thuộc cặp khác (ở vai trò chính hoặc phụ)
        var used = await db.StampPairs.IgnoreQueryFilters()
            .Where(p => codes.Contains(p.MainQrId) || codes.Contains(p.SubQrId))
            .Select(p => p.MainQrId == mainQrId || p.SubQrId == mainQrId ? p.MainQrId : p.SubQrId)
            .ToListAsync();
        if (used.Count > 0)
            return new PairResult(false, $"Tem đã được ghép cặp trước đó: {string.Join(", ", used)}", 0, mainQrId, subQrId);

        var pair = new StampPair { MainQrId = mainQrId, SubQrId = subQrId, Remark = remark, CreatedBy = createdBy };
        db.StampPairs.Add(pair);
        await db.SaveChangesAsync();
        return new PairResult(true, $"Đã ghép cặp tem {mainQrId} ↔ {subQrId}.", pair.Id, mainQrId, subQrId);
    }

    // ── CONSUMER (công khai) ─────────────────────────────────────────
    // ── PHIẾU TEM RÁCH/VỠ (InvF_BrokenStamp) ────────────────────────
    public Task<List<BrokenStamp>> BrokenStampsAsync() =>
        db.BrokenStamps.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<BrokenStamp?> GetBrokenStampAsync(int id) =>
        db.BrokenStamps.Include(x => x.Product).Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Ghi nhận 1 phiếu tem rách/vỡ (NG). Mô phỏng nghiệp vụ InvF_BrokenStamp của EQR:
    /// - Mọi tem phải tồn tại trong hệ thống.
    /// - Tem đã được ghi nhận lỗi ở phiếu khác → từ chối (không ghi trùng).
    /// - Tem đã vô hiệu (Void) → từ chối.
    /// - Ghi nhận xong: đánh dấu tem StampStatus.Broken (loại khỏi vòng đời).
    /// </summary>
    public async Task<BrokenResult> ReportBrokenAsync(string bsNo, int productId, IEnumerable<string> qrIds, string? note, string createdBy)
    {
        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new BrokenResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();

        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new BrokenResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        var voided = stamps.Where(s => s.Status == StampStatus.Void).ToList();
        if (voided.Count > 0)
            return new BrokenResult(false, $"{voided.Count} tem đã vô hiệu, không thể ghi nhận lỗi: {string.Join(", ", voided.Take(10).Select(s => s.QrId))}", 0, "", 0);

        // tem đã được ghi nhận lỗi ở phiếu khác
        var alreadyBroken = await db.BrokenStampLines.IgnoreQueryFilters()
            .Where(l => codes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        if (alreadyBroken.Count > 0)
            return new BrokenResult(false, $"{alreadyBroken.Count} tem đã được ghi nhận lỗi trước đó: {string.Join(", ", alreadyBroken.Take(10))}", 0, "", 0);

        var bs = await db.BrokenStamps.FirstOrDefaultAsync(x => x.BsNo == bsNo);
        if (bs == null)
        {
            bs = new BrokenStamp { BsNo = bsNo, ProductId = productId, Note = note, CreatedBy = createdBy };
            db.BrokenStamps.Add(bs);
            await db.SaveChangesAsync();
        }

        var now = DateTime.Now;
        foreach (var s in stamps)
        {
            bs.Lines.Add(new BrokenStampLine { QrId = s.QrId, BrokenAt = now });
            s.Status = StampStatus.Broken;
        }
        bs.Quantity = await db.BrokenStampLines.CountAsync(l => l.BrokenStampId == bs.Id) + stamps.Count;
        await db.SaveChangesAsync();
        return new BrokenResult(true, $"Đã ghi nhận {stamps.Count} tem rách/vỡ vào phiếu {bs.BsNo}.", bs.Id, bs.BsNo, stamps.Count);
    }

    // ── PHIẾU NHẬP KHO THÀNH PHẨM (InvF_InventoryInFG) ──────────────
    public Task<List<InventoryInFG>> InventoryInFGsAsync() =>
        db.InventoryInFGs.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<InventoryInFG?> GetInventoryInFGAsync(int id) =>
        db.InventoryInFGs.Include(x => x.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo phiếu nhập kho thành phẩm (FG). Mô phỏng nghiệp vụ InvF_InventoryInFG_Save của EQR:
    /// - Mỗi dòng phải có sản phẩm hợp lệ và số lượng > 0.
    /// - Phiếu mới ở trạng thái PENDING (chờ duyệt).
    /// </summary>
    public async Task<InvInResult> CreateInventoryInFGAsync(string invInNo, string? remark,
        IEnumerable<(int ProductId, int Qty, DateTime ProductionDate)> lines, string createdBy)
    {
        var rows = (lines ?? []).Where(l => l.ProductId > 0 && l.Qty > 0).ToList();
        if (rows.Count == 0) return new InvInResult(false, "Cần ít nhất 1 dòng có sản phẩm và số lượng > 0.", 0, "", 0);

        var productIds = rows.Select(r => r.ProductId).Distinct().ToList();
        var validIds = await db.Products.Where(p => productIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
        var bad = productIds.Except(validIds).ToList();
        if (bad.Count > 0) return new InvInResult(false, $"Sản phẩm không tồn tại: {string.Join(", ", bad)}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(invInNo)) invInNo = $"PNK{DateTime.Now:yyMMddHHmmss}";
        invInNo = invInNo.Trim();
        if (await db.InventoryInFGs.AnyAsync(x => x.InvInNo == invInNo))
            return new InvInResult(false, $"Mã phiếu {invInNo} đã tồn tại.", 0, "", 0);

        var fg = new InventoryInFG { InvInNo = invInNo, Remark = remark, CreatedBy = createdBy, Status = "PENDING" };
        foreach (var r in rows)
            fg.Lines.Add(new InventoryInFGDtl
            {
                ProductId = r.ProductId,
                Qty = r.Qty,
                ProductionDate = r.ProductionDate == default ? DateTime.Today : r.ProductionDate
            });
        db.InventoryInFGs.Add(fg);
        await db.SaveChangesAsync();
        return new InvInResult(true, $"Đã tạo phiếu nhập {fg.InvInNo} ({rows.Count} dòng, {rows.Sum(r => r.Qty)} SP).", fg.Id, fg.InvInNo, rows.Sum(r => r.Qty));
    }

    /// <summary>
    /// Duyệt phiếu nhập kho. Mô phỏng nghiệp vụ InvF_InventoryInFG_Approve của EQR:
    /// - Chỉ phiếu PENDING mới được duyệt.
    /// - Duyệt xong: Status = APPROVE + ghi mốc thời gian/người duyệt.
    /// </summary>
    public async Task<InvInResult> ApproveInventoryInFGAsync(int id, string approvedBy)
    {
        var fg = await db.InventoryInFGs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (fg == null) return new InvInResult(false, "Không tìm thấy phiếu nhập.", 0, "", 0);
        if (fg.Status != "PENDING") return new InvInResult(false, $"Phiếu {fg.InvInNo} đang ở trạng thái {fg.Status}, không thể duyệt.", fg.Id, fg.InvInNo, 0);

        fg.Status = "APPROVE";
        fg.ApprovedAt = DateTime.Now;
        fg.ApprovedBy = approvedBy;
        await db.SaveChangesAsync();
        return new InvInResult(true, $"Đã duyệt phiếu nhập {fg.InvInNo}.", fg.Id, fg.InvInNo, fg.Lines.Sum(l => l.Qty));
    }

    // ── PHIẾU XUẤT KHO THÀNH PHẨM (InvF_InventoryOutFG) ─────────────
    public Task<List<InventoryOutFG>> InventoryOutFGsAsync() =>
        db.InventoryOutFGs.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<InventoryOutFG?> GetInventoryOutFGAsync(int id) =>
        db.InventoryOutFGs.Include(x => x.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo/cập nhật phiếu xuất kho thành phẩm. Mô phỏng nghiệp vụ
    /// InvF_InventoryOutFG_Save của EQR (InventoryForm.cs):
    /// - IF_InvOutFGNo bắt buộc và duy nhất.
    /// - InvFOutType chỉ nhận OUTTHUONGMAI hoặc OUTENDCUS.
    /// - Phải có ít nhất 1 dòng mặt hàng (PartCode) với số lượng > 0.
    /// - Mọi mặt hàng phải tồn tại trong hệ thống.
    /// - Phiếu mới ở trạng thái PENDING (chờ duyệt).
    /// </summary>
    public async Task<InvOutResult> CreateInventoryOutFGAsync(InventoryOutFG header,
        IEnumerable<(int ProductId, int Qty, string? SerialNo)> lines, string createdBy)
    {
        var rows = (lines ?? []).Where(l => l.ProductId > 0 && l.Qty > 0).ToList();
        if (rows.Count == 0) return new InvOutResult(false, "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0.", 0, "", 0);

        var type = (header.InvFOutType ?? "").Trim().ToUpperInvariant();
        if (type != "OUTTHUONGMAI" && type != "OUTENDCUS")
            return new InvOutResult(false, "Loại xuất (InvFOutType) chỉ nhận OUTTHUONGMAI hoặc OUTENDCUS.", 0, "", 0);

        var productIds = rows.Select(r => r.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
        var bad = productIds.Except(products.Select(p => p.Id)).ToList();
        if (bad.Count > 0) return new InvOutResult(false, $"Mặt hàng không tồn tại: {string.Join(", ", bad)}", 0, "", 0);
        var codeById = products.ToDictionary(p => p.Id, p => p.Code);

        if (string.IsNullOrWhiteSpace(header.InvOutFGNo)) header.InvOutFGNo = $"PXKTP{DateTime.Now:yyMMddHHmmss}";
        header.InvOutFGNo = header.InvOutFGNo.Trim();
        if (await db.InventoryOutFGs.AnyAsync(x => x.InvOutFGNo == header.InvOutFGNo))
            return new InvOutResult(false, $"Mã phiếu {header.InvOutFGNo} đã tồn tại.", 0, "", 0);

        header.InvFOutType = type;
        header.Status = "PENDING";
        header.CreatedBy = createdBy;
        foreach (var r in rows)
            header.Lines.Add(new InventoryOutFGDtl
            {
                ProductId = r.ProductId,
                PartCode = codeById[r.ProductId],
                Qty = r.Qty,
                SerialNo = string.IsNullOrWhiteSpace(r.SerialNo) ? null : r.SerialNo.Trim()
            });
        db.InventoryOutFGs.Add(header);
        await db.SaveChangesAsync();
        return new InvOutResult(true, $"Đã tạo phiếu xuất kho TP {header.InvOutFGNo} ({rows.Count} dòng, {rows.Sum(r => r.Qty)} SP).", header.Id, header.InvOutFGNo, rows.Sum(r => r.Qty));
    }

    /// <summary>
    /// Duyệt phiếu xuất kho thành phẩm. Mô phỏng nghiệp vụ
    /// InvF_InventoryOutFG_Approve của EQR:
    /// - Chỉ phiếu PENDING mới được duyệt.
    /// - Duyệt xong: Status = APPROVE + ghi mốc thời gian/người duyệt.
    /// </summary>
    public async Task<InvOutResult> ApproveInventoryOutFGAsync(int id, string approvedBy)
    {
        var fg = await db.InventoryOutFGs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (fg == null) return new InvOutResult(false, "Không tìm thấy phiếu xuất kho.", 0, "", 0);
        if (fg.Status != "PENDING") return new InvOutResult(false, $"Phiếu {fg.InvOutFGNo} đang ở trạng thái {fg.Status}, không thể duyệt.", fg.Id, fg.InvOutFGNo, 0);

        fg.Status = "APPROVE";
        fg.ApprovedAt = DateTime.Now;
        fg.ApprovedBy = approvedBy;
        await db.SaveChangesAsync();
        return new InvOutResult(true, $"Đã duyệt phiếu xuất kho TP {fg.InvOutFGNo}.", fg.Id, fg.InvOutFGNo, fg.Lines.Sum(l => l.Qty));
    }

    /// <summary>
    /// Xóa phiếu xuất kho thành phẩm. Mô phỏng ràng buộc EQR: chỉ xóa được phiếu PENDING.
    /// </summary>
    public async Task<InvOutResult> DeleteInventoryOutFGAsync(int id)
    {
        var fg = await db.InventoryOutFGs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (fg == null) return new InvOutResult(false, "Không tìm thấy phiếu xuất kho.", 0, "", 0);
        if (fg.Status != "PENDING") return new InvOutResult(false, $"Phiếu {fg.InvOutFGNo} đang ở trạng thái {fg.Status}, không thể xóa.", fg.Id, fg.InvOutFGNo, 0);

        db.InventoryOutFGs.Remove(fg);
        await db.SaveChangesAsync();
        return new InvOutResult(true, $"Đã xóa phiếu xuất kho TP {fg.InvOutFGNo}.", 0, fg.InvOutFGNo, 0);
    }

    // ── PHIẾU XUẤT KHO THEO TEM (Inv_VerifiedIDInOut / OutGenInAndOut) ─
    public Task<List<Shipment>> ShipmentsAsync() =>
        db.Shipments.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Shipment?> GetShipmentAsync(int id) =>
        db.Shipments.Include(x => x.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo phiếu xuất kho theo tem. Mô phỏng nghiệp vụ Inv_InvVerifiedID_OutGenInAndOut của EQR:
    /// - Mọi tem phải tồn tại trong hệ thống.
    /// - Tem đã vô hiệu (Void) hoặc rách/vỡ (Broken) → từ chối.
    /// - Tem đã xuất ở phiếu khác → từ chối (không xuất trùng).
    /// - Phiếu mới ở trạng thái PENDING (chờ xuất); dòng tem gắn theo sản phẩm của tem.
    /// </summary>
    public async Task<ShipResult> CreateShipmentAsync(Shipment header, IEnumerable<string> qrIds, string createdBy)
    {
        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new ShipResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();

        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new ShipResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        var bad = stamps.Where(s => s.Status == StampStatus.Void || s.Status == StampStatus.Broken).ToList();
        if (bad.Count > 0)
            return new ShipResult(false, $"{bad.Count} tem đã vô hiệu/rách-vỡ, không thể xuất: {string.Join(", ", bad.Take(10).Select(s => s.QrId))}", 0, "", 0);

        // tem đã xuất ở phiếu khác
        var alreadyShipped = await db.ShipmentLines.IgnoreQueryFilters()
            .Where(l => codes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        if (alreadyShipped.Count > 0)
            return new ShipResult(false, $"{alreadyShipped.Count} tem đã được xuất trước đó: {string.Join(", ", alreadyShipped.Take(10))}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(header.ShipmentNo)) header.ShipmentNo = $"PXK{DateTime.Now:yyMMddHHmmss}";
        header.ShipmentNo = header.ShipmentNo.Trim();
        if (await db.Shipments.AnyAsync(x => x.ShipmentNo == header.ShipmentNo))
            return new ShipResult(false, $"Mã phiếu {header.ShipmentNo} đã tồn tại.", 0, "", 0);

        header.Status = "PENDING";
        header.CreatedBy = createdBy;
        var now = DateTime.Now;
        foreach (var s in stamps)
            header.Lines.Add(new ShipmentLine { QrId = s.QrId, ProductId = s.ProductId, ShippedAt = now });
        db.Shipments.Add(header);
        await db.SaveChangesAsync();
        return new ShipResult(true, $"Đã tạo phiếu xuất {header.ShipmentNo} ({stamps.Count} tem).", header.Id, header.ShipmentNo, stamps.Count);
    }

    /// <summary>
    /// Xuất phiếu (ghi nhận tem ra khỏi kho). Mô phỏng bước OutInv của EQR:
    /// - Chỉ phiếu PENDING mới được xuất.
    /// - Xuất xong: Status = SHIPPED + ghi mốc/người xuất; đánh dấu tem đã xuất (ShipmentId, ShippedAt, CustomerCode).
    /// </summary>
    public async Task<ShipResult> ShipShipmentAsync(int id, string shippedBy)
    {
        var sh = await db.Shipments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (sh == null) return new ShipResult(false, "Không tìm thấy phiếu xuất.", 0, "", 0);
        if (sh.Status != "PENDING") return new ShipResult(false, $"Phiếu {sh.ShipmentNo} đang ở trạng thái {sh.Status}, không thể xuất.", sh.Id, sh.ShipmentNo, 0);

        var codes = sh.Lines.Select(l => l.QrId).ToList();
        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var now = DateTime.Now;
        foreach (var s in stamps)
        {
            s.ShipmentId = sh.Id;
            s.ShippedAt = now;
            s.CustomerCode = sh.CustomerCode;
        }
        sh.Status = "SHIPPED";
        sh.ShippedAt = now;
        sh.ShippedBy = shippedBy;
        await db.SaveChangesAsync();
        return new ShipResult(true, $"Đã xuất phiếu {sh.ShipmentNo} ({stamps.Count} tem).", sh.Id, sh.ShipmentNo, stamps.Count);
    }

    /// <summary>
    /// Hủy phiếu xuất kho theo tem. Mô phỏng nghiệp vụ
    /// WAS_Inv_VerifiedIDInOut_Cancel_New20230316 của EQR (Temp.cs):
    /// - Phiếu phải tồn tại và chưa bị hủy (không hủy trùng).
    /// - Phiếu đã xuất (SHIPPED) mới được hủy; phiếu PENDING chưa xuất thì không có gì để hủy.
    /// - Hủy xong: Status = CANCEL + ghi mốc/người/lý do hủy.
    /// - Giải phóng tem: gỡ liên kết phiếu xuất (ShipmentId/ShippedAt/CustomerCode) để tem
    ///   trở về trạng thái chưa xuất (đúng tinh thần EQR: tem trong phiếu bị hủy trở về tem trắng).
    /// </summary>
    public async Task<ShipCancelResult> CancelShipmentAsync(int id, string? reason, string cancelledBy)
    {
        var sh = await db.Shipments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (sh == null) return new ShipCancelResult(false, "Không tìm thấy phiếu xuất.", 0, "", 0);
        if (sh.Status == "CANCEL")
            return new ShipCancelResult(false, $"Phiếu {sh.ShipmentNo} đã bị hủy trước đó.", sh.Id, sh.ShipmentNo, 0);
        if (sh.Status != "SHIPPED")
            return new ShipCancelResult(false, $"Phiếu {sh.ShipmentNo} đang ở trạng thái {sh.Status}, chỉ hủy được phiếu đã xuất (SHIPPED).", sh.Id, sh.ShipmentNo, 0);

        var codes = sh.Lines.Select(l => l.QrId).ToList();
        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var now = DateTime.Now;
        foreach (var s in stamps)
        {
            s.ShipmentId = null;
            s.ShippedAt = null;
            s.CustomerCode = null;
        }
        sh.Status = "CANCEL";
        sh.CancelledAt = now;
        sh.CancelledBy = cancelledBy;
        sh.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await db.SaveChangesAsync();
        return new ShipCancelResult(true, $"Đã hủy phiếu {sh.ShipmentNo}, giải phóng {stamps.Count} tem về trạng thái chưa xuất.", sh.Id, sh.ShipmentNo, stamps.Count);
    }

    /// <summary>
    /// Gộp các phiếu xuất kho theo tem TRÙNG KHÓA về 1 phiếu duy nhất. Mô phỏng
    /// nghiệp vụ WAS_Inv_VerifiedIDInOut_Merge của EQR (Temp.cs → Inv_VerifiedIDInOut_MergeX):
    /// - Khóa gộp = (RefNoSys, ProductCode, UserMoveOrder) — cả 3 bắt buộc.
    /// - Phải có ÍT NHẤT 2 phiếu cùng khóa mới gộp được (InvalidQtyRows).
    /// - Mỗi lượt: chọn phiếu có ÍT tem nhất (QtyVerified nhỏ nhất) làm phiếu bị gộp,
    ///   chuyển toàn bộ tem của nó sang phiếu còn lại (phiếu đích), rồi xóa phiếu bị gộp.
    /// - Lặp lại cho tới khi chỉ còn 1 phiếu cho khóa đó.
    /// </summary>
    public async Task<ShipMergeResult> MergeShipmentsAsync(string refNoSys, string productCode, string userMoveOrder, string mergedBy)
    {
        refNoSys = (refNoSys ?? "").Trim();
        productCode = (productCode ?? "").Trim();
        userMoveOrder = (userMoveOrder ?? "").Trim();
        if (refNoSys.Length == 0) return new ShipMergeResult(false, "Cần nhập RefNoSys (đơn hàng nguồn).", 0, "", 0, 0);
        if (productCode.Length == 0) return new ShipMergeResult(false, "Cần nhập mã sản phẩm (ProductCode).", 0, "", 0, 0);
        if (userMoveOrder.Length == 0) return new ShipMergeResult(false, "Cần nhập UserMoveOrder (mã lệnh điều chuyển).", 0, "", 0, 0);

        var product = await db.Products.FirstOrDefaultAsync(p => p.Code == productCode);
        if (product == null) return new ShipMergeResult(false, $"Không tìm thấy sản phẩm {productCode}.", 0, "", 0, 0);

        // Các phiếu xuất cùng khóa gộp, chỉ gồm phiếu có tem của sản phẩm này
        var candidates = await db.Shipments.Include(x => x.Lines)
            .Where(x => x.RefNoSys == refNoSys && x.UserMoveOrder == userMoveOrder
                        && x.Status != "CANCEL" && x.MergedIntoId == null
                        && x.Lines.Any(l => l.ProductId == product.Id))
            .ToListAsync();

        if (candidates.Count <= 1)
            return new ShipMergeResult(false, $"Cần ít nhất 2 phiếu cùng khóa (RefNoSys={refNoSys}, SP={productCode}, UserMoveOrder={userMoveOrder}) để gộp; hiện có {candidates.Count}.", 0, "", 0, 0);

        var now = DateTime.Now;
        var mergedCount = 0;
        var movedStamps = 0;

        // Lặp: gộp phiếu ít tem nhất vào phiếu nhiều tem nhất cho tới khi còn 1 phiếu
        while (candidates.Count > 1)
        {
            var target = candidates.OrderByDescending(x => x.Lines.Count(l => l.ProductId == product.Id)).First();
            var source = candidates.Where(x => x.Id != target.Id)
                .OrderBy(x => x.Lines.Count(l => l.ProductId == product.Id)).First();

            // chuyển tem của phiếu nguồn sang phiếu đích
            var sourceLines = source.Lines.Where(l => l.ProductId == product.Id).ToList();
            var sourceQrIds = sourceLines.Select(l => l.QrId).ToList();
            var stamps = await db.Stamps.Where(s => sourceQrIds.Contains(s.QrId)).ToListAsync();
            foreach (var s in stamps)
            {
                s.ShipmentId = target.Id;
                s.ShippedAt = now;
                s.CustomerCode = target.CustomerCode;
            }
            foreach (var l in sourceLines)
                target.Lines.Add(new ShipmentLine { QrId = l.QrId, ProductId = l.ProductId, ShippedAt = now });

            // đánh dấu phiếu nguồn đã gộp rồi xóa
            source.MergedIntoId = target.Id;
            source.MergedAt = now;
            source.MergedBy = mergedBy;
            db.ShipmentLines.RemoveRange(sourceLines);
            db.Shipments.Remove(source);

            mergedCount++;
            movedStamps += stamps.Count;
            candidates.Remove(source);
        }

        await db.SaveChangesAsync();
        var kept = candidates[0];
        return new ShipMergeResult(true,
            $"Đã gộp {mergedCount} phiếu vào {kept.ShipmentNo} (chuyển {movedStamps} tem).",
            kept.Id, kept.ShipmentNo, mergedCount, movedStamps);
    }

    /// <summary>
    /// Cho phép sửa phiếu xuất kho theo tem. Mô phỏng nghiệp vụ
    /// WAS_Inv_VerifiedIDInOut_UpdFlagAllowModify_New20210922 của EQR (zTemp.cs →
    /// Inv_VerifiedIDInOut_UpdFlagAllowModifyX):
    /// - Lọc phiếu theo (RefNoSys, PlateNo) — PlateNo được CHUẨN HOÁ nhiễu (bỏ space/'-'/','/'.')
    ///   rồi so khớp với biển số lưu trên phiếu (đúng cách EQR so khớp QRCodeOS).
    /// - Chỉ mở khóa được trong THỜI HẠN cho phép (phút) kể từ khi tạo phiếu
    ///   (Mst_Param INBRAND_MINUTECHECK_UPDPXK) — quá hạn ⇒ từ chối.
    /// - Mở khóa xong: FlagAllowModify = true + ghi mốc/người mở khóa.
    /// </summary>
    public async Task<ShipAllowModifyResult> AllowModifyShipmentAsync(string refNoSys, string plateNo, int minutes, string by)
    {
        refNoSys = (refNoSys ?? "").Trim();
        plateNo = NormalizePlate(plateNo);
        if (refNoSys.Length == 0) return new ShipAllowModifyResult(false, "Cần nhập RefNoSys (đơn hàng nguồn).", 0, []);
        if (plateNo.Length == 0) return new ShipAllowModifyResult(false, "Cần nhập biển số xe (PlateNo).", 0, []);
        if (minutes <= 0) minutes = 60;   // mặc định 60 phút nếu không cấu hình

        // Lọc phiếu theo RefNoSys, rồi so khớp PlateNo đã chuẩn hoá nhiễu
        var byRef = await db.Shipments
            .Where(x => x.RefNoSys == refNoSys && x.Status != "CANCEL")
            .ToListAsync();
        var matched = byRef.Where(x => NormalizePlate(x.PlateNo) == plateNo).ToList();
        if (matched.Count == 0)
            return new ShipAllowModifyResult(false,
                $"Không tìm thấy phiếu xuất nào có RefNoSys={refNoSys} và biển số {plateNo}.", 0, []);

        // Ràng buộc thời hạn: chỉ mở khóa trong vòng `minutes` phút kể từ khi tạo phiếu
        var now = DateTime.Now;
        var inWindow = matched.Where(x => (now - x.CreatedAt).TotalMinutes <= minutes).ToList();
        if (inWindow.Count == 0)
            return new ShipAllowModifyResult(false,
                $"Đã quá thời hạn cho phép sửa phiếu ({minutes} phút kể từ khi tạo).", 0, []);

        foreach (var s in inWindow)
        {
            s.FlagAllowModify = true;
            s.AllowModifyAt = now;
            s.AllowModifyBy = by;
        }
        await db.SaveChangesAsync();
        return new ShipAllowModifyResult(true,
            $"Đã cho phép sửa {inWindow.Count} phiếu xuất (RefNoSys={refNoSys}, biển số {plateNo}).",
            inWindow.Count, inWindow.Select(x => x.ShipmentNo).ToList());
    }

    // Chuẩn hoá nhiễu biển số: bỏ space/'-'/','/'.' (đúng cách EQR xử lý PlateNo)
    private static string NormalizePlate(string? plate)
    {
        if (string.IsNullOrWhiteSpace(plate)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (var ch in plate)
            if (ch != ' ' && ch != '-' && ch != ',' && ch != '.') sb.Append(ch);
        return sb.ToString().ToUpperInvariant();
    }

    // ── KÍCH HOẠT THÔNG TIN SẢN XUẤT (InvF_ProductionActive) ────────
    public Task<List<ProductLife>> ProductLivesAsync() =>
        db.ProductLives.OrderBy(x => x.ValueByDay).ToListAsync();

    public Task<List<ProductionActive>> ProductionActivesAsync() =>
        db.ProductionActives.Include(x => x.Product).Include(x => x.ProductLife)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<ProductionActive?> GetProductionActiveAsync(int id) =>
        db.ProductionActives.Include(x => x.Product).Include(x => x.ProductLife)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Kích hoạt thông tin sản xuất cho 1 lô tem. Mô phỏng nghiệp vụ
    /// InvF_ProductionActive_Save của EQR:
    /// - RefNo bắt buộc và không được trùng với phiếu khác.
    /// - Origin (nguồn gốc) bắt buộc.
    /// - Sản phẩm phải tồn tại; hạn sử dụng (ProductLife) phải tồn tại.
    /// - Ngày hết hạn = Ngày SX + ProductLifeValue - 1 (tự suy ra, không nhận từ client).
    /// - Dải serial tem vào SX bắt buộc.
    /// </summary>
    public async Task<PaResult> CreateProductionActiveAsync(string paNo, string refNo, string origin, int productId,
        int qtyPlan, DateTime productDate, int productLifeId, string listSerialIn, string listSerialOut, string createdBy)
    {
        refNo = (refNo ?? "").Trim();
        origin = (origin ?? "").Trim();
        listSerialIn = (listSerialIn ?? "").Trim();
        listSerialOut = (listSerialOut ?? "").Trim();

        if (string.IsNullOrWhiteSpace(refNo))
            return new PaResult(false, "Số tham chiếu (RefNo) không được để trống.", 0, "", default);
        if (string.IsNullOrWhiteSpace(origin))
            return new PaResult(false, "Nguồn gốc (Origin) không được để trống.", 0, "", default);
        if (string.IsNullOrWhiteSpace(listSerialIn))
            return new PaResult(false, "Dải serial tem vào sản xuất không được để trống.", 0, "", default);

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return new PaResult(false, "Sản phẩm không tồn tại.", 0, "", default);

        var life = await db.ProductLives.FirstOrDefaultAsync(x => x.Id == productLifeId);
        if (life == null) return new PaResult(false, "Hạn sử dụng không tồn tại.", 0, "", default);

        if (await db.ProductionActives.AnyAsync(x => x.RefNo == refNo))
            return new PaResult(false, $"Số tham chiếu {refNo} đã được dùng ở phiếu khác.", 0, "", default);

        if (string.IsNullOrWhiteSpace(paNo)) paNo = $"PA{DateTime.Now:yyMMddHHmmss}";
        paNo = paNo.Trim();
        if (await db.ProductionActives.AnyAsync(x => x.PaNo == paNo))
            return new PaResult(false, $"Mã phiếu {paNo} đã tồn tại.", 0, "", default);

        if (productDate == default) productDate = DateTime.Today;
        // Ngày hết hạn = Ngày SX + ProductLifeValue - 1 (đúng công thức EQR)
        var expiry = productDate.AddDays(Math.Max(life.Value, 1) - 1);

        var pa = new ProductionActive
        {
            PaNo = paNo, RefNo = refNo, Origin = origin, ProductId = productId,
            QtyPlan = qtyPlan, ProductDate = productDate, ExpiryDate = expiry,
            ProductLifeId = productLifeId, ListSerialInManufacture = listSerialIn,
            ListSerialOutManufacture = listSerialOut, CreatedBy = createdBy
        };
        db.ProductionActives.Add(pa);
        await db.SaveChangesAsync();
        return new PaResult(true, $"Đã kích hoạt thông tin sản xuất {pa.PaNo} — hết hạn {expiry:dd/MM/yyyy}.", pa.Id, pa.PaNo, expiry);
    }

    /// <summary>
    /// Xóa phiếu kích hoạt. Mô phỏng ràng buộc EQR: chỉ cho xóa trong 72h kể từ khi tạo.
    /// </summary>
    public async Task<PaResult> DeleteProductionActiveAsync(int id)
    {
        var pa = await db.ProductionActives.FirstOrDefaultAsync(x => x.Id == id);
        if (pa == null) return new PaResult(false, "Không tìm thấy phiếu kích hoạt.", 0, "", default);
        if ((DateTime.Now - pa.CreatedAt).TotalHours > 72)
            return new PaResult(false, $"Phiếu {pa.PaNo} đã tạo quá 72 giờ, không thể xóa.", pa.Id, pa.PaNo, pa.ExpiryDate);

        db.ProductionActives.Remove(pa);
        await db.SaveChangesAsync();
        return new PaResult(true, $"Đã xóa phiếu kích hoạt {pa.PaNo}.", 0, pa.PaNo, pa.ExpiryDate);
    }

    // ── PHIÊN SẢN XUẤT (InvF_ProductionSession) ─────────────────────
    public Task<List<ProductionSession>> ProductionSessionsAsync() =>
        db.ProductionSessions.Include(x => x.Product).Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<ProductionSession?> GetProductionSessionAsync(int id) =>
        db.ProductionSessions.Include(x => x.Product).Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Ghi nhận 1 phiên sản xuất (ca sản xuất) — bước (2) vòng đời tem.
    /// Mô phỏng nghiệp vụ InvF_ProductionSession_Add của EQR (zTemp.cs):
    /// - IF_PSNo (mã phiên) bắt buộc và không được trùng phiên khác.
    /// - Sản phẩm (nếu có) phải tồn tại.
    /// - Phải có ít nhất 1 dòng tem (IDNo).
    /// - Mọi IDNo phải tồn tại trong kho sinh số (Inv_InventoryGenID).
    /// - QtyVerified = số tem thực tế đã ghi nhận vào phiên.
    /// - "Rút ruột": tem đã thuộc phiên khác thì gỡ khỏi phiên cũ, chuyển sang phiên này.
    /// </summary>
    public async Task<PsResult> CreateProductionSessionAsync(string psNo, string? orgCode, string? shiftCode, string? lotCode,
        int productId, int qtyInput, IEnumerable<string> qrIds, string? remark, string createdBy)
    {
        psNo = (psNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(psNo))
            return new PsResult(false, "Mã phiên sản xuất (IF_PSNo) không được để trống.", 0, "", 0);

        if (await db.ProductionSessions.AnyAsync(x => x.PsNo == psNo))
            return new PsResult(false, $"Mã phiên {psNo} đã tồn tại.", 0, psNo, 0);

        if (productId > 0 && !await db.Products.AnyAsync(p => p.Id == productId))
            return new PsResult(false, "Sản phẩm không tồn tại.", 0, psNo, 0);

        // Chuẩn hoá danh sách tem: bỏ rỗng, giữ thứ tự quét, chống trùng trong cùng phiên.
        var list = (qrIds ?? []).Select(x => (x ?? "").Trim())
            .Where(x => x.Length > 0).Distinct().ToList();
        if (list.Count == 0)
            return new PsResult(false, "Phải có ít nhất 1 tem (IDNo) trong phiên sản xuất.", 0, psNo, 0);

        // Mọi IDNo phải tồn tại trong kho sinh số (Inv_InventoryGenID).
        var existing = await db.Stamps.Where(s => list.Contains(s.QrId)).Select(s => s.QrId).ToListAsync();
        var missing = list.Except(existing).ToList();
        if (missing.Count > 0)
            return new PsResult(false, $"Tem không tồn tại trong kho sinh số: {string.Join(", ", missing)}.", 0, psNo, 0);

        var ps = new ProductionSession
        {
            PsNo = psNo, OrgCode = orgCode, ShiftCode = shiftCode, LotCode = lotCode,
            ProductId = productId, QtyInput = qtyInput, Remark = remark, CreatedBy = createdBy
        };
        int idx = 0;
        foreach (var qr in list)
            ps.Lines.Add(new ProductionSessionLine { QrId = qr, Idx = ++idx });
        ps.QtyVerified = ps.Lines.Count;

        // "Rút ruột": gỡ các tem này khỏi phiên cũ (nếu đã thuộc phiên khác) rồi mới gắn vào phiên mới.
        var oldLines = await db.ProductionSessionLines.Where(l => list.Contains(l.QrId)).ToListAsync();
        if (oldLines.Count > 0) db.ProductionSessionLines.RemoveRange(oldLines);

        db.ProductionSessions.Add(ps);
        await db.SaveChangesAsync();
        return new PsResult(true, $"Đã ghi nhận phiên sản xuất {ps.PsNo} với {ps.QtyVerified} tem.", ps.Id, ps.PsNo, ps.QtyVerified);
    }

    // ── DANH MỤC NGUỒN GỐC (Mst_NguonGoc) ───────────────────────────
    /// <summary>
    /// Danh sách nguồn gốc. Nếu có từ khóa q → lọc theo Code/Name/DisplayName
    /// (phục vụ Auto Complete, giống Ft_WhereClause của EQR).
    /// </summary>
    public async Task<List<OriginCatalog>> OriginsAsync(string? q)
    {
        var query = db.OriginCatalogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            query = query.Where(x => x.Code.Contains(k) || x.Name.Contains(k) || x.DisplayName.Contains(k));
        }
        return await query.OrderBy(x => x.Code).Take(200).ToListAsync();
    }

    public Task<OriginCatalog?> GetOriginAsync(int id) =>
        db.OriginCatalogs.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo 1 nguồn gốc. Mô phỏng nghiệp vụ Mst_NguonGoc_Create của EQR:
    /// - Code bắt buộc và không được trùng.
    /// - Name bắt buộc.
    /// - CertificateDateStart <= CertificateDateEnd.
    /// - DisplayName do server tự dựng = "&lt;Code&gt; (&lt;CertCode&gt; &lt;CertNo&gt;)" (rỗng chứng chỉ ⇒ = Code).
    /// </summary>
    public async Task<OriginResult> CreateOriginAsync(OriginCatalog o, string createdBy)
    {
        o.Code = (o.Code ?? "").Trim();
        o.Name = (o.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(o.Code)) return new OriginResult(false, "Mã nguồn gốc không được để trống.", 0, "");
        if (string.IsNullOrWhiteSpace(o.Name)) return new OriginResult(false, "Tên nguồn gốc không được để trống.", 0, "");
        if (o.CertificateDateStart != null && o.CertificateDateEnd != null && o.CertificateDateStart > o.CertificateDateEnd)
            return new OriginResult(false, "Ngày bắt đầu chứng chỉ phải nhỏ hơn hoặc bằng ngày kết thúc.", 0, "");
        if (await db.OriginCatalogs.AnyAsync(x => x.Code == o.Code))
            return new OriginResult(false, $"Mã nguồn gốc {o.Code} đã tồn tại.", 0, "");

        o.DisplayName = BuildDisplayName(o);
        o.CreatedBy = createdBy;
        db.OriginCatalogs.Add(o);
        await db.SaveChangesAsync();
        return new OriginResult(true, $"Đã tạo nguồn gốc {o.Code}.", o.Id, o.Code);
    }

    /// <summary>
    /// Cập nhật 1 nguồn gốc. Mô phỏng nghiệp vụ Mst_NguonGoc_Update của EQR:
    /// - Bản ghi phải tồn tại; Name không rỗng.
    /// - Kiểm tra khoảng ngày chứng chỉ.
    /// - Tự dựng lại DisplayName để chuỗi gợi ý không lệch dữ liệu.
    /// </summary>
    public async Task<OriginResult> UpdateOriginAsync(int id, OriginCatalog o)
    {
        var cur = await db.OriginCatalogs.FirstOrDefaultAsync(x => x.Id == id);
        if (cur == null) return new OriginResult(false, "Không tìm thấy nguồn gốc.", 0, "");

        o.Name = (o.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(o.Name)) return new OriginResult(false, "Tên nguồn gốc không được để trống.", cur.Id, cur.Code);
        if (o.CertificateDateStart != null && o.CertificateDateEnd != null && o.CertificateDateStart > o.CertificateDateEnd)
            return new OriginResult(false, "Ngày bắt đầu chứng chỉ phải nhỏ hơn hoặc bằng ngày kết thúc.", cur.Id, cur.Code);

        cur.Name = o.Name;
        cur.CertificateCode = o.CertificateCode;
        cur.CertificateNo = o.CertificateNo;
        cur.CertificateName = o.CertificateName;
        cur.CertificateDateStart = o.CertificateDateStart;
        cur.CertificateDateEnd = o.CertificateDateEnd;
        cur.Address = o.Address;
        cur.GlnCode = o.GlnCode;
        cur.Remark = o.Remark;
        cur.IsActive = o.IsActive;
        cur.DisplayName = BuildDisplayName(cur);
        await db.SaveChangesAsync();
        return new OriginResult(true, $"Đã cập nhật nguồn gốc {cur.Code}.", cur.Id, cur.Code);
    }

    /// <summary>
    /// Xóa 1 nguồn gốc. Mô phỏng nghiệp vụ Mst_NguonGoc_Delete của EQR:
    /// - Bản ghi phải tồn tại.
    /// - CHẶN xóa nếu đã được dùng: có phiếu kích hoạt SX với Origin = Code.
    /// </summary>
    public async Task<OriginResult> DeleteOriginAsync(int id)
    {
        var cur = await db.OriginCatalogs.FirstOrDefaultAsync(x => x.Id == id);
        if (cur == null) return new OriginResult(false, "Không tìm thấy nguồn gốc.", 0, "");

        var inUse = await db.ProductionActives.AnyAsync(x => x.Origin == cur.Code);
        if (inUse)
            return new OriginResult(false, $"Nguồn gốc {cur.Code} đang được dùng ở phiếu kích hoạt SX, không thể xóa.", cur.Id, cur.Code);

        db.OriginCatalogs.Remove(cur);
        await db.SaveChangesAsync();
        return new OriginResult(true, $"Đã xóa nguồn gốc {cur.Code}.", 0, cur.Code);
    }

    /// <summary>Dựng chuỗi gợi ý: "&lt;Code&gt; (&lt;CertCode&gt; &lt;CertNo&gt;)"; rỗng chứng chỉ ⇒ = Code.</summary>
    private static string BuildDisplayName(OriginCatalog o)
    {
        var cert = string.Join(" ", new[] { o.CertificateCode, o.CertificateNo }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        return string.IsNullOrWhiteSpace(cert) ? o.Code : $"{o.Code} ({cert})";
    }

    // ── DANH MỤC ĐỊA ĐIỂM GS1 (Mst_GLN) ────────────────────────────
    /// <summary>
    /// Danh sách địa điểm GS1. Nếu có từ khóa q → lọc theo Code/Name/OrgCode
    /// (phục vụ Auto Complete, giống Ft_WhereClause của EQR).
    /// </summary>
    public async Task<List<Gs1Location>> Gs1LocationsAsync(string? q)
    {
        var query = db.Gs1Locations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            query = query.Where(x => x.Code.Contains(k) || x.Name.Contains(k) || (x.OrgCode != null && x.OrgCode.Contains(k)));
        }
        return await query.OrderBy(x => x.Code).Take(200).ToListAsync();
    }

    public Task<Gs1Location?> GetGs1LocationAsync(int id) =>
        db.Gs1Locations.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo 1 địa điểm GS1. Mô phỏng nghiệp vụ Mst_GLN_Create_New20210408 của EQR:
    /// - GLNCode bắt buộc và không được trùng (Mst_GLN_CheckDB_MstGLNExist).
    /// - GLNName bắt buộc.
    /// - OrgID (đơn vị sở hữu) bắt buộc.
    /// </summary>
    public async Task<GlnResult> CreateGs1LocationAsync(Gs1Location g, string createdBy)
    {
        g.Code = (g.Code ?? "").Trim();
        g.Name = (g.Name ?? "").Trim();
        g.OrgCode = string.IsNullOrWhiteSpace(g.OrgCode) ? null : g.OrgCode.Trim();
        if (string.IsNullOrWhiteSpace(g.Code)) return new GlnResult(false, "Mã địa điểm GS1 (GLNCode) không được để trống.", 0, "");
        if (string.IsNullOrWhiteSpace(g.Name)) return new GlnResult(false, "Tên địa điểm (GLNName) không được để trống.", 0, g.Code);
        if (string.IsNullOrWhiteSpace(g.OrgCode)) return new GlnResult(false, "Đơn vị sở hữu (OrgID) không được để trống.", 0, g.Code);
        if (await db.Gs1Locations.AnyAsync(x => x.Code == g.Code))
            return new GlnResult(false, $"Mã địa điểm GS1 {g.Code} đã tồn tại.", 0, g.Code);

        g.CreatedBy = createdBy;
        db.Gs1Locations.Add(g);
        await db.SaveChangesAsync();
        return new GlnResult(true, $"Đã tạo địa điểm GS1 {g.Code}.", g.Id, g.Code);
    }

    /// <summary>
    /// Cập nhật 1 địa điểm GS1. Mô phỏng nghiệp vụ Mst_GLN_Update_New20210408 của EQR:
    /// - Bản ghi phải tồn tại (Mst_GLN_CheckDB_MstGLNNotFound).
    /// - Nếu đổi tên thì tên không được rỗng.
    /// - Đơn vị sở hữu phải tồn tại.
    /// </summary>
    public async Task<GlnResult> UpdateGs1LocationAsync(int id, Gs1Location g)
    {
        var cur = await db.Gs1Locations.FirstOrDefaultAsync(x => x.Id == id);
        if (cur == null) return new GlnResult(false, "Không tìm thấy địa điểm GS1.", 0, "");

        g.Name = (g.Name ?? "").Trim();
        g.OrgCode = string.IsNullOrWhiteSpace(g.OrgCode) ? null : g.OrgCode.Trim();
        if (string.IsNullOrWhiteSpace(g.Name)) return new GlnResult(false, "Tên địa điểm (GLNName) không được để trống.", cur.Id, cur.Code);
        if (string.IsNullOrWhiteSpace(g.OrgCode)) return new GlnResult(false, "Đơn vị sở hữu (OrgID) không được để trống.", cur.Id, cur.Code);

        cur.Name = g.Name;
        cur.GpsLat = g.GpsLat;
        cur.GpsLong = g.GpsLong;
        cur.OrgCode = g.OrgCode;
        cur.Remark = g.Remark;
        cur.IsActive = g.IsActive;
        cur.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return new GlnResult(true, $"Đã cập nhật địa điểm GS1 {cur.Code}.", cur.Id, cur.Code);
    }

    /// <summary>
    /// Xóa 1 địa điểm GS1. Mô phỏng nghiệp vụ Mst_GLN_Delete_New20210409 của EQR:
    /// - Bản ghi phải tồn tại (Mst_GLN_CheckDB_MstGLNNotFound).
    /// </summary>
    public async Task<GlnResult> DeleteGs1LocationAsync(int id)
    {
        var cur = await db.Gs1Locations.FirstOrDefaultAsync(x => x.Id == id);
        if (cur == null) return new GlnResult(false, "Không tìm thấy địa điểm GS1.", 0, "");

        db.Gs1Locations.Remove(cur);
        await db.SaveChangesAsync();
        return new GlnResult(true, $"Đã xóa địa điểm GS1 {cur.Code}.", 0, cur.Code);
    }

    // ── TRUY XUẤT NGUỒN GỐC GS1 (Mst_CTE / Mst_KDE / CTE_KDE / Event_Event) ─
    public Task<List<TraceEventType>> TraceEventTypesAsync() =>
        db.TraceEventTypes.Include(x => x.Kdes).ThenInclude(k => k.TraceKde)
            .OrderBy(x => x.Code).ToListAsync();

    public Task<TraceEventType?> GetTraceEventTypeAsync(int id) =>
        db.TraceEventTypes.Include(x => x.Kdes).ThenInclude(k => k.TraceKde)
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<TraceKde>> TraceKdesAsync() =>
        db.TraceKdes.OrderBy(x => x.Code).ToListAsync();

    public Task<List<TraceEvent>> TraceEventsAsync(string? cteCode)
    {
        var query = db.TraceEvents.Include(x => x.Specs).AsQueryable();
        if (!string.IsNullOrWhiteSpace(cteCode)) query = query.Where(x => x.CteCode == cteCode);
        return query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync();
    }

    public Task<TraceEvent?> GetTraceEventAsync(int id) =>
        db.TraceEvents.Include(x => x.Specs).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Lưu 1 sự kiện truy xuất GS1. Mô phỏng nghiệp vụ
    /// WAS_Event_Event_Save_New20210922 của EQR:
    /// - CTECode bắt buộc và phải tồn tại (đang hoạt động).
    /// - UIStyleCode bắt buộc.
    /// - Mọi KDE phải thuộc CTE (tồn tại cặp CTE_KDE) — nếu không ⇒ từ chối.
    /// - Tối đa 1 KDE dạng danh sách (FlagList) cho mỗi sự kiện.
    /// - Các KDE khóa (FlagKey) bắt buộc có giá trị và phải đủ số lượng khóa.
    /// - Nếu đã có sự kiện cùng bộ giá trị khóa ⇒ cập nhật (UPDATE) thay vì tạo mới.
    /// </summary>
    public async Task<TraceEventResult> SaveTraceEventAsync(string? eventNo, string cteCode, string uiStyleCode,
        string? glnOrgCode, string? remark, IEnumerable<(string KdeCode, string KdeValue)> specs, string createdBy)
    {
        cteCode = (cteCode ?? "").Trim();
        uiStyleCode = (uiStyleCode ?? "").Trim();
        if (string.IsNullOrWhiteSpace(cteCode))
            return new TraceEventResult(false, "Loại sự kiện (CTECode) không được để trống.", 0, "", "");
        if (string.IsNullOrWhiteSpace(uiStyleCode))
            return new TraceEventResult(false, "Kiểu hiển thị (UIStyleCode) không được để trống.", 0, "", "");

        var cte = await db.TraceEventTypes.Include(x => x.Kdes).ThenInclude(k => k.TraceKde)
            .FirstOrDefaultAsync(x => x.Code == cteCode && x.IsActive);
        if (cte == null)
            return new TraceEventResult(false, $"Loại sự kiện {cteCode} không tồn tại hoặc đã ngừng.", 0, "", "");

        var rows = (specs ?? []).Select(s => (KdeCode: (s.KdeCode ?? "").Trim(), KdeValue: (s.KdeValue ?? "").Trim()))
            .Where(s => s.KdeCode.Length > 0).ToList();
        if (rows.Count == 0)
            return new TraceEventResult(false, "Sự kiện phải có ít nhất 1 trường dữ liệu (KDE).", 0, "", "");

        // mọi KDE phải thuộc CTE (CTECode_KDECodeNotFound)
        var allowed = cte.Kdes.Select(k => k.TraceKde.Code).ToHashSet();
        var bad = rows.Select(r => r.KdeCode).Where(c => !allowed.Contains(c)).Distinct().ToList();
        if (bad.Count > 0)
            return new TraceEventResult(false, $"Trường dữ liệu không thuộc loại sự kiện {cteCode}: {string.Join(", ", bad)}", 0, "", "");

        // tối đa 1 KDE dạng danh sách (AllowOnlyOneListPerEvent)
        var listCodes = cte.Kdes.Where(k => k.IsList).Select(k => k.TraceKde.Code).ToHashSet();
        if (rows.Count(r => listCodes.Contains(r.KdeCode)) > 1)
            return new TraceEventResult(false, "Mỗi sự kiện chỉ được có tối đa 1 trường dạng danh sách.", 0, "", "");

        // KDE khóa bắt buộc có giá trị + phải đủ số lượng khóa
        var keyCodes = cte.Kdes.Where(k => k.IsKey).Select(k => k.TraceKde.Code).ToList();
        var emptyKey = rows.Where(r => keyCodes.Contains(r.KdeCode) && string.IsNullOrWhiteSpace(r.KdeValue))
            .Select(r => r.KdeCode).ToList();
        if (emptyKey.Count > 0)
            return new TraceEventResult(false, $"Trường khóa bắt buộc có giá trị: {string.Join(", ", emptyKey)}", 0, "", "");
        var inputKeyCount = rows.Count(r => keyCodes.Contains(r.KdeCode));
        if (inputKeyCount != keyCodes.Count)
            return new TraceEventResult(false, $"Sự kiện chưa đủ trường khóa: cần {keyCodes.Count}, có {inputKeyCount}.", 0, "", "");

        // tìm sự kiện trùng theo bộ giá trị khóa (nếu có ⇒ UPDATE)
        var keyValues = rows.Where(r => keyCodes.Contains(r.KdeCode)).ToDictionary(r => r.KdeCode, r => r.KdeValue);
        TraceEvent? existing = null;
        if (keyCodes.Count > 0)
        {
            var candidates = await db.TraceEvents.Include(x => x.Specs)
                .Where(x => x.CteCode == cteCode).ToListAsync();
            existing = candidates.FirstOrDefault(ev => keyCodes.All(kc =>
                ev.Specs.Any(sp => sp.KdeCode == kc && sp.KdeValue == keyValues[kc])));
        }

        var now = DateTime.Now;
        if (existing != null)
        {
            db.TraceEventSpecs.RemoveRange(existing.Specs);
            existing.Specs.Clear();
            foreach (var r in rows)
                existing.Specs.Add(new TraceEventSpec { CteCode = cteCode, KdeCode = r.KdeCode, KdeValue = r.KdeValue });
            existing.UIStyleCode = uiStyleCode;
            existing.GLNOrgCode = glnOrgCode;
            existing.Remark = remark;
            existing.TplVECode = cte.TplVECode;
            existing.TplVEDetail = cte.TplVEDetail;
            existing.UpdatedAt = now;
            await db.SaveChangesAsync();
            return new TraceEventResult(true, $"Đã cập nhật sự kiện {existing.EventNo}.", existing.Id, existing.EventNo, "UPDATE");
        }

        if (string.IsNullOrWhiteSpace(eventNo)) eventNo = $"EV{DateTime.Now:yyMMddHHmmss}";
        eventNo = eventNo.Trim();
        if (await db.TraceEvents.AnyAsync(x => x.EventNo == eventNo))
            return new TraceEventResult(false, $"Mã sự kiện {eventNo} đã tồn tại.", 0, "", "");

        var ev = new TraceEvent
        {
            EventNo = eventNo, CteCode = cteCode, UIStyleCode = uiStyleCode, GLNOrgCode = glnOrgCode,
            TplVECode = cte.TplVECode, TplVEDetail = cte.TplVEDetail, Remark = remark, CreatedBy = createdBy
        };
        foreach (var r in rows)
            ev.Specs.Add(new TraceEventSpec { CteCode = cteCode, KdeCode = r.KdeCode, KdeValue = r.KdeValue });
        db.TraceEvents.Add(ev);
        await db.SaveChangesAsync();
        return new TraceEventResult(true, $"Đã tạo sự kiện {ev.EventNo} ({rows.Count} trường dữ liệu).", ev.Id, ev.EventNo, "ADD");
    }

    // ── HÓA ĐƠN ĐIỆN TỬ (Invoice_Invoice) ────────────────────────
    public Task<List<Invoice>> InvoicesAsync() =>
        db.Invoices.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Invoice?> GetInvoiceAsync(int id) =>
        db.Invoices.Include(x => x.Lines).ThenInclude(l => l.Product).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo hóa đơn điện tử. Mô phỏng nghiệp vụ Invoice_Invoice_Save_Root của EQR:
    /// - InvoiceCode bắt buộc và duy nhất.
    /// - Phải có ít nhất 1 dòng mặt hàng (SL > 0); mặt hàng phải tồn tại.
    /// - Ngày hóa đơn không được ở tương lai.
    /// - Thành tiền từng dòng = SL × đơn giá; Tổng thanh toán = tiền hàng + VAT.
    /// - Phiếu mới ở trạng thái PENDING.
    /// </summary>
    public async Task<InvoiceResult> CreateInvoiceAsync(Invoice header,
        IEnumerable<(int ProductId, int Qty, decimal UnitPrice)> lines, string createdBy)
    {
        var rows = (lines ?? []).Where(l => l.ProductId > 0 && l.Qty > 0).ToList();
        if (rows.Count == 0) return new InvoiceResult(false, "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0.", 0, "", null);

        header.InvoiceCode = (header.InvoiceCode ?? "").Trim();
        if (string.IsNullOrWhiteSpace(header.InvoiceCode))
            return new InvoiceResult(false, "Mã hóa đơn (InvoiceCode) không được để trống.", 0, "", null);

        if (header.InvoiceDate == default) header.InvoiceDate = DateTime.Today;
        if (header.InvoiceDate.Date > DateTime.Today)
            return new InvoiceResult(false, "Ngày hóa đơn không được ở tương lai.", 0, header.InvoiceCode, null);

        var productIds = rows.Select(r => r.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
        var bad = productIds.Except(products.Select(p => p.Id)).ToList();
        if (bad.Count > 0) return new InvoiceResult(false, $"Mặt hàng không tồn tại: {string.Join(", ", bad)}", 0, header.InvoiceCode, null);
        var codeById = products.ToDictionary(p => p.Id, p => p.Code);

        if (await db.Invoices.AnyAsync(x => x.InvoiceCode == header.InvoiceCode))
            return new InvoiceResult(false, $"Mã hóa đơn {header.InvoiceCode} đã tồn tại.", 0, header.InvoiceCode, null);

        header.Status = "PENDING";
        header.CreatedBy = createdBy;
        header.InvoiceNo = null;
        foreach (var r in rows)
            header.Lines.Add(new InvoiceDtl
            {
                ProductId = r.ProductId,
                PartCode = codeById[r.ProductId],
                Qty = r.Qty,
                UnitPrice = r.UnitPrice,
                Amount = r.Qty * r.UnitPrice
            });
        header.TotalValInvoice = header.Lines.Sum(l => l.Amount);
        header.TotalValPmt = header.TotalValInvoice + header.TotalValVat;

        db.Invoices.Add(header);
        await db.SaveChangesAsync();
        return new InvoiceResult(true, $"Đã tạo hóa đơn {header.InvoiceCode} ({rows.Count} dòng, tổng {header.TotalValPmt:N0} đ).", header.Id, header.InvoiceCode, null);
    }

    /// <summary>
    /// Duyệt hóa đơn. Mô phỏng Invoice_Invoice_Approved của EQR: chỉ phiếu PENDING mới duyệt được.
    /// </summary>
    public async Task<InvoiceResult> ApproveInvoiceAsync(int id, string approvedBy)
    {
        var inv = await db.Invoices.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return new InvoiceResult(false, "Không tìm thấy hóa đơn.", 0, "", null);
        if (inv.Status != "PENDING")
            return new InvoiceResult(false, $"Hóa đơn {inv.InvoiceCode} đang ở trạng thái {inv.Status}, không thể duyệt.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);

        inv.Status = "APPROVED";
        inv.ApprovedAt = DateTime.Now;
        inv.ApprovedBy = approvedBy;
        await db.SaveChangesAsync();
        return new InvoiceResult(true, $"Đã duyệt hóa đơn {inv.InvoiceCode}.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);
    }

    /// <summary>
    /// Cấp số & phát hành hóa đơn. Mô phỏng Invoice_Invoice_AllocatedInv / _Issued của EQR:
    /// - Chỉ hóa đơn APPROVED mới phát hành được.
    /// - Lấy số kế tiếp trong dải [InvoiceNoStart..InvoiceNoEnd] của mẫu hóa đơn.
    /// - Hết dải ⇒ từ chối (InvalidQtyIssueRemain).
    /// </summary>
    public async Task<InvoiceResult> IssueInvoiceAsync(int id, string issuedBy)
    {
        var inv = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return new InvoiceResult(false, "Không tìm thấy hóa đơn.", 0, "", null);
        if (inv.Status != "APPROVED")
            return new InvoiceResult(false, $"Hóa đơn {inv.InvoiceCode} đang ở trạng thái {inv.Status}, chỉ phát hành hóa đơn đã duyệt.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);
        if (inv.InvoiceNoStart <= 0 || inv.InvoiceNoEnd < inv.InvoiceNoStart)
            return new InvoiceResult(false, "Mẫu hóa đơn chưa có dải số hợp lệ (Start/End).", inv.Id, inv.InvoiceCode, null);

        // số đã dùng của cùng mẫu (đã phát hành)
        var used = await db.Invoices.Where(x => x.TInvoiceCode == inv.TInvoiceCode && x.Status == "ISSUED" && x.InvoiceNo != null)
            .Select(x => x.InvoiceNo!).ToListAsync();
        var usedSet = used.ToHashSet();
        long? next = null;
        for (long n = inv.InvoiceNoStart; n <= inv.InvoiceNoEnd; n++)
            if (!usedSet.Contains(n.ToString())) { next = n; break; }
        if (next == null)
            return new InvoiceResult(false, $"Mẫu {inv.TInvoiceCode} đã hết số trong dải {inv.InvoiceNoStart}..{inv.InvoiceNoEnd}.", inv.Id, inv.InvoiceCode, null);

        inv.InvoiceNo = next.Value.ToString();
        inv.Status = "ISSUED";
        inv.IssuedAt = DateTime.Now;
        inv.IssuedBy = issuedBy;
        await db.SaveChangesAsync();
        return new InvoiceResult(true, $"Đã phát hành hóa đơn {inv.InvoiceCode} — số {inv.InvoiceNo}.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);
    }

    /// <summary>
    /// Hủy hóa đơn. Mô phỏng Invoice_Invoice_Cancel của EQR:
    /// - Không hủy hóa đơn đã hủy; hóa đơn đã phát hành phải ghi lý do.
    /// </summary>
    public async Task<InvoiceResult> CancelInvoiceAsync(int id, string? reason)
    {
        var inv = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return new InvoiceResult(false, "Không tìm thấy hóa đơn.", 0, "", null);
        if (inv.Status == "CANCEL")
            return new InvoiceResult(false, $"Hóa đơn {inv.InvoiceCode} đã được hủy trước đó.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);
        if (inv.Status == "ISSUED" && string.IsNullOrWhiteSpace(reason))
            return new InvoiceResult(false, "Hóa đơn đã phát hành — cần ghi lý do hủy.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);

        inv.Status = "CANCEL";
        inv.CancelledAt = DateTime.Now;
        inv.CancelReason = reason;
        await db.SaveChangesAsync();
        return new InvoiceResult(true, $"Đã hủy hóa đơn {inv.InvoiceCode}.", inv.Id, inv.InvoiceCode, inv.InvoiceNo);
    }

    // ── KÍCH HOẠT BÁN HÀNG (Inv_InvVerifiedID_ActivateSales) ────────
    public Task<List<SalesActivation>> SalesActivationsAsync() =>
        db.SalesActivations.Include(x => x.Product).Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<SalesActivation?> GetSalesActivationAsync(int id) =>
        db.SalesActivations.Include(x => x.Product).Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Kích hoạt bán hàng cho 1 tập tem. Mô phỏng nghiệp vụ
    /// WAS_Inv_InvVerifiedID_ActivateSales_New20210614 của EQR (zTemp.cs):
    /// - Phải có ít nhất 1 tem; mọi tem phải tồn tại trong hệ thống.
    /// - Tem đã vô hiệu (Void) hoặc rách/vỡ (Broken) → từ chối.
    /// - Tem đã kích hoạt bán hàng trước đó → từ chối (không kích hoạt trùng).
    /// - Sinh 1 phiếu xuất nội bộ (RefType=INVOUT, tiền tố PXKHT) trỏ về khách
    ///   hàng bán mặc định; đánh dấu từng tem FlagSales='1' + SalesDTime + CustomerCode.
    /// </summary>
    public async Task<SalesResult> ActivateSalesAsync(string saNo, int productId, string? customerCode,
        string? customerName, DateTime salesDTime, IEnumerable<string> qrIds, string? remark, string createdBy)
    {
        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new SalesResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();

        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new SalesResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        var bad = stamps.Where(s => s.Status == StampStatus.Void || s.Status == StampStatus.Broken).ToList();
        if (bad.Count > 0)
            return new SalesResult(false, $"{bad.Count} tem đã vô hiệu/rách-vỡ, không thể kích hoạt bán: {string.Join(", ", bad.Take(10).Select(s => s.QrId))}", 0, "", 0);

        // tem đã kích hoạt bán ở phiếu khác
        var alreadySold = await db.SalesActivationLines.IgnoreQueryFilters()
            .Where(l => codes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        if (alreadySold.Count > 0)
            return new SalesResult(false, $"{alreadySold.Count} tem đã được kích hoạt bán trước đó: {string.Join(", ", alreadySold.Take(10))}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(saNo)) saNo = $"PXKHT{DateTime.Now:yyMMddHHmmss}";
        saNo = saNo.Trim();
        if (await db.SalesActivations.AnyAsync(x => x.SaNo == saNo))
            return new SalesResult(false, $"Mã phiếu {saNo} đã tồn tại.", 0, "", 0);

        if (salesDTime == default) salesDTime = DateTime.Now;
        var now = DateTime.Now;
        var sa = new SalesActivation
        {
            SaNo = saNo,
            RefNoSys = $"PXKHT.{now:yyyyMMdd.HHmmss}.0",   // đúng định dạng RefNoSys của EQR
            RefType = "INVOUT",
            ProductId = productId > 0 ? productId : stamps[0].ProductId,
            CustomerCode = string.IsNullOrWhiteSpace(customerCode) ? null : customerCode.Trim(),
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim(),
            SalesDTime = salesDTime,
            Remark = remark,
            CreatedBy = createdBy
        };
        foreach (var s in stamps)
        {
            sa.Lines.Add(new SalesActivationLine { QrId = s.QrId, SalesDTime = salesDTime });
            s.FlagSales = true;
            s.SalesDTime = salesDTime;
            s.CustomerCode = sa.CustomerCode;
        }
        sa.Quantity = stamps.Count;
        db.SalesActivations.Add(sa);
        await db.SaveChangesAsync();

        // gán lại SalesActivationId cho tem (sau khi có Id phiếu)
        foreach (var s in stamps) s.SalesActivationId = sa.Id;
        await db.SaveChangesAsync();

        return new SalesResult(true, $"Đã kích hoạt bán hàng {sa.SaNo} ({stamps.Count} tem).", sa.Id, sa.SaNo, stamps.Count);
    }

    /// <summary>
    /// Hoàn tác (đưa về trạng thái chưa kích hoạt bán hàng) 1 phiếu kích hoạt bán hàng.
    /// Mô phỏng nghiệp vụ WAS_Inv_InventoryVerifiedID_FlagSalesBackStatus của EQR (zTemp.cs):
    /// - Phiếu phải tồn tại; không hoàn tác phiếu đã hoàn tác (chống trùng).
    /// - Với mọi tem trong phiếu: xóa liên kết xuất bán (FlagSales='0', SalesDTime=null,
    ///   CustomerCode=null, SalesActivationId=null) — đúng các cột EQR set về null.
    /// - Xóa phiếu kích hoạt bán hàng (tương ứng EQR xóa bản ghi Inv_VerifiedIDInOut).
    /// </summary>
    public async Task<SalesRevertResult> RevertSalesActivationAsync(int id, string? reason, string revertedBy)
    {
        var sa = await db.SalesActivations.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (sa == null) return new SalesRevertResult(false, "Không tìm thấy phiếu kích hoạt bán hàng.", 0, "", 0);

        var codes = sa.Lines.Select(l => l.QrId).Distinct().ToList();
        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();

        // gỡ liên kết xuất bán trên từng tem (đúng các cột EQR set null + FlagSales='0')
        foreach (var s in stamps)
        {
            s.FlagSales = false;
            s.SalesDTime = null;
            s.CustomerCode = null;
            s.SalesActivationId = null;
        }

        var saNo = sa.SaNo;
        db.SalesActivations.Remove(sa);
        await db.SaveChangesAsync();

        return new SalesRevertResult(true,
            $"Đã hoàn tác kích hoạt bán hàng {saNo}, giải phóng {stamps.Count} tem về trạng thái chưa bán.",
            0, saNo, stamps.Count);
    }

    // ── GOM TEM VÀO BLOCK (Map_Block) ───────────────────────────────
    public Task<List<BlockType>> BlockTypesAsync() =>
        db.BlockTypes.OrderBy(x => x.Code).ToListAsync();

    public Task<List<Block>> BlocksAsync() =>
        db.Blocks.Include(x => x.Product).Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Block?> GetBlockAsync(int id) =>
        db.Blocks.Include(x => x.Product).Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Gom 1 tập tem cùng (lô/ca/loại block) thành 1 Block logic. Mô phỏng
    /// nghiệp vụ WAS_Map_Block_Add_New20220701 → Map_Block_AddX_New20230213
    /// của EQR (InvGen.cs):
    /// - BlockType phải tồn tại trong Mst_BlockType (Mst_BlockType_CheckDB).
    /// - Phải có ít nhất 1 tem; mọi tem phải tồn tại trong hệ thống.
    /// - Tem đã thuộc block khác → từ chối (1 tem chỉ thuộc 1 block).
    /// - Qty = BlockSize của loại block; QtyVerified = số tem thực tế đã gom.
    /// </summary>
    public async Task<BlockResult> CreateBlockAsync(string blockNo, int productId, string blockType,
        string? blockLocalId, string? shiftCode, string? lotCode, IEnumerable<string> qrIds, string? remark, string createdBy)
    {
        blockType = (blockType ?? "").Trim();
        if (blockType.Length == 0)
            return new BlockResult(false, "Cần chọn loại Block.", 0, "", 0);

        var bt = await db.BlockTypes.FirstOrDefaultAsync(x => x.Code == blockType);
        if (bt == null)
            return new BlockResult(false, $"Loại Block '{blockType}' không tồn tại trong danh mục.", 0, "", 0);
        if (!bt.IsActive)
            return new BlockResult(false, $"Loại Block '{blockType}' đã ngừng dùng.", 0, "", 0);

        var codes = (qrIds ?? []).Select(c => (c ?? "").Trim().ToUpperInvariant())
            .Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new BlockResult(false, "Chưa nhập mã tem nào.", 0, "", 0);

        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var missing = codes.Except(stamps.Select(s => s.QrId)).ToList();
        if (missing.Count > 0)
            return new BlockResult(false, $"Không tìm thấy {missing.Count} mã tem: {string.Join(", ", missing.Take(10))}", 0, "", 0);

        // tem đã thuộc block khác
        var used = await db.BlockLines.IgnoreQueryFilters()
            .Where(l => codes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        if (used.Count > 0)
            return new BlockResult(false, $"{used.Count} tem đã thuộc block khác: {string.Join(", ", used.Take(10))}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(blockNo)) blockNo = $"BLK{DateTime.Now:yyMMddHHmmss}";
        blockNo = blockNo.Trim();
        if (await db.Blocks.AnyAsync(x => x.BlockNo == blockNo))
            return new BlockResult(false, $"Mã block {blockNo} đã tồn tại.", 0, "", 0);

        var block = new Block
        {
            BlockNo = blockNo,
            ProductId = productId > 0 ? productId : stamps[0].ProductId,
            BlockType = blockType,
            BlockLocalID = string.IsNullOrWhiteSpace(blockLocalId) ? null : blockLocalId.Trim(),
            ShiftCode = string.IsNullOrWhiteSpace(shiftCode) ? null : shiftCode.Trim(),
            LotCode = string.IsNullOrWhiteSpace(lotCode) ? null : lotCode.Trim(),
            Qty = bt.BlockSize,
            QtyVerified = stamps.Count,
            Remark = remark,
            CreatedBy = createdBy
        };
        foreach (var s in stamps)
            block.Lines.Add(new BlockLine { QrId = s.QrId, AddedAt = DateTime.Now });
        db.Blocks.Add(block);
        await db.SaveChangesAsync();

        return new BlockResult(true, $"Đã gom {stamps.Count} tem vào block {block.BlockNo}.", block.Id, block.BlockNo, stamps.Count);
    }

    // ── TEM TRUNG TÍNH / NGHI VẤN (Inv_InventoryNeutralID) ──────────
    public Task<List<NeutralStamp>> NeutralStampsAsync(bool? flagNeutral)
    {
        var q = db.NeutralStamps.AsQueryable();
        if (flagNeutral.HasValue) q = q.Where(x => x.FlagNeutral == flagNeutral.Value);
        return q.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
    }

    public Task<NeutralStamp?> GetNeutralStampAsync(int id) =>
        db.NeutralStamps.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Rà soát tem nghi vấn. Mô phỏng nghiệp vụ
    /// WAS_Inv_InventoryNeutralID_InsertSuspectID của EQR (zTemp.cs):
    /// - Tem bị XUẤT KHO NHIỀU LẦN (xuất hiện ở > 1 phiếu xuất khác nhau) ⇒ nghi vấn.
    /// - Bỏ qua tem đã có bản ghi trung tính (chống trùng).
    /// - Ghi vào bảng trung tính với FlagNeutral = false (nghi vấn).
    /// </summary>
    public async Task<NeutralResult> DetectSuspectStampsAsync(string createdBy)
    {
        // đếm số lần mỗi tem xuất hiện trong các dòng phiếu xuất
        var outCounts = await db.ShipmentLines.IgnoreQueryFilters()
            .GroupBy(l => l.QrId)
            .Select(g => new { QrId = g.Key, Count = g.Count() })
            .Where(x => x.Count > 1)
            .ToListAsync();

        if (outCounts.Count == 0)
            return new NeutralResult(true, "Không phát hiện tem nào bị xuất kho nhiều lần.", 0, 0);

        var suspectCodes = outCounts.Select(x => x.QrId).ToList();
        var existing = await db.NeutralStamps.IgnoreQueryFilters()
            .Where(x => suspectCodes.Contains(x.QrId)).Select(x => x.QrId).ToListAsync();

        var toInsert = outCounts.Where(x => !existing.Contains(x.QrId)).ToList();
        var now = DateTime.Now;
        foreach (var x in toInsert)
            db.NeutralStamps.Add(new NeutralStamp
            {
                QrId = x.QrId, FlagNeutral = false, OutCount = x.Count,
                Note = $"Tem bị xuất kho {x.Count} lần — nghi vấn trùng/thất lạc.",
                CreatedBy = createdBy, CreatedAt = now
            });
        if (toInsert.Count > 0) await db.SaveChangesAsync();

        return new NeutralResult(true,
            $"Phát hiện {outCounts.Count} tem nghi vấn, thêm mới {toInsert.Count} bản ghi trung tính.",
            outCounts.Count, toInsert.Count);
    }

    /// <summary>
    /// Xác nhận 1 tem là trung tính (đã rà soát xong) — đặt FlagNeutral = true.
    /// </summary>
    public async Task<NeutralResult> ResolveNeutralStampAsync(int id, string? note, string resolvedBy)
    {
        var n = await db.NeutralStamps.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return new NeutralResult(false, "Không tìm thấy bản ghi trung tính.", 0, 0);
        if (n.FlagNeutral) return new NeutralResult(false, $"Tem {n.QrId} đã được xác nhận trung tính trước đó.", 0, 0);

        n.FlagNeutral = true;
        n.Note = string.IsNullOrWhiteSpace(note) ? n.Note : note.Trim();
        n.ResolvedAt = DateTime.Now;
        n.ResolvedBy = resolvedBy;
        await db.SaveChangesAsync();
        return new NeutralResult(true, $"Đã xác nhận tem {n.QrId} là trung tính.", 0, 0);
    }

    // ── TEM BÍ MẬT / SERIAL ẨN (Inv_InventorySecret) ────────────────
    public Task<List<InventorySecret>> InventorySecretsAsync(string? q, bool? flagUsed)
    {
        var query = db.InventorySecrets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            query = query.Where(x => x.SerialNo.Contains(k) || (x.QrSerialNo != null && x.QrSerialNo.Contains(k)));
        }
        if (flagUsed.HasValue) query = query.Where(x => x.FlagUsed == flagUsed.Value);
        return query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
    }

    public Task<InventorySecret?> GetInventorySecretAsync(int id) =>
        db.InventorySecrets.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo 1 serial bí mật (Inv_InventorySecret). Ràng buộc EQR: SerialNo bắt buộc + duy nhất.
    /// </summary>
    public async Task<SecretResult> CreateInventorySecretAsync(string serialNo, string? qrSerialNo, string? mst,
        string? genTimesNo, string? secretNo, string? remark, string createdBy)
    {
        serialNo = (serialNo ?? "").Trim();
        if (serialNo.Length == 0) return new SecretResult(false, "SerialNo không được để trống.", 0, 0);
        if (await db.InventorySecrets.AnyAsync(x => x.SerialNo == serialNo))
            return new SecretResult(false, $"SerialNo {serialNo} đã tồn tại.", 0, 0);

        var s = new InventorySecret
        {
            SerialNo = serialNo,
            QrSerialNo = string.IsNullOrWhiteSpace(qrSerialNo) ? null : qrSerialNo.Trim(),
            Mst = string.IsNullOrWhiteSpace(mst) ? null : mst.Trim(),
            GenTimesNo = string.IsNullOrWhiteSpace(genTimesNo) ? null : genTimesNo.Trim(),
            SecretNo = string.IsNullOrWhiteSpace(secretNo) ? null : secretNo.Trim(),
            FlagMap = !string.IsNullOrWhiteSpace(qrSerialNo),
            FlagUsed = false,
            Remark = remark,
            CreatedBy = createdBy
        };
        db.InventorySecrets.Add(s);
        await db.SaveChangesAsync();
        return new SecretResult(true, $"Đã tạo serial bí mật {s.SerialNo}.", s.Id, 0);
    }

    /// <summary>
    /// Đánh dấu 1 dãy serial bí mật là ĐÃ DÙNG. Mô phỏng nghiệp vụ
    /// WAS_Inv_InventorySecret_UpdateFlagUsed của EQR (License.cs):
    /// - Serial phải TỒN TẠI (InvalidSerial).
    /// - Serial chưa được dùng (FlagUsed='0') mới đánh dấu được (InvalidFlagUsed).
    /// - Serial phải thuộc đúng MST đang thao tác (InvalidMST).
    /// - Cộng dồn số lượng đã dùng (TotalQtyUsed) để đối soát quota.
    /// </summary>
    public async Task<SecretResult> MarkSecretsUsedAsync(IEnumerable<string> serialNos, string? mst, string usedBy)
    {
        var codes = (serialNos ?? []).Select(c => (c ?? "").Trim()).Where(c => c.Length > 0).Distinct().ToList();
        if (codes.Count == 0) return new SecretResult(false, "Chưa nhập serial nào.", 0, 0);

        var secrets = await db.InventorySecrets.Where(x => codes.Contains(x.SerialNo)).ToListAsync();
        var missing = codes.Except(secrets.Select(s => s.SerialNo)).ToList();
        if (missing.Count > 0)
            return new SecretResult(false, $"Không tìm thấy {missing.Count} serial: {string.Join(", ", missing.Take(10))}", 0, 0);

        var used = secrets.Where(s => s.FlagUsed).Select(s => s.SerialNo).ToList();
        if (used.Count > 0)
            return new SecretResult(false, $"{used.Count} serial đã được dùng trước đó: {string.Join(", ", used.Take(10))}", 0, 0);

        if (!string.IsNullOrWhiteSpace(mst))
        {
            var wrongMst = secrets.Where(s => !string.IsNullOrWhiteSpace(s.Mst) && s.Mst != mst.Trim())
                .Select(s => s.SerialNo).ToList();
            if (wrongMst.Count > 0)
                return new SecretResult(false, $"{wrongMst.Count} serial không thuộc MST {mst}: {string.Join(", ", wrongMst.Take(10))}", 0, 0);
        }

        var now = DateTime.Now;
        foreach (var s in secrets)
        {
            s.FlagUsed = true;
            s.UsedAt = now;
            s.UsedBy = usedBy;
        }
        await db.SaveChangesAsync();

        return new SecretResult(true, $"Đã đánh dấu {secrets.Count} serial bí mật là đã dùng.", 0, secrets.Count);
    }

    // ── YÊU CẦU XUẤT KHO (InvF_ReqInvOut) ───────────────────────────
    public Task<List<ReqInvOut>> ReqInvOutsAsync() =>
        db.ReqInvOuts.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<ReqInvOut?> GetReqInvOutAsync(int id) =>
        db.ReqInvOuts.Include(x => x.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo/cập nhật 1 phiếu yêu cầu xuất kho. Mô phỏng nghiệp vụ
    /// WAS_InvF_ReqInvOut_Save của EQR (InventoryForm.cs):
    /// - ReqInvOutNo bắt buộc (để trống ⇒ tự sinh).
    /// - RefNo (số yêu cầu) bắt buộc + duy nhất trong tenant.
    /// - Phải có ít nhất 1 dòng mặt hàng SL > 0; mọi mặt hàng phải tồn tại.
    /// - Phiếu mới ở trạng thái PENDING; nếu mã đã tồn tại thì chỉ sửa được khi đang PENDING.
    /// </summary>
    public async Task<ReqInvOutResult> CreateReqInvOutAsync(ReqInvOut header,
        IEnumerable<(int ProductId, int Qty, string? UnitCode, bool FlagDiscount, string? Remark)> lines, string createdBy)
    {
        var rows = (lines ?? []).Where(l => l.ProductId > 0 && l.Qty > 0).ToList();
        if (rows.Count == 0) return new ReqInvOutResult(false, "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0.", 0, "", "");

        header.RefNo = (header.RefNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(header.RefNo))
            return new ReqInvOutResult(false, "Số yêu cầu (RefNo) không được để trống.", 0, "", "");

        var productIds = rows.Select(r => r.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
        var bad = productIds.Except(products.Select(p => p.Id)).ToList();
        if (bad.Count > 0) return new ReqInvOutResult(false, $"Mặt hàng không tồn tại: {string.Join(", ", bad)}", 0, "", "");
        var codeById = products.ToDictionary(p => p.Id, p => p.Code);

        if (string.IsNullOrWhiteSpace(header.ReqInvOutNo)) header.ReqInvOutNo = $"YCXK{DateTime.Now:yyMMddHHmmss}";
        header.ReqInvOutNo = header.ReqInvOutNo.Trim();

        var existing = await db.ReqInvOuts.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.ReqInvOutNo == header.ReqInvOutNo);
        if (existing != null)
        {
            // sửa: chỉ cho phép khi đang PENDING (đúng ràng buộc InvF_ReqInvOut_Save_InvalidReqStatus)
            if (existing.ReqStatus != "PENDING")
                return new ReqInvOutResult(false, $"Phiếu {existing.ReqInvOutNo} đang ở trạng thái {existing.ReqStatus}, không thể sửa.", existing.Id, existing.ReqInvOutNo, existing.ReqStatus);
            if (await db.ReqInvOuts.AnyAsync(x => x.RefNo == header.RefNo && x.Id != existing.Id))
                return new ReqInvOutResult(false, $"Số yêu cầu {header.RefNo} đã được dùng ở phiếu khác.", existing.Id, existing.ReqInvOutNo, existing.ReqStatus);

            db.ReqInvOutDtls.RemoveRange(existing.Lines);
            existing.Lines.Clear();
            existing.RefNo = header.RefNo;
            existing.InvCode = header.InvCode;
            existing.InvOutType = header.InvOutType;
            existing.InvOutDate = header.InvOutDate == default ? DateTime.Today : header.InvOutDate;
            existing.TransportType = header.TransportType;
            existing.VehicleNumber = header.VehicleNumber;
            existing.CustomerCodeSys = header.CustomerCodeSys;
            existing.ReceiveAddress = header.ReceiveAddress;
            existing.QRCodeOS = header.QRCodeOS;
            existing.Remark = header.Remark;
            foreach (var r in rows)
                existing.Lines.Add(new ReqInvOutDtl
                {
                    ProductId = r.ProductId, ProductCode = codeById[r.ProductId],
                    Qty = r.Qty, UnitCode = r.UnitCode, FlagDiscount = r.FlagDiscount, Remark = r.Remark
                });
            await db.SaveChangesAsync();
            return new ReqInvOutResult(true, $"Đã cập nhật yêu cầu xuất kho {existing.ReqInvOutNo} ({rows.Count} dòng).", existing.Id, existing.ReqInvOutNo, existing.ReqStatus);
        }

        if (await db.ReqInvOuts.AnyAsync(x => x.RefNo == header.RefNo))
            return new ReqInvOutResult(false, $"Số yêu cầu {header.RefNo} đã tồn tại.", 0, "", "");

        header.ReqStatus = "PENDING";
        header.CreatedBy = createdBy;
        if (header.InvOutDate == default) header.InvOutDate = DateTime.Today;
        foreach (var r in rows)
            header.Lines.Add(new ReqInvOutDtl
            {
                ProductId = r.ProductId, ProductCode = codeById[r.ProductId],
                Qty = r.Qty, UnitCode = r.UnitCode, FlagDiscount = r.FlagDiscount, Remark = r.Remark
            });
        db.ReqInvOuts.Add(header);
        await db.SaveChangesAsync();
        return new ReqInvOutResult(true, $"Đã tạo yêu cầu xuất kho {header.ReqInvOutNo} ({rows.Count} dòng, {rows.Sum(r => r.Qty)} SP).", header.Id, header.ReqInvOutNo, header.ReqStatus);
    }

    /// <summary>
    /// Duyệt phiếu yêu cầu xuất kho. Mô phỏng nghiệp vụ
    /// WAS_InvF_ReqInvOut_Approve của EQR:
    /// - Chỉ phiếu PENDING mới duyệt được.
    /// - Gắn IVerifiedIDInOutNo (phiếu xuất theo tem); số này không được trùng phiếu khác.
    /// - Duyệt xong: ReqStatus = APPROVE + ghi mốc/người duyệt.
    /// </summary>
    public async Task<ReqInvOutResult> ApproveReqInvOutAsync(int id, string? iVerifiedIDInOutNo, string approvedBy)
    {
        var r = await db.ReqInvOuts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return new ReqInvOutResult(false, "Không tìm thấy yêu cầu xuất kho.", 0, "", "");
        if (r.ReqStatus != "PENDING")
            return new ReqInvOutResult(false, $"Phiếu {r.ReqInvOutNo} đang ở trạng thái {r.ReqStatus}, không thể duyệt.", r.Id, r.ReqInvOutNo, r.ReqStatus);

        var ivNo = string.IsNullOrWhiteSpace(iVerifiedIDInOutNo) ? null : iVerifiedIDInOutNo.Trim();
        if (ivNo != null && await db.ReqInvOuts.AnyAsync(x => x.IVerifiedIDInOutNo == ivNo && x.Id != r.Id))
            return new ReqInvOutResult(false, $"Phiếu xuất theo tem {ivNo} đã gắn với yêu cầu khác.", r.Id, r.ReqInvOutNo, r.ReqStatus);

        r.ReqStatus = "APPROVE";
        r.IVerifiedIDInOutNo = ivNo;
        r.ApprovedAt = DateTime.Now;
        r.ApprovedBy = approvedBy;
        await db.SaveChangesAsync();
        return new ReqInvOutResult(true, $"Đã duyệt yêu cầu xuất kho {r.ReqInvOutNo}.", r.Id, r.ReqInvOutNo, r.ReqStatus);
    }

    /// <summary>
    /// Bỏ duyệt phiếu yêu cầu xuất kho (đưa về PENDING). Mô phỏng nhánh
    /// FlagIsUnApprove của WAS_InvF_ReqInvOut_Approve: chỉ phiếu APPROVE mới bỏ duyệt được;
    /// xóa liên kết IVerifiedIDInOutNo + mốc duyệt.
    /// </summary>
    public async Task<ReqInvOutResult> UnApproveReqInvOutAsync(int id, string unApprovedBy)
    {
        var r = await db.ReqInvOuts.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return new ReqInvOutResult(false, "Không tìm thấy yêu cầu xuất kho.", 0, "", "");
        if (r.ReqStatus != "APPROVE")
            return new ReqInvOutResult(false, $"Phiếu {r.ReqInvOutNo} đang ở trạng thái {r.ReqStatus}, không thể bỏ duyệt.", r.Id, r.ReqInvOutNo, r.ReqStatus);

        r.ReqStatus = "PENDING";
        r.IVerifiedIDInOutNo = null;
        r.ApprovedAt = null;
        r.ApprovedBy = null;
        await db.SaveChangesAsync();
        return new ReqInvOutResult(true, $"Đã bỏ duyệt yêu cầu xuất kho {r.ReqInvOutNo}.", r.Id, r.ReqInvOutNo, r.ReqStatus);
    }

    /// <summary>
    /// Xóa phiếu yêu cầu xuất kho. Mô phỏng nhánh FlagIsDelete của
    /// WAS_InvF_ReqInvOut_Save: chỉ xóa được phiếu PENDING.
    /// </summary>
    public async Task<ReqInvOutResult> DeleteReqInvOutAsync(int id)
    {
        var r = await db.ReqInvOuts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return new ReqInvOutResult(false, "Không tìm thấy yêu cầu xuất kho.", 0, "", "");
        if (r.ReqStatus != "PENDING")
            return new ReqInvOutResult(false, $"Phiếu {r.ReqInvOutNo} đang ở trạng thái {r.ReqStatus}, không thể xóa.", r.Id, r.ReqInvOutNo, r.ReqStatus);

        db.ReqInvOuts.Remove(r);
        await db.SaveChangesAsync();
        return new ReqInvOutResult(true, $"Đã xóa yêu cầu xuất kho {r.ReqInvOutNo}.", 0, r.ReqInvOutNo, "");
    }

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
        if (s.Status == StampStatus.Broken) warnings.Add("Tem đã được ghi nhận rách/vỡ (NG) — không còn giá trị sử dụng.");
        if (s.ScanCount > 20) warnings.Add($"Tem này đã được quét {s.ScanCount} lần — bất thường, cảnh giác hàng giả sao chép mã.");
        if (s.ActivatedAt != null) warnings.Add($"Tem đã được kích hoạt bảo hành ngày {s.ActivatedAt:dd/MM/yyyy}.");

        var genuine = s.Status != StampStatus.Void && s.Status != StampStatus.Broken;
        await db.SaveChangesAsync();

        return new VerifyResult(true, genuine,
            genuine ? "SẢN PHẨM CHÍNH HÃNG" : (s.Status == StampStatus.Broken ? "TEM ĐÃ RÁCH/VỠ" : "TEM ĐÃ VÔ HIỆU"),
            genuine ? "Tem hợp lệ do nhà sản xuất phát hành." : (s.Status == StampStatus.Broken ? "Tem này đã được ghi nhận rách/vỡ." : "Tem này đã bị thu hồi."),
            s, s.Product, s.Batch, warnings);
    }

    public async Task<(bool ok, string msg)> ActivateAsync(string qrId, string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return (false, "Cần số điện thoại.");
        var s = await db.Stamps.IgnoreQueryFilters().Include(x => x.Product).Include(x => x.Batch)
            .FirstOrDefaultAsync(x => x.QrId == qrId.Trim());
        if (s == null) return (false, "Mã tem không tồn tại.");
        if (s.Status == StampStatus.Void) return (false, "Tem đã vô hiệu.");
        if (s.Status == StampStatus.Broken) return (false, "Tem đã được ghi nhận rách/vỡ.");
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
        if (s.Status == StampStatus.Broken) return (false, "Tem đã được ghi nhận rách/vỡ.");
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

    // ── KÍCH HOẠT BẢO HÀNH BẰNG PIN (WarrantyDateStartFromPIN_Activate) ─
    public Task<List<WarrantyActivation>> WarrantyActivationsAsync(string? qrId)
    {
        var query = db.WarrantyActivations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(qrId)) query = query.Where(x => x.QrId == qrId.Trim());
        return query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
    }

    public Task<WarrantyActivation?> GetWarrantyActivationAsync(int id) =>
        db.WarrantyActivations.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Kích hoạt bảo hành bằng PIN. Mô phỏng nghiệp vụ
    /// WAS_WarrantyDateStartFromPIN_Activate_New20250520 của EQR (Report.cs):
    /// - Tem phải tồn tại; PIN phải khớp (so khớp PIN hoặc MD5(IDNo|PIN)).
    /// - Tem đã vô hiệu (Void)/rách-vỡ (Broken) → từ chối.
    /// - Lần kích hoạt ĐẦU TIÊN: cấp WarrantyNo mới + ghi WarrantyDateStart.
    /// - Kích hoạt LẠI: giữ nguyên WarrantyNo/WarrantyDateStart, chỉ tăng WarrantyCount
    ///   và cập nhật SĐT/IP/vị trí (đúng nhánh update thứ 2 của EQR).
    /// - Mỗi lần kích hoạt ghi 1 bản ghi lịch sử (WarrantyActivation).
    /// </summary>
    public async Task<WarrantyResult> ActivateWarrantyByPinAsync(string qrId, string pin, string? phone, string? ip,
        string? mapLatitude, string? mapLongitude)
    {
        qrId = (qrId ?? "").Trim();
        pin = (pin ?? "").Trim();
        if (qrId.Length == 0) return new WarrantyResult(false, "Cần mã tem.", 0, qrId, "", default, false, 0);
        if (pin.Length == 0) return new WarrantyResult(false, "Cần nhập mã PIN cào trên tem.", 0, qrId, "", default, false, 0);

        var s = await db.Stamps.IgnoreQueryFilters().Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.QrId == qrId);
        if (s == null) return new WarrantyResult(false, "Mã tem không tồn tại.", 0, qrId, "", default, false, 0);
        if (s.Status == StampStatus.Void) return new WarrantyResult(false, "Tem đã vô hiệu.", 0, qrId, "", default, false, 0);
        if (s.Status == StampStatus.Broken) return new WarrantyResult(false, "Tem đã được ghi nhận rách/vỡ.", 0, qrId, "", default, false, 0);

        // PIN phải khớp: so khớp trực tiếp PIN hoặc hash MD5(IDNo|PIN) — đúng EQR
        var hash = Md5Hex($"{qrId}|{pin}");
        if (!string.Equals(s.Pin, pin, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(s.Pin, hash, StringComparison.OrdinalIgnoreCase))
            return new WarrantyResult(false, "Mã PIN không đúng với tem này.", 0, qrId, "", default, false, 0);

        var now = DateTime.Now;
        var isFirst = s.ActivatedAt == null;
        var warrantyNo = isFirst ? NewWarrantyNo() : (s.WarrantyNo ?? NewWarrantyNo());
        var warrantyDateStart = isFirst ? now.Date : (s.ActivatedAt?.Date ?? now.Date);
        s.WarrantyCount += 1;

        if (isFirst)
        {
            s.ActivatedAt = now;
            s.WarrantyNo = warrantyNo;
            s.Status = StampStatus.Activated;
            s.WarrantyEnd = now.Date.AddMonths(s.Product?.WarrantyMonths ?? 12);
        }
        s.ActivatedPhone = string.IsNullOrWhiteSpace(phone) ? s.ActivatedPhone : phone.Trim();
        s.WarrantyStartIp = string.IsNullOrWhiteSpace(ip) ? s.WarrantyStartIp : ip.Trim();
        s.WarrantyLat = string.IsNullOrWhiteSpace(mapLatitude) ? s.WarrantyLat : mapLatitude.Trim();
        s.WarrantyLong = string.IsNullOrWhiteSpace(mapLongitude) ? s.WarrantyLong : mapLongitude.Trim();

        var log = new WarrantyActivation
        {
            QrId = qrId, Pin = pin, PhoneNoUser = s.ActivatedPhone, IpAddress = s.WarrantyStartIp,
            MapLatitude = s.WarrantyLat, MapLongitude = s.WarrantyLong,
            WarrantyNo = warrantyNo, WarrantyDateStart = warrantyDateStart,
            IsFirstActivate = isFirst, WarrantyCount = s.WarrantyCount, CreatedBy = "consumer"
        };
        db.WarrantyActivations.Add(log);
        await db.SaveChangesAsync();

        var msg = isFirst
            ? $"Kích hoạt bảo hành thành công — số phiếu {warrantyNo}, bắt đầu {warrantyDateStart:dd/MM/yyyy}, hết hạn {s.WarrantyEnd:dd/MM/yyyy}."
            : $"Tem đã kích hoạt trước đó (số phiếu {warrantyNo}) — đã cập nhật thông tin, lần kích hoạt thứ {s.WarrantyCount}.";
        return new WarrantyResult(true, msg, log.Id, qrId, warrantyNo, warrantyDateStart, isFirst, s.WarrantyCount);
    }

    private static string NewWarrantyNo() => $"BH{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";

    private static string Md5Hex(string input)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string NewQrId()
        => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();   // 12 ký tự hex — ngắn gọn cho QR

    // ── VÒNG ĐỜI TEM THEO KỲ THÁNG (InvIVID_LifeCircleIDNoByPeriod) ─
    public Task<List<StampLifecyclePeriod>> LifecyclePeriodsAsync() =>
        db.StampLifecyclePeriods.OrderByDescending(x => x.PeriodMonth).Take(500).ToListAsync();

    public Task<StampLifecyclePeriod?> GetLifecyclePeriodAsync(int id) =>
        db.StampLifecyclePeriods.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Chốt 1 bản ghi vòng đời tem cho 1 KỲ THÁNG. Mô phỏng nghiệp vụ
    /// WAS_InvIVID_LifeCircleIDNoByPeriod_Add của EQR (Template.cs):
    /// - Kỳ tháng chuẩn hoá về ngày đầu tháng (yyyy-MM-01); mỗi kỳ chỉ chốt 1 lần.
    /// - Đếm 4 chỉ số tem trong kỳ: đã ghép SP (MapIDDTimeUTC), đã bán (SalesDTime),
    ///   đã kích hoạt bảo hành (ActivatedAt), đã tra cứu (LastScanAt).
    /// - Delta* = chỉ số kỳ này − chỉ số kỳ trước (kỳ trước = PeriodMonth lớn nhất < kỳ này).
    /// </summary>
    public async Task<LifecycleResult> SnapshotLifecycleAsync(DateTime periodMonth, string? remark, string createdBy)
    {
        // chuẩn hoá kỳ về ngày đầu tháng
        var period = new DateTime(periodMonth.Year, periodMonth.Month, 1);
        var from = period;
        var to = period.AddMonths(1).AddTicks(-1);

        if (await db.StampLifecyclePeriods.AnyAsync(x => x.PeriodMonth == period))
            return new LifecycleResult(false, $"Kỳ tháng {period:MM/yyyy} đã được chốt trước đó.", 0, period, 0, 0, 0, 0);

        // đếm tem trong kỳ (xuyên tenant theo OrgId hiện tại — dùng query filter mặc định)
        var qtyVerified = await db.Stamps.CountAsync(x => x.BatchId != 0 && x.BoxedAt >= from && x.BoxedAt <= to);
        var qtySales = await db.Stamps.CountAsync(x => x.FlagSales && x.SalesDTime >= from && x.SalesDTime <= to);
        var qtyWarranty = await db.Stamps.CountAsync(x => x.ActivatedAt >= from && x.ActivatedAt <= to);
        var qtySearch = await db.Stamps.CountAsync(x => x.ScanCount != 0 && x.LastScanAt >= from && x.LastScanAt <= to);

        // kỳ trước = PeriodMonth lớn nhất < kỳ này (đúng EQR: select top 1 ... order by desc)
        var prev = await db.StampLifecyclePeriods
            .Where(x => x.PeriodMonth < period)
            .OrderByDescending(x => x.PeriodMonth)
            .FirstOrDefaultAsync();

        var rec = new StampLifecyclePeriod
        {
            PeriodMonth = period,
            QtyVerifiedID = qtyVerified,
            QtySales = qtySales,
            QtyWarranty = qtyWarranty,
            QtySearch = qtySearch,
            DeltaVerifiedID = qtyVerified - (prev?.QtyVerifiedID ?? 0),
            DeltaSales = qtySales - (prev?.QtySales ?? 0),
            DeltaWarranty = qtyWarranty - (prev?.QtyWarranty ?? 0),
            DeltaSearch = qtySearch - (prev?.QtySearch ?? 0),
            Remark = remark,
            CreatedBy = createdBy
        };
        db.StampLifecyclePeriods.Add(rec);
        await db.SaveChangesAsync();

        return new LifecycleResult(true,
            $"Đã chốt vòng đời tem kỳ {period:MM/yyyy}: ghép SP {qtyVerified}, bán {qtySales}, bảo hành {qtyWarranty}, tra cứu {qtySearch}.",
            rec.Id, period, qtyVerified, qtySales, qtyWarranty, qtySearch);
    }

    // ── XUẤT KHO THEO HỘP (Inv_InventoryVerifiedID_OutByBox) ────────
    public Task<List<BoxShipment>> BoxShipmentsAsync() =>
        db.BoxShipments.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<BoxShipment?> GetBoxShipmentAsync(int id) =>
        db.BoxShipments.Include(x => x.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Tạo phiếu xuất kho THEO HỘP. Mô phỏng nghiệp vụ
    /// WAS_Inv_InventoryVerifiedID_OutByBox_New20220601 của EQR (zTemp.cs):
    /// - Mỗi mã quét phải có StampType = ID (tem lẻ) hoặc BOX (mã hộp) — nếu khác ⇒ từ chối.
    /// - Tem lẻ (ID) phải tồn tại trong hệ thống (IDNoNotExistInInvGen).
    /// - Tem phải đã được ghép sản phẩm + đóng vào hộp (IDNoNotMapBox).
    /// - Hộp (BOX) phải tồn tại và có tem con.
    /// - Tem đã xuất ở phiếu khác ⇒ từ chối (không xuất trùng).
    /// - Phiếu mới ở trạng thái PENDING; mỗi tem con trong hộp thành 1 dòng chi tiết.
    /// </summary>
    public async Task<BoxShipResult> CreateBoxShipmentAsync(BoxShipment header, IEnumerable<(string ScanCode, string StampType)> scans, string createdBy)
    {
        var list = (scans ?? [])
            .Select(s => (ScanCode: (s.ScanCode ?? "").Trim().ToUpperInvariant(), StampType: (s.StampType ?? "").Trim().ToUpperInvariant()))
            .Where(s => s.ScanCode.Length > 0).ToList();
        if (list.Count == 0) return new BoxShipResult(false, "Chưa nhập mã quét nào.", 0, "", 0);

        // StampType chỉ nhận ID/BOX (StampTypeInvalid)
        var badType = list.Where(s => s.StampType != "ID" && s.StampType != "BOX").ToList();
        if (badType.Count > 0)
            return new BoxShipResult(false, $"Loại quét không hợp lệ (chỉ nhận ID/BOX): {string.Join(", ", badType.Take(10).Select(s => s.ScanCode))}", 0, "", 0);

        // tách tem lẻ (ID) và hộp (BOX)
        var idCodes = list.Where(s => s.StampType == "ID").Select(s => s.ScanCode).Distinct().ToList();
        var boxNos = list.Where(s => s.StampType == "BOX").Select(s => s.ScanCode).Distinct().ToList();

        // tem lẻ phải tồn tại (IDNoNotExistInInvGen)
        var idStamps = await db.Stamps.Where(s => idCodes.Contains(s.QrId)).ToListAsync();
        var missingId = idCodes.Except(idStamps.Select(s => s.QrId)).ToList();
        if (missingId.Count > 0)
            return new BoxShipResult(false, $"Không tìm thấy {missingId.Count} mã tem: {string.Join(", ", missingId.Take(10))}", 0, "", 0);

        // tem lẻ phải đã đóng vào hộp (IDNoNotMapBox)
        var notBoxed = idStamps.Where(s => s.BoxId == null).ToList();
        if (notBoxed.Count > 0)
            return new BoxShipResult(false, $"{notBoxed.Count} tem chưa được đóng vào hộp, không thể xuất theo hộp: {string.Join(", ", notBoxed.Take(10).Select(s => s.QrId))}", 0, "", 0);

        // hộp phải tồn tại
        var boxes = await db.Boxes.Where(b => boxNos.Contains(b.BoxNo)).ToListAsync();
        var missingBox = boxNos.Except(boxes.Select(b => b.BoxNo)).ToList();
        if (missingBox.Count > 0)
            return new BoxShipResult(false, $"Không tìm thấy {missingBox.Count} mã hộp: {string.Join(", ", missingBox.Take(10))}", 0, "", 0);

        // bung hộp ra toàn bộ tem con
        var boxIds = boxes.Select(b => b.Id).ToList();
        var boxStamps = await db.Stamps.Where(s => s.BoxId != null && boxIds.Contains(s.BoxId.Value)).ToListAsync();
        var emptyBoxes = boxes.Where(b => !boxStamps.Any(s => s.BoxId == b.Id)).ToList();
        if (emptyBoxes.Count > 0)
            return new BoxShipResult(false, $"{emptyBoxes.Count} hộp không có tem con: {string.Join(", ", emptyBoxes.Take(10).Select(b => b.BoxNo))}", 0, "", 0);

        // gộp tem lẻ + tem bung từ hộp (khử trùng theo QrId)
        var allStamps = idStamps.Concat(boxStamps)
            .GroupBy(s => s.QrId).Select(g => g.First()).ToList();

        // tem đã vô hiệu/rách-vỡ
        var bad = allStamps.Where(s => s.Status == StampStatus.Void || s.Status == StampStatus.Broken).ToList();
        if (bad.Count > 0)
            return new BoxShipResult(false, $"{bad.Count} tem đã vô hiệu/rách-vỡ, không thể xuất: {string.Join(", ", bad.Take(10).Select(s => s.QrId))}", 0, "", 0);

        // tem đã xuất ở phiếu khác (cả phiếu xuất theo tem lẫn theo hộp)
        var allCodes = allStamps.Select(s => s.QrId).ToList();
        var shippedByStamp = await db.ShipmentLines.IgnoreQueryFilters()
            .Where(l => allCodes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        var shippedByBox = await db.BoxShipmentLines.IgnoreQueryFilters()
            .Where(l => allCodes.Contains(l.QrId)).Select(l => l.QrId).ToListAsync();
        var alreadyShipped = shippedByStamp.Concat(shippedByBox).Distinct().ToList();
        if (alreadyShipped.Count > 0)
            return new BoxShipResult(false, $"{alreadyShipped.Count} tem đã được xuất trước đó: {string.Join(", ", alreadyShipped.Take(10))}", 0, "", 0);

        if (string.IsNullOrWhiteSpace(header.BsNo)) header.BsNo = $"PXKH{DateTime.Now:yyMMddHHmmss}";
        header.BsNo = header.BsNo.Trim();
        if (await db.BoxShipments.AnyAsync(x => x.BsNo == header.BsNo))
            return new BoxShipResult(false, $"Mã phiếu {header.BsNo} đã tồn tại.", 0, "", 0);

        header.Status = "PENDING";
        header.CreatedBy = createdBy;
        var now = DateTime.Now;

        // dòng chi tiết: tem lẻ ghi ScanCode = chính nó; tem bung từ hộp ghi ScanCode = mã hộp
        var boxNoByBoxId = boxes.ToDictionary(b => b.Id, b => b.BoxNo);
        foreach (var s in idStamps)
            header.Lines.Add(new BoxShipmentLine { ScanCode = s.QrId, StampType = "ID", QrId = s.QrId, ProductId = s.ProductId, ShippedAt = now });
        foreach (var s in boxStamps)
            header.Lines.Add(new BoxShipmentLine
            {
                ScanCode = s.BoxId != null && boxNoByBoxId.TryGetValue(s.BoxId.Value, out var bn) ? bn : "",
                StampType = "BOX", QrId = s.QrId, ProductId = s.ProductId, ShippedAt = now
            });

        db.BoxShipments.Add(header);
        await db.SaveChangesAsync();
        return new BoxShipResult(true, $"Đã tạo phiếu xuất theo hộp {header.BsNo} ({header.Lines.Count} tem).", header.Id, header.BsNo, header.Lines.Count);
    }

    /// <summary>
    /// Xuất phiếu xuất theo hộp (ghi nhận tem ra khỏi kho). Mô phỏng bước OutInv của EQR:
    /// - Chỉ phiếu PENDING mới được xuất.
    /// - Xuất xong: Status = SHIPPED + ghi mốc/người xuất; đánh dấu tem đã xuất (ShipmentId, ShippedAt, CustomerCode).
    /// </summary>
    public async Task<BoxShipResult> ShipBoxShipmentAsync(int id, string shippedBy)
    {
        var sh = await db.BoxShipments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (sh == null) return new BoxShipResult(false, "Không tìm thấy phiếu xuất theo hộp.", 0, "", 0);
        if (sh.Status != "PENDING") return new BoxShipResult(false, $"Phiếu {sh.BsNo} đang ở trạng thái {sh.Status}, không thể xuất.", sh.Id, sh.BsNo, 0);

        var codes = sh.Lines.Select(l => l.QrId).ToList();
        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var now = DateTime.Now;
        foreach (var s in stamps)
        {
            s.ShipmentId = sh.Id;
            s.ShippedAt = now;
            s.CustomerCode = sh.CustomerCode;
        }
        sh.Status = "SHIPPED";
        sh.ShippedAt = now;
        sh.ShippedBy = shippedBy;
        await db.SaveChangesAsync();
        return new BoxShipResult(true, $"Đã xuất phiếu {sh.BsNo} ({stamps.Count} tem).", sh.Id, sh.BsNo, stamps.Count);
    }

    /// <summary>
    /// Hủy phiếu xuất theo hộp. Mô phỏng nghiệp vụ WAS_Inv_VerifiedIDInOut_Cancel của EQR:
    /// - Phiếu phải tồn tại và chưa bị hủy (không hủy trùng).
    /// - Chỉ hủy được phiếu đã xuất (SHIPPED).
    /// - Hủy xong: Status = CANCEL + ghi mốc/người/lý do; giải phóng tem về trạng thái chưa xuất.
    /// </summary>
    public async Task<BoxShipCancelResult> CancelBoxShipmentAsync(int id, string? reason, string cancelledBy)
    {
        var sh = await db.BoxShipments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (sh == null) return new BoxShipCancelResult(false, "Không tìm thấy phiếu xuất theo hộp.", 0, "", 0);
        if (sh.Status == "CANCEL")
            return new BoxShipCancelResult(false, $"Phiếu {sh.BsNo} đã bị hủy trước đó.", sh.Id, sh.BsNo, 0);
        if (sh.Status != "SHIPPED")
            return new BoxShipCancelResult(false, $"Phiếu {sh.BsNo} đang ở trạng thái {sh.Status}, chỉ hủy được phiếu đã xuất (SHIPPED).", sh.Id, sh.BsNo, 0);

        var codes = sh.Lines.Select(l => l.QrId).ToList();
        var stamps = await db.Stamps.Where(s => codes.Contains(s.QrId)).ToListAsync();
        var now = DateTime.Now;
        foreach (var s in stamps)
        {
            s.ShipmentId = null;
            s.ShippedAt = null;
            s.CustomerCode = null;
        }
        sh.Status = "CANCEL";
        sh.CancelledAt = now;
        sh.CancelledBy = cancelledBy;
        sh.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await db.SaveChangesAsync();
        return new BoxShipCancelResult(true, $"Đã hủy phiếu {sh.BsNo}, giải phóng {stamps.Count} tem về trạng thái chưa xuất.", sh.Id, sh.BsNo, stamps.Count);
    }

    // ── NHẬP DÃY SERIAL NGƯỜI DÙNG (Inv_StampUser) ─────────────────
    public Task<List<StampUser>> StampUsersAsync() =>
        db.StampUsers.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();

    public Task<StampUser?> GetStampUserAsync(int id) =>
        db.StampUsers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Nhập 1 dãy serial người dùng (IDNo_User) và GẮN vào các tem đã sinh nhưng
    /// CHƯA có serial người dùng. Mô phỏng nghiệp vụ WAS_Inv_StampUser_Add →
    /// Inv_StampUser_AddX của EQR (DVP/ImportIDUser.cs):
    /// - IF_SUINo bắt buộc + duy nhất (chặn trùng phiếu).
    /// - Phải có ít nhất 1 serial.
    /// - Serial không được trùng với serial đã nhập trước đó (StampUserIDNo_UserExist).
    /// - Số tem còn trống (chưa gắn serial người dùng) phải >= số serial nhập (InvalidValue).
    /// - Map theo thứ tự: serial nhỏ nhất ↔ tem nhỏ nhất; đánh dấu FlagMapIDNo_User='1'.
    /// </summary>
    public async Task<StampUserResult> ImportStampUsersAsync(string suiNo, IEnumerable<(string IdNoUser, string? Remark)> serials, string createdBy)
    {
        suiNo = (suiNo ?? "").Trim();
        if (suiNo.Length == 0) return new StampUserResult(false, "IF_SUINo (mã phiếu nhập) không được để trống.", 0, "", 0, 0);
        if (await db.StampUsers.AnyAsync(x => x.SuiNo == suiNo))
            return new StampUserResult(false, $"Phiếu nhập {suiNo} đã tồn tại.", 0, suiNo, 0, 0);

        // Chuẩn hoá + loại trùng trong danh sách đầu vào
        var input = (serials ?? [])
            .Select(s => (IdNoUser: (s.IdNoUser ?? "").Trim(), Remark: string.IsNullOrWhiteSpace(s.Remark) ? null : s.Remark!.Trim()))
            .Where(s => s.IdNoUser.Length > 0)
            .GroupBy(s => s.IdNoUser).Select(g => g.First())
            .OrderBy(s => s.IdNoUser, StringComparer.Ordinal)
            .ToList();
        if (input.Count == 0) return new StampUserResult(false, "Cần ít nhất 1 serial người dùng.", 0, suiNo, 0, 0);

        // Serial không được trùng với serial đã nhập trước đó
        var codes = input.Select(s => s.IdNoUser).ToList();
        var existed = await db.StampUserLines.Where(l => codes.Contains(l.IdNoUser)).Select(l => l.IdNoUser).ToListAsync();
        if (existed.Count > 0)
            return new StampUserResult(false, $"{existed.Count} serial đã nhập trước đó: {string.Join(", ", existed.Take(10))}", 0, suiNo, 0, 0);

        // Tem còn trống: tem chưa gắn serial người dùng (QrId chưa xuất hiện trong StampUserLine)
        var mappedQrIds = await db.StampUserLines.Where(l => l.QrId != null).Select(l => l.QrId!).ToListAsync();
        var available = await db.Stamps.Where(s => !mappedQrIds.Contains(s.QrId))
            .OrderBy(s => s.QrId).Take(input.Count).ToListAsync();
        if (available.Count < input.Count)
            return new StampUserResult(false, $"Số tem còn trống ({available.Count}) ít hơn số serial nhập vào ({input.Count}).", 0, suiNo, 0, 0);

        var su = new StampUser { SuiNo = suiNo, ImportDTime = DateTime.Now, CreatedBy = createdBy };
        for (int i = 0; i < input.Count; i++)
            su.Lines.Add(new StampUserLine { IdNoUser = input[i].IdNoUser, QrId = available[i].QrId, Remark = input[i].Remark });
        su.Quantity = su.Lines.Count;
        db.StampUsers.Add(su);
        await db.SaveChangesAsync();

        return new StampUserResult(true, $"Đã nhập {su.Lines.Count} serial người dùng và gắn vào {available.Count} tem.", su.Id, su.SuiNo, su.Lines.Count, available.Count);
    }

    // ── KÍCH HOẠT TEM TRẮNG BỞI TRẠM BÁN HÀNG (Inv_InventoryVerifiedID_ActivateByTBH) ──
    public Task<List<TbhActivation>> TbhActivationsAsync() =>
        db.TbhActivations.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();

    public Task<TbhActivation?> GetTbhActivationAsync(int id) =>
        db.TbhActivations.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Kích hoạt 1 tem TRẮNG bởi Trạm bán hàng (TBH). Mô phỏng nghiệp vụ
    /// WAS_Inv_InventoryVerifiedID_ActivateByTBH_New20210922 của EQR (zTemp.cs):
    /// - IDNo bắt buộc; chuẩn hoá nhiễu (bỏ space/'-'/tab/xuống dòng) đúng EQR.
    /// - Tem phải tồn tại trong hệ thống.
    /// - Tem đã vô hiệu (Void)/rách-vỡ (Broken) → từ chối.
    /// - Tem đã kích hoạt bán hàng trước đó → từ chối (không kích hoạt trùng).
    /// - Sinh 1 phiếu xuất nội bộ (RefType=INVOUT, tiền tố PXKHTT) + đánh dấu tem
    ///   FlagSales='1' + SalesDTime + CustomerCode (đúng các cột EQR set).
    /// </summary>
    public async Task<TbhResult> ActivateByTbhAsync(string qrId, string? pin, int productId, string? customerCode,
        string? areaCode, string? proofImagePath, string? proofImagePathName, string? remark, string createdBy)
    {
        // 20210222: Xử lý nhiễu cho mã tem — bỏ space, '-', tab, xuống dòng (đúng EQR)
        var code = new string((qrId ?? "").Where(c => c != ' ' && c != '-' && c != '\r' && c != '\t' && c != '\n').ToArray())
            .Trim().ToUpperInvariant();
        if (code.Length == 0) return new TbhResult(false, "Cần nhập mã tem (IDNo).", 0, "", "");

        var stamp = await db.Stamps.FirstOrDefaultAsync(s => s.QrId == code);
        if (stamp == null) return new TbhResult(false, $"Không tìm thấy tem {code} trong hệ thống.", 0, "", code);

        if (stamp.Status == StampStatus.Void || stamp.Status == StampStatus.Broken)
            return new TbhResult(false, $"Tem {code} đã vô hiệu/rách-vỡ, không thể kích hoạt.", 0, "", code);

        if (stamp.FlagSales)
            return new TbhResult(false, $"Tem {code} đã được kích hoạt bán trước đó.", 0, "", code);

        var now = DateTime.Now;
        var tbhNo = $"PXKHTT{now:yyMMddHHmmss}";
        if (await db.TbhActivations.AnyAsync(x => x.TbhNo == tbhNo))
            tbhNo = $"PXKHTT{now:yyMMddHHmmssfff}";

        var tbh = new TbhActivation
        {
            TbhNo = tbhNo,
            RefNoSys = $"PXKHTT.{now:yyyyMMdd.HHmmss}.0",   // đúng định dạng RefNoSys của EQR
            RefType = "INVOUT",
            QrId = stamp.QrId,
            Pin = string.IsNullOrWhiteSpace(pin) ? null : pin!.Trim(),
            ProductId = productId > 0 ? productId : stamp.ProductId,
            CustomerCode = string.IsNullOrWhiteSpace(customerCode) ? null : customerCode!.Trim(),
            AreaCode = string.IsNullOrWhiteSpace(areaCode) ? null : areaCode!.Trim(),
            ProofImagePath = string.IsNullOrWhiteSpace(proofImagePath) ? null : proofImagePath!.Trim(),
            ProofImagePathName = string.IsNullOrWhiteSpace(proofImagePathName) ? null : proofImagePathName!.Trim(),
            Remark = remark,
            CreatedBy = createdBy
        };

        // đánh dấu tem đã xuất bán (đúng các cột EQR set khi kích hoạt TBH)
        stamp.FlagSales = true;
        stamp.SalesDTime = now;
        stamp.CustomerCode = tbh.CustomerCode;

        db.TbhActivations.Add(tbh);
        await db.SaveChangesAsync();

        return new TbhResult(true, $"Đã kích hoạt tem trắng {stamp.QrId} bởi Trạm bán hàng ({tbh.TbhNo}).", tbh.Id, tbh.TbhNo, stamp.QrId);
    }

    // ── CẬP NHẬT NGÀY SẢN XUẤT CHO TEM (Inv_InventoryVerifiedID_UpdPrdDTime) ──
    public Task<List<StampProductionDateLog>> ProductionDateLogsAsync() =>
        db.StampProductionDateLogs.Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();

    public Task<StampProductionDateLog?> GetProductionDateLogAsync(int id) =>
        db.StampProductionDateLogs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>
    /// Cập nhật NGÀY SẢN XUẤT (ProductionDTimeUTC) cho 1 danh sách tem. Mô phỏng
    /// nghiệp vụ WAS_Inv_InventoryVerifiedID_UpdPrdDTime → Inv_InventoryVerifiedID_UpdPrdDTimeX
    /// của EQR (zTemp.cs):
    /// - Danh sách tem không được rỗng.
    /// - Mọi IDNo phải tồn tại trong kho tem (InvalidIDNo).
    /// - Ngày SX rỗng ⇒ lấy thời điểm hiện tại.
    /// - Cập nhật ProductionDTimeUTC + ghi log (LogLUDTimeUTC/LogLUBy).
    /// - Ghi 1 bản ghi lịch sử (tương ứng Inv_InventoryVerifiedIDHist) để truy vết.
    /// </summary>
    public async Task<PrdDTimeResult> UpdateProductionDateAsync(DateTime? productionDTime, IEnumerable<string> qrIds,
        string? remark, string createdBy)
    {
        // Ngày SX rỗng ⇒ lấy thời điểm hiện tại (đúng EQR).
        var prdDTime = productionDTime ?? DateTime.Now;

        // Chuẩn hoá danh sách tem: bỏ rỗng, chống trùng, giữ thứ tự.
        var list = (qrIds ?? []).Select(x => (x ?? "").Trim())
            .Where(x => x.Length > 0).Distinct().ToList();
        if (list.Count == 0)
            return new PrdDTimeResult(false, "Phải có ít nhất 1 tem (IDNo) để cập nhật ngày sản xuất.", 0, "", 0, prdDTime);

        // Mọi IDNo phải tồn tại trong kho tem (InvalidIDNo).
        var stamps = await db.Stamps.Where(s => list.Contains(s.QrId)).ToListAsync();
        var found = stamps.Select(s => s.QrId).ToHashSet();
        var missing = list.Where(x => !found.Contains(x)).ToList();
        if (missing.Count > 0)
            return new PrdDTimeResult(false, $"Tem không tồn tại trong kho tem: {string.Join(", ", missing)}.", 0, "", 0, prdDTime);

        var now = DateTime.Now;
        var logNo = $"PRDD.{now:yyyyMMdd.HHmmss.ffff}";
        var log = new StampProductionDateLog
        {
            LogNo = logNo,
            ProductionDTime = prdDTime,
            Quantity = stamps.Count,
            Remark = remark,
            CreatedBy = createdBy
        };

        // Cập nhật ngày SX + ghi log trên từng tem; lưu lại ngày cũ vào lịch sử.
        foreach (var s in stamps)
        {
            log.Lines.Add(new StampProductionDateLogLine { QrId = s.QrId, OldProductionDTime = s.ProductionDTime });
            s.ProductionDTime = prdDTime;
        }

        db.StampProductionDateLogs.Add(log);
        await db.SaveChangesAsync();

        return new PrdDTimeResult(true,
            $"Đã cập nhật ngày sản xuất {prdDTime:dd/MM/yyyy HH:mm} cho {stamps.Count} tem.",
            log.Id, log.LogNo, stamps.Count, prdDTime);
    }
}
