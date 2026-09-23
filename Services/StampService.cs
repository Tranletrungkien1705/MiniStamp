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

/// <summary>Kết quả ghi nhận phiếu tem rách/vỡ (nghiệp vụ InvF_BrokenStamp).</summary>
public record BrokenResult(bool Ok, string Message, int BrokenStampId, string BsNo, int Count);

/// <summary>Kết quả tạo/duyệt phiếu nhập kho thành phẩm (nghiệp vụ InvF_InventoryInFG).</summary>
public record InvInResult(bool Ok, string Message, int Id, string InvInNo, int TotalQty);

/// <summary>Kết quả tạo/xuất phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut).</summary>
public record ShipResult(bool Ok, string Message, int Id, string ShipmentNo, int TotalQty);

/// <summary>Kết quả kích hoạt thông tin sản xuất (nghiệp vụ InvF_ProductionActive).</summary>
public record PaResult(bool Ok, string Message, int Id, string PaNo, DateTime ExpiryDate);

/// <summary>Kết quả thao tác danh mục nguồn gốc (nghiệp vụ Mst_NguonGoc).</summary>
public record OriginResult(bool Ok, string Message, int Id, string Code);

/// <summary>Kết quả lưu sự kiện truy xuất GS1 (nghiệp vụ Event_Event_Save).</summary>
public record TraceEventResult(bool Ok, string Message, int Id, string EventNo, string Action);

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
    // phiếu tem rách/vỡ (InvF_BrokenStamp)
    Task<List<BrokenStamp>> BrokenStampsAsync();
    Task<BrokenStamp?> GetBrokenStampAsync(int id);
    Task<BrokenResult> ReportBrokenAsync(string bsNo, int productId, IEnumerable<string> qrIds, string? note, string createdBy);
    // phiếu nhập kho thành phẩm (InvF_InventoryInFG)
    Task<List<InventoryInFG>> InventoryInFGsAsync();
    Task<InventoryInFG?> GetInventoryInFGAsync(int id);
    Task<InvInResult> CreateInventoryInFGAsync(string invInNo, string? remark, IEnumerable<(int ProductId, int Qty, DateTime ProductionDate)> lines, string createdBy);
    Task<InvInResult> ApproveInventoryInFGAsync(int id, string approvedBy);
    // phiếu xuất kho theo tem (Inv_VerifiedIDInOut / OutGenInAndOut)
    Task<List<Shipment>> ShipmentsAsync();
    Task<Shipment?> GetShipmentAsync(int id);
    Task<ShipResult> CreateShipmentAsync(Shipment header, IEnumerable<string> qrIds, string createdBy);
    Task<ShipResult> ShipShipmentAsync(int id, string shippedBy);
    // kích hoạt thông tin sản xuất (InvF_ProductionActive)
    Task<List<ProductLife>> ProductLivesAsync();
    Task<List<ProductionActive>> ProductionActivesAsync();
    Task<ProductionActive?> GetProductionActiveAsync(int id);
    Task<PaResult> CreateProductionActiveAsync(string paNo, string refNo, string origin, int productId,
        int qtyPlan, DateTime productDate, int productLifeId, string listSerialIn, string listSerialOut, string createdBy);
    Task<PaResult> DeleteProductionActiveAsync(int id);
    // danh mục nguồn gốc (Mst_NguonGoc)
    Task<List<OriginCatalog>> OriginsAsync(string? q);
    Task<OriginCatalog?> GetOriginAsync(int id);
    Task<OriginResult> CreateOriginAsync(OriginCatalog o, string createdBy);
    Task<OriginResult> UpdateOriginAsync(int id, OriginCatalog o);
    Task<OriginResult> DeleteOriginAsync(int id);
    // truy xuất nguồn gốc GS1 (Mst_CTE / Mst_KDE / CTE_KDE / Event_Event)
    Task<List<TraceEventType>> TraceEventTypesAsync();
    Task<TraceEventType?> GetTraceEventTypeAsync(int id);
    Task<List<TraceKde>> TraceKdesAsync();
    Task<List<TraceEvent>> TraceEventsAsync(string? cteCode);
    Task<TraceEvent?> GetTraceEventAsync(int id);
    Task<TraceEventResult> SaveTraceEventAsync(string? eventNo, string cteCode, string uiStyleCode, string? glnOrgCode,
        string? remark, IEnumerable<(string KdeCode, string KdeValue)> specs, string createdBy);
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
        await db.SaveChangesAsync();
        return new PackResult(true, $"Đã đóng {stamps.Count} tem vào hộp {box.BoxNo}.", box.Id, box.BoxNo, stamps.Count);
    }

    // ── ĐÓNG GÓI HỘP VÀO THÙNG (Map_Can) ────────────────────────────
    public Task<List<Carton>> CartonsAsync() =>
        db.Cartons.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Carton?> GetCartonAsync(int id) =>
        db.Cartons.Include(x => x.Product).Include(x => x.Boxes).ThenInclude(b => b.Product)
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

    private static string NewQrId()
        => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();   // 12 ký tự hex — ngắn gọn cho QR
}
