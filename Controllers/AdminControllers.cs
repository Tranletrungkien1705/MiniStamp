using Microsoft.AspNetCore.Mvc;
using MiniStamp.Models;
using MiniStamp.Services;

namespace MiniStamp.Controllers;

public class HomeController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Dash = await svc.DashboardAsync();
        return View();
    }
}

public class ProductController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ProductsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? code, string? manufacturer, int warrantyMonths, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên sản phẩm."; return View(); }
        await svc.CreateProductAsync(new Product { Name = name.Trim(), Code = code ?? "", Manufacturer = manufacturer, WarrantyMonths = warrantyMonths, Description = description });
        TempData["Success"] = "Đã tạo sản phẩm.";
        return RedirectToAction(nameof(Index));
    }
}

public class BatchController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BatchesAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string lotNo, DateTime mfgDate, int quantity)
    {
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        var id = await svc.GenerateBatchAsync(new StampBatch { ProductId = productId, LotNo = lotNo ?? "", MfgDate = mfgDate == default ? DateTime.Today : mfgDate, CreatedBy = "web" }, quantity);
        TempData["Success"] = $"Đã sinh {quantity} tem QR.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var b = await svc.GetBatchAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }
}

public class StampController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, int? batchId)
    {
        ViewBag.Q = q; ViewBag.BatchId = batchId;
        return View(await svc.StampsAsync(q, batchId));
    }

    // Ảnh QR của 1 tem (mã hóa link tra cứu công khai)
    public IActionResult Qr(string code, int px = 6)
    {
        var url = $"{Request.Scheme}://{Request.Host}/Verify?code={code}";
        return File(QrService.PngBytes(url, px), "image/png");
    }
}

// Đóng gói tem vào hộp (nghiệp vụ Map_IDInBox của EQR)
public class BoxController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BoxesAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string? boxNo, string? codes)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        if (string.IsNullOrWhiteSpace(boxNo)) boxNo = $"BOX{DateTime.Now:yyMMddHHmmss}";
        var r = await svc.PackBoxAsync(boxNo.Trim(), productId, list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.BoxId });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var b = await svc.GetBoxAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }

    // Gộp tem vào 1 hộp mới (nghiệp vụ Map_IDInBox_Merge của EQR)
    public async Task<IActionResult> Merge()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Merge(int productId, string? boxNo, string? codes)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        var r = await svc.MergeBoxAsync(boxNo ?? "", productId, list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.BoxId });
    }
}

// Khôi phục hộp tem từ lịch sử (nghiệp vụ Map_IDInBox_RestoreBoxNo của EQR)
public class BoxHistoryController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? boxNo)
    {
        ViewBag.BoxNo = boxNo;
        return View(await svc.BoxHistoriesAsync(boxNo));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var h = await svc.GetBoxHistoryAsync(id);
        if (h == null) return NotFound();
        return View(h);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var r = await svc.RestoreBoxAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}

// Đóng gói hộp vào thùng (nghiệp vụ Map_Can của EQR)
public class CartonController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.CartonsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string? canNo, string? boxNos)
    {
        var list = (boxNos ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        if (string.IsNullOrWhiteSpace(canNo)) canNo = $"CAN{DateTime.Now:yyMMddHHmmss}";
        var r = await svc.PackCartonAsync(canNo.Trim(), productId, list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.CartonId });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var c = await svc.GetCartonAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    // Gán tem trực tiếp vào thùng (nghiệp vụ Inv_InventoryVerifiedID_UpdCan của EQR)
    public IActionResult Assign() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(string? canNo, string? codes)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var r = await svc.AssignStampsToCartonAsync(canNo ?? "", list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.CartonId });
    }

    // Gán tem vào thùng THEO HỘP (nghiệp vụ Inv_InventoryVerifiedID_UpdCanFromBox của EQR)
    public IActionResult AssignFromBox() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignFromBox(string? canNo, string? boxNos)
    {
        var list = (boxNos ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var r = await svc.AssignStampsToCartonFromBoxAsync(canNo ?? "", list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.CartonId });
    }
}

// Ghép cặp tem (nghiệp vụ Map_StampPair của EQR)
public class StampPairController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.StampPairsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? mainQrId, string? subQrId, string? remark)
    {
        var r = await svc.PairStampsAsync(mainQrId ?? "", subQrId ?? "", remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var p = await svc.GetStampPairAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }
}

// Gom tem vào Block (nghiệp vụ Map_Block của EQR)
public class BlockController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BlocksAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        ViewBag.BlockTypes = await svc.BlockTypesAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string? blockNo, string? blockType, string? blockLocalId,
        string? shiftCode, string? lotCode, string? codes, string? remark)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); ViewBag.BlockTypes = await svc.BlockTypesAsync(); return View(); }
        var r = await svc.CreateBlockAsync(blockNo ?? "", productId, blockType ?? "", blockLocalId, shiftCode, lotCode, list, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); ViewBag.BlockTypes = await svc.BlockTypesAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var b = await svc.GetBlockAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }
}

// Tem trung tính / nghi vấn (nghiệp vụ Inv_InventoryNeutralID của EQR)
public class NeutralStampController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(bool? flagNeutral)
    {
        ViewBag.FlagNeutral = flagNeutral;
        return View(await svc.NeutralStampsAsync(flagNeutral));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var n = await svc.GetNeutralStampAsync(id);
        if (n == null) return NotFound();
        return View(n);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Detect()
    {
        var r = await svc.DetectSuspectStampsAsync("web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(int id, string? note)
    {
        var r = await svc.ResolveNeutralStampAsync(id, note, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}

// Tem bí mật / serial ẩn (nghiệp vụ Inv_InventorySecret của EQR)
public class InventorySecretController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, bool? flagUsed)
    {
        ViewBag.Q = q; ViewBag.FlagUsed = flagUsed;
        return View(await svc.InventorySecretsAsync(q, flagUsed));
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? serialNo, string? qrSerialNo, string? mst, string? genTimesNo,
        string? secretNo, string? remark)
    {
        if (string.IsNullOrWhiteSpace(serialNo)) { TempData["Error"] = "Cần nhập SerialNo."; return View(); }
        var r = await svc.CreateInventorySecretAsync(serialNo, qrSerialNo, mst, genTimesNo, secretNo, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var s = await svc.GetInventorySecretAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUsed(string? serials, string? mst)
    {
        var list = (serials ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var r = await svc.MarkSecretsUsedAsync(list, mst, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Phiếu tem rách/vỡ (nghiệp vụ InvF_BrokenStamp của EQR)
public class BrokenStampController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BrokenStampsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, string? bsNo, string? codes, string? note)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        if (string.IsNullOrWhiteSpace(bsNo)) bsNo = $"BS{DateTime.Now:yyMMddHHmmss}";
        var r = await svc.ReportBrokenAsync(bsNo.Trim(), productId, list, note, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.BrokenStampId });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var b = await svc.GetBrokenStampAsync(id);
        if (b == null) return NotFound();
        return View(b);
    }
}

// Phiếu nhập kho thành phẩm (nghiệp vụ InvF_InventoryInFG của EQR)
public class InventoryInController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.InventoryInFGsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? invInNo, string? remark, int[]? productId, int[]? qty, DateTime[]? productionDate)
    {
        var products = await svc.ProductsAsync();
        var lines = new List<(int, int, DateTime)>();
        if (productId != null)
            for (int i = 0; i < productId.Length; i++)
            {
                var q = (qty != null && i < qty.Length) ? qty[i] : 0;
                var d = (productionDate != null && i < productionDate.Length) ? productionDate[i] : DateTime.Today;
                if (productId[i] > 0 && q > 0) lines.Add((productId[i], q, d));
            }
        if (lines.Count == 0) { TempData["Error"] = "Cần ít nhất 1 dòng có sản phẩm và số lượng > 0."; ViewBag.Products = products; return View(); }
        var r = await svc.CreateInventoryInFGAsync(invInNo ?? "", remark, lines, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = products; return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var fg = await svc.GetInventoryInFGAsync(id);
        if (fg == null) return NotFound();
        return View(fg);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await svc.ApproveInventoryInFGAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}

// Phiếu xuất kho thành phẩm (nghiệp vụ InvF_InventoryOutFG của EQR)
public class InventoryOutController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.InventoryOutFGsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? invOutFGNo, string? mst, string? invFOutType, string? invOutType,
        string? invCode, string? pmType, string? formOutType, string? plateNo, string? moocNo, string? driverName,
        string? driverPhoneNo, string? agentCode, string? customerName, string? remark,
        int[]? productId, int[]? qty, string[]? serialNo)
    {
        var products = await svc.ProductsAsync();
        var lines = new List<(int, int, string?)>();
        if (productId != null)
            for (int i = 0; i < productId.Length; i++)
            {
                var q = (qty != null && i < qty.Length) ? qty[i] : 0;
                var sn = (serialNo != null && i < serialNo.Length) ? serialNo[i] : null;
                if (productId[i] > 0 && q > 0) lines.Add((productId[i], q, sn));
            }
        if (lines.Count == 0) { TempData["Error"] = "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0."; ViewBag.Products = products; return View(); }

        var header = new InventoryOutFG
        {
            InvOutFGNo = invOutFGNo ?? "", Mst = mst, InvFOutType = invFOutType ?? "OUTTHUONGMAI",
            InvOutType = invOutType, InvCode = invCode, PmType = pmType, FormOutType = formOutType ?? "KHONGMAVACH",
            PlateNo = plateNo, MoocNo = moocNo, DriverName = driverName, DriverPhoneNo = driverPhoneNo,
            AgentCode = agentCode, CustomerName = customerName, Remark = remark
        };
        var r = await svc.CreateInventoryOutFGAsync(header, lines, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = products; return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var fg = await svc.GetInventoryOutFGAsync(id);
        if (fg == null) return NotFound();
        return View(fg);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await svc.ApproveInventoryOutFGAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await svc.DeleteInventoryOutFGAsync(id);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut / OutGenInAndOut của EQR)
public class ShipmentController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ShipmentsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? shipmentNo, string? customerCode, string? customerName,
        string? customerAddress, string? plateNo, string? moocNo, string? driverName, string? driverPhoneNo,
        string? transportType, string? receivePlace, string? refNoSys, string? refNo, string? refType,
        string? remark, string? codes)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var header = new Shipment
        {
            ShipmentNo = shipmentNo ?? "",
            CustomerCode = customerCode, CustomerName = customerName, CustomerAddress = customerAddress,
            PlateNo = plateNo, MoocNo = moocNo, DriverName = driverName, DriverPhoneNo = driverPhoneNo,
            TransportType = transportType, ReceivePlace = receivePlace,
            RefNoSys = refNoSys, RefNo = refNo, RefType = refType, Remark = remark
        };
        var r = await svc.CreateShipmentAsync(header, list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var s = await svc.GetShipmentAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(int id)
    {
        var r = await svc.ShipShipmentAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var r = await svc.CancelShipmentAsync(id, reason, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    // Gộp phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut_Merge của EQR)
    public IActionResult Merge() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Merge(string? refNoSys, string? productCode, string? userMoveOrder)
    {
        var r = await svc.MergeShipmentsAsync(refNoSys ?? "", productCode ?? "", userMoveOrder ?? "", "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.KeptId });
    }

    // Cho phép sửa phiếu xuất kho theo tem (nghiệp vụ Inv_VerifiedIDInOut_UpdFlagAllowModify của EQR)
    public IActionResult AllowModify() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AllowModify(string? refNoSys, string? plateNo, int minutes)
    {
        var r = await svc.AllowModifyShipmentAsync(refNoSys ?? "", plateNo ?? "", minutes, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Index));
    }
}

// Xuất kho theo hộp (nghiệp vụ Inv_InventoryVerifiedID_OutByBox của EQR)
public class BoxShipmentController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.BoxShipmentsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? bsNo, string? customerCode, string? customerName,
        string? customerAddress, string? plateNo, string? moocNo, string? driverName, string? driverPhoneNo,
        string? transportType, string? receivePlace, string? refNoSys, string? refNo, string? refType,
        string? remark, string? codes)
    {
        // Mỗi dòng quét: "<mã>" (mặc định tem lẻ ID) hoặc "BOX:<mã hộp>" / "ID:<mã tem>"
        var scans = new List<(string, string)>();
        foreach (var raw in (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var token = raw.Trim();
            if (token.Length == 0) continue;
            var type = "ID";
            var code = token;
            var idx = token.IndexOf(':');
            if (idx > 0)
            {
                type = token[..idx].Trim().ToUpperInvariant();
                code = token[(idx + 1)..].Trim();
            }
            scans.Add((code, type));
        }
        var header = new BoxShipment
        {
            BsNo = bsNo ?? "",
            CustomerCode = customerCode, CustomerName = customerName, CustomerAddress = customerAddress,
            PlateNo = plateNo, MoocNo = moocNo, DriverName = driverName, DriverPhoneNo = driverPhoneNo,
            TransportType = transportType, ReceivePlace = receivePlace,
            RefNoSys = refNoSys, RefNo = refNo, RefType = refType, Remark = remark
        };
        var r = await svc.CreateBoxShipmentAsync(header, scans, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var s = await svc.GetBoxShipmentAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(int id)
    {
        var r = await svc.ShipBoxShipmentAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var r = await svc.CancelBoxShipmentAsync(id, reason, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}

// Kích hoạt thông tin sản xuất (nghiệp vụ InvF_ProductionActive của EQR)
public class ProductionActiveController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ProductionActivesAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        ViewBag.ProductLives = await svc.ProductLivesAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? paNo, string? refNo, string? origin, int productId,
        int qtyPlan, DateTime productDate, int productLifeId, string? listSerialIn, string? listSerialOut)
    {
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); ViewBag.ProductLives = await svc.ProductLivesAsync(); return View(); }
        var r = await svc.CreateProductionActiveAsync(paNo ?? "", refNo ?? "", origin ?? "", productId,
            qtyPlan, productDate, productLifeId, listSerialIn ?? "", listSerialOut ?? "", "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); ViewBag.ProductLives = await svc.ProductLivesAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var pa = await svc.GetProductionActiveAsync(id);
        if (pa == null) return NotFound();
        return View(pa);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await svc.DeleteProductionActiveAsync(id);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Phiên sản xuất (nghiệp vụ InvF_ProductionSession của EQR)
public class ProductionSessionController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ProductionSessionsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? psNo, string? orgCode, string? shiftCode, string? lotCode,
        int productId, int qtyInput, string? codes, string? remark)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (string.IsNullOrWhiteSpace(psNo)) psNo = $"PS{DateTime.Now:yyMMddHHmmss}";
        var r = await svc.CreateProductionSessionAsync(psNo.Trim(), orgCode, shiftCode, lotCode,
            productId, qtyInput, list, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var ps = await svc.GetProductionSessionAsync(id);
        if (ps == null) return NotFound();
        return View(ps);
    }
}
public class OriginController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.OriginsAsync(q));
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? code, string? name, string? certificateCode, string? certificateNo,
        string? certificateName, DateTime? certificateDateStart, DateTime? certificateDateEnd,
        string? address, string? glnCode, string? remark, bool isActive)
    {
        var o = new OriginCatalog
        {
            Code = code ?? "", Name = name ?? "", CertificateCode = certificateCode, CertificateNo = certificateNo,
            CertificateName = certificateName, CertificateDateStart = certificateDateStart, CertificateDateEnd = certificateDateEnd,
            Address = address, GlnCode = glnCode, Remark = remark, IsActive = isActive
        };
        var r = await svc.CreateOriginAsync(o, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View(o);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var o = await svc.GetOriginAsync(id);
        if (o == null) return NotFound();
        return View(o);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string? name, string? certificateCode, string? certificateNo,
        string? certificateName, DateTime? certificateDateStart, DateTime? certificateDateEnd,
        string? address, string? glnCode, string? remark, bool isActive)
    {
        var o = new OriginCatalog
        {
            Name = name ?? "", CertificateCode = certificateCode, CertificateNo = certificateNo,
            CertificateName = certificateName, CertificateDateStart = certificateDateStart, CertificateDateEnd = certificateDateEnd,
            Address = address, GlnCode = glnCode, Remark = remark, IsActive = isActive
        };
        var r = await svc.UpdateOriginAsync(id, o);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { o.Id = id; o.Code = r.Code; return View(o); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await svc.DeleteOriginAsync(id);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Danh mục địa điểm GS1 (nghiệp vụ Mst_GLN của EQR)
public class Gs1LocationController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.Gs1LocationsAsync(q));
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? code, string? name, string? gpsLat, string? gpsLong,
        string? orgCode, string? remark, bool isActive)
    {
        var g = new Gs1Location
        {
            Code = code ?? "", Name = name ?? "", GpsLat = gpsLat, GpsLong = gpsLong,
            OrgCode = orgCode, Remark = remark, IsActive = isActive
        };
        var r = await svc.CreateGs1LocationAsync(g, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View(g);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var g = await svc.GetGs1LocationAsync(id);
        if (g == null) return NotFound();
        return View(g);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string? name, string? gpsLat, string? gpsLong,
        string? orgCode, string? remark, bool isActive)
    {
        var g = new Gs1Location
        {
            Name = name ?? "", GpsLat = gpsLat, GpsLong = gpsLong,
            OrgCode = orgCode, Remark = remark, IsActive = isActive
        };
        var r = await svc.UpdateGs1LocationAsync(id, g);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { g.Id = id; g.Code = r.Code; return View(g); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await svc.DeleteGs1LocationAsync(id);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Truy xuất nguồn gốc GS1 (nghiệp vụ Mst_CTE / Mst_KDE / CTE_KDE / Event_Event của EQR)
public class TraceController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? cteCode)
    {
        ViewBag.CteCode = cteCode;
        ViewBag.Types = await svc.TraceEventTypesAsync();
        return View(await svc.TraceEventsAsync(cteCode));
    }

    // Danh mục loại sự kiện (Mst_CTE) + trường dữ liệu (Mst_KDE)
    public async Task<IActionResult> Catalog()
    {
        ViewBag.Kdes = await svc.TraceKdesAsync();
        return View(await svc.TraceEventTypesAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Types = await svc.TraceEventTypesAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? eventNo, string? cteCode, string? uiStyleCode, string? glnOrgCode,
        string? remark, string[]? kdeCode, string[]? kdeValue)
    {
        var specs = new List<(string, string)>();
        if (kdeCode != null)
            for (int i = 0; i < kdeCode.Length; i++)
            {
                var v = (kdeValue != null && i < kdeValue.Length) ? kdeValue[i] : "";
                if (!string.IsNullOrWhiteSpace(kdeCode[i])) specs.Add((kdeCode[i], v ?? ""));
            }
        var r = await svc.SaveTraceEventAsync(eventNo, cteCode ?? "", uiStyleCode ?? "", glnOrgCode, remark, specs, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Types = await svc.TraceEventTypesAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var ev = await svc.GetTraceEventAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }
}

// Hóa đơn điện tử (nghiệp vụ Invoice_Invoice của EQR)
public class InvoiceController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.InvoicesAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? invoiceCode, string? tinvoiceCode, long invoiceNoStart, long invoiceNoEnd,
        string? refNo, string? mst, string? paymentMethodCode, string? customerNntCode, string? customerNntName,
        string? customerNntAddress, string? customerNntPhone, string? customerNntEmail, string? customerMst,
        DateTime invoiceDate, decimal totalValVat, string? remark,
        int[]? productId, int[]? qty, decimal[]? unitPrice)
    {
        var products = await svc.ProductsAsync();
        var lines = new List<(int, int, decimal)>();
        if (productId != null)
            for (int i = 0; i < productId.Length; i++)
            {
                var q = (qty != null && i < qty.Length) ? qty[i] : 0;
                var up = (unitPrice != null && i < unitPrice.Length) ? unitPrice[i] : 0m;
                if (productId[i] > 0 && q > 0) lines.Add((productId[i], q, up));
            }
        if (lines.Count == 0) { TempData["Error"] = "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0."; ViewBag.Products = products; return View(); }

        var header = new Invoice
        {
            InvoiceCode = invoiceCode ?? "", TInvoiceCode = tinvoiceCode ?? "",
            InvoiceNoStart = invoiceNoStart, InvoiceNoEnd = invoiceNoEnd,
            RefNo = refNo, Mst = mst, PaymentMethodCode = paymentMethodCode,
            CustomerNntCode = customerNntCode, CustomerNntName = customerNntName, CustomerNntAddress = customerNntAddress,
            CustomerNntPhone = customerNntPhone, CustomerNntEmail = customerNntEmail, CustomerMst = customerMst,
            InvoiceDate = invoiceDate, TotalValVat = totalValVat, Remark = remark
        };
        var r = await svc.CreateInvoiceAsync(header, lines, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = products; return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var inv = await svc.GetInvoiceAsync(id);
        if (inv == null) return NotFound();
        return View(inv);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await svc.ApproveInvoiceAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(int id)
    {
        var r = await svc.IssueInvoiceAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var r = await svc.CancelInvoiceAsync(id, reason);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}

// Kích hoạt bán hàng (nghiệp vụ Inv_InvVerifiedID_ActivateSales của EQR)
public class SalesActivationController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.SalesActivationsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? saNo, int productId, string? customerCode, string? customerName,
        DateTime salesDTime, string? remark, string? codes)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        var r = await svc.ActivateSalesAsync(saNo ?? "", productId, customerCode, customerName, salesDTime, list, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var sa = await svc.GetSalesActivationAsync(id);
        if (sa == null) return NotFound();
        return View(sa);
    }

    // Hoàn tác kích hoạt bán hàng (nghiệp vụ Inv_InventoryVerifiedID_FlagSalesBackStatus của EQR)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revert(int id, string? reason)
    {
        var r = await svc.RevertSalesActivationAsync(id, reason, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Yêu cầu xuất kho (nghiệp vụ InvF_ReqInvOut của EQR)
public class ReqInvOutController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ReqInvOutsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? reqInvOutNo, string? refNo, string? invCode, string? invOutType,
        DateTime invOutDate, string? transportType, string? vehicleNumber, string? customerCodeSys,
        string? receiveAddress, string? remark,
        int[]? productId, int[]? qty, string[]? unitCode, bool[]? flagDiscount, string[]? lineRemark)
    {
        var products = await svc.ProductsAsync();
        var lines = new List<(int, int, string?, bool, string?)>();
        if (productId != null)
            for (int i = 0; i < productId.Length; i++)
            {
                var q = (qty != null && i < qty.Length) ? qty[i] : 0;
                var uc = (unitCode != null && i < unitCode.Length) ? unitCode[i] : null;
                var fd = (flagDiscount != null && i < flagDiscount.Length) && flagDiscount[i];
                var lr = (lineRemark != null && i < lineRemark.Length) ? lineRemark[i] : null;
                if (productId[i] > 0 && q > 0) lines.Add((productId[i], q, uc, fd, lr));
            }
        if (lines.Count == 0) { TempData["Error"] = "Cần ít nhất 1 dòng có mặt hàng và số lượng > 0."; ViewBag.Products = products; return View(); }

        var header = new ReqInvOut
        {
            ReqInvOutNo = reqInvOutNo ?? "", RefNo = refNo ?? "", InvCode = invCode, InvOutType = invOutType,
            InvOutDate = invOutDate, TransportType = transportType, VehicleNumber = vehicleNumber,
            CustomerCodeSys = customerCodeSys, ReceiveAddress = receiveAddress, Remark = remark
        };
        var r = await svc.CreateReqInvOutAsync(header, lines, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = products; return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var r = await svc.GetReqInvOutAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? iVerifiedIDInOutNo)
    {
        var r = await svc.ApproveReqInvOutAsync(id, iVerifiedIDInOutNo, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UnApprove(int id)
    {
        var r = await svc.UnApproveReqInvOutAsync(id, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await svc.DeleteReqInvOutAsync(id);
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

// Vòng đời tem theo kỳ tháng (nghiệp vụ InvIVID_LifeCircleIDNoByPeriod của EQR)
public class LifecycleController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.LifecyclePeriodsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime periodMonth, string? remark)
    {
        if (periodMonth == default) { TempData["Error"] = "Chọn kỳ tháng cần chốt."; return View(); }
        var r = await svc.SnapshotLifecycleAsync(periodMonth, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var p = await svc.GetLifecyclePeriodAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }
}

// Kích hoạt bảo hành bằng PIN (nghiệp vụ WarrantyDateStartFromPIN_Activate của EQR)
public class WarrantyController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index(string? qrId)
    {
        ViewBag.QrId = qrId;
        return View(await svc.WarrantyActivationsAsync(qrId));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var w = await svc.GetWarrantyActivationAsync(id);
        if (w == null) return NotFound();
        return View(w);
    }
}

// Nhập dãy serial người dùng (nghiệp vụ Inv_StampUser của EQR)
public class StampUserController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.StampUsersAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? suiNo, string? serials)
    {
        // Mỗi dòng: "<IDNo_User>" hoặc "<IDNo_User>|<ghi chú>"
        var list = new List<(string, string?)>();
        foreach (var raw in (serials ?? "").Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var token = raw.Trim();
            if (token.Length == 0) continue;
            var idx = token.IndexOf('|');
            if (idx >= 0) list.Add((token[..idx].Trim(), token[(idx + 1)..].Trim()));
            else list.Add((token, null));
        }
        if (string.IsNullOrWhiteSpace(suiNo)) suiNo = $"SUI{DateTime.Now:yyMMddHHmmss}";
        var r = await svc.ImportStampUsersAsync(suiNo.Trim(), list, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var su = await svc.GetStampUserAsync(id);
        if (su == null) return NotFound();
        return View(su);
    }
}

// Kích hoạt tem trắng bởi Trạm bán hàng (nghiệp vụ Inv_InventoryVerifiedID_ActivateByTBH của EQR)
public class TbhActivationController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.TbhActivationsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await svc.ProductsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? qrId, string? pin, int productId, string? customerCode,
        string? areaCode, string? proofImagePath, string? proofImagePathName, string? remark)
    {
        if (productId <= 0) { TempData["Error"] = "Chọn sản phẩm."; ViewBag.Products = await svc.ProductsAsync(); return View(); }
        var r = await svc.ActivateByTbhAsync(qrId ?? "", pin, productId, customerCode, areaCode,
            proofImagePath, proofImagePathName, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) { ViewBag.Products = await svc.ProductsAsync(); return View(); }
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var t = await svc.GetTbhActivationAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }
}

// Cập nhật ngày sản xuất cho tem (nghiệp vụ Inv_InventoryVerifiedID_UpdPrdDTime của EQR)
public class ProductionDateController(IStampService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.ProductionDateLogsAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime? productionDTime, string? codes, string? remark)
    {
        var list = (codes ?? "").Split(new[] { '\n', '\r', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var r = await svc.UpdateProductionDateAsync(productionDTime, list, remark, "web");
        TempData[r.Ok ? "Success" : "Error"] = r.Message;
        if (!r.Ok) return View();
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var log = await svc.GetProductionDateLogAsync(id);
        if (log == null) return NotFound();
        return View(log);
    }
}
