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
