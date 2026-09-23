using Microsoft.EntityFrameworkCore;
using MiniStamp.Models;

namespace MiniStamp.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await MigratePostgresAsync(db);

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Stamp", ApiKey = TenantContext.DefaultApiKey });
            await db.SaveChangesAsync();
        }

        if (!await db.Rewards.AnyAsync())
        {
            db.Rewards.AddRange(
                new LotteryReward { Name = "Chúc bạn may mắn lần sau", Weight = 60, IsLose = true, Stock = 0 },
                new LotteryReward { Name = "Voucher 20.000đ", Weight = 25, Stock = 500 },
                new LotteryReward { Name = "Voucher 50.000đ", Weight = 12, Stock = 100 },
                new LotteryReward { Name = "Nạp thẻ 100.000đ", Weight = 3, Stock = 20 });
            await db.SaveChangesAsync();
        }

        // Danh mục hạn sử dụng (Mst_ProductLife) — master data cho kích hoạt SX
        if (!await db.ProductLives.AnyAsync())
        {
            db.ProductLives.AddRange(
                new ProductLife { Code = "1DAY", Name = "1 ngày", Type = "DAY", Value = 1, ValueByDay = 1 },
                new ProductLife { Code = "3DAY", Name = "3 ngày", Type = "DAY", Value = 3, ValueByDay = 3 },
                new ProductLife { Code = "1WEEK", Name = "1 tuần", Type = "WEEK", Value = 1, ValueByDay = 7 },
                new ProductLife { Code = "1MONTH", Name = "1 tháng", Type = "MONTH", Value = 1, ValueByDay = 30 },
                new ProductLife { Code = "3MONTH", Name = "3 tháng", Type = "MONTH", Value = 3, ValueByDay = 90 },
                new ProductLife { Code = "1YEAR", Name = "1 năm", Type = "MONTH", Value = 12, ValueByDay = 365 });
            await db.SaveChangesAsync();
        }

        // Danh mục loại Block (Mst_BlockType) — master data cho nghiệp vụ Map_Block
        if (!await db.BlockTypes.AnyAsync())
        {
            db.BlockTypes.AddRange(
                new BlockType { Code = "MASTAMP", Name = "Mã stamp", BlockSize = 100, CreatedBy = "seed" },
                new BlockType { Code = "MABOX", Name = "Mã Box", BlockSize = 50, CreatedBy = "seed" },
                new BlockType { Code = "MACAN", Name = "Mã Can", BlockSize = 20, CreatedBy = "seed" },
                new BlockType { Code = "MAPACK", Name = "Mã Pack", BlockSize = 10, CreatedBy = "seed" },
                new BlockType { Code = "MAPALLET", Name = "Mã Pallet", BlockSize = 1000, CreatedBy = "seed" },
                new BlockType { Code = "MACONTAINER", Name = "Mã Container", BlockSize = 5000, CreatedBy = "seed" });
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            var p1 = new Product { Code = "SP001", Name = "Phân bón NPK Lâm Thao 20kg", Manufacturer = "Supe Lâm Thao", WarrantyMonths = 0, Description = "Phân bón tổng hợp NPK." };
            var p2 = new Product { Code = "SP002", Name = "Rượu vang Đà Lạt 750ml", Manufacturer = "Vang Đà Lạt", WarrantyMonths = 0 };
            var p3 = new Product { Code = "SP003", Name = "Máy lọc nước Karofi", Manufacturer = "Karofi", WarrantyMonths = 24 };
            db.Products.AddRange(p1, p2, p3);
            await db.SaveChangesAsync();

            // 1 lô tem mẫu 30 tem cho SP001 để demo quét
            var batch = new StampBatch { ProductId = p1.Id, LotNo = "L2026-001", MfgDate = DateTime.Today.AddDays(-10), CreatedBy = "seed", Code = "LOTSEED01", Quantity = 30 };
            for (int i = 0; i < 30; i++)
                batch.Stamps.Add(new Stamp
                {
                    ProductId = p1.Id,
                    QrId = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                    Pin = Random.Shared.Next(100000, 999999).ToString(),
                    Status = StampStatus.Generated
                });
            db.Batches.Add(batch);
            await db.SaveChangesAsync();

            // 1 hộp mẫu: đóng 5 tem đầu của lô vào hộp để demo nghiệp vụ đóng gói
            var box = new Box { BoxNo = "BOX-SEED-001", ProductId = p1.Id, CreatedBy = "seed" };
            db.Boxes.Add(box);
            await db.SaveChangesAsync();
            var firstFive = batch.Stamps.Take(5).ToList();
            foreach (var s in firstFive) { s.BoxId = box.Id; s.BoxedAt = DateTime.Now; }
            box.Quantity = firstFive.Count;
            await db.SaveChangesAsync();

            // 1 thùng mẫu: đóng hộp vừa tạo vào thùng để demo nghiệp vụ đóng thùng (Map_Can)
            var carton = new Carton { CanNo = "CAN-SEED-001", ProductId = p1.Id, CreatedBy = "seed" };
            db.Cartons.Add(carton);
            await db.SaveChangesAsync();
            box.CartonId = carton.Id;
            box.CartonedAt = DateTime.Now;
            carton.BoxCount = 1;
            await db.SaveChangesAsync();

            // 1 bản ghi lịch sử đóng hộp mẫu (Map_IDInBoxHist) — lần đóng 5 tem vào hộp mẫu
            var hist = new BoxHistory
            {
                BoxNo = box.BoxNo, FunctionName = "MAP_IDINBOX_ADDX", RefType = "ADD",
                CreateDTimeUTC = DateTime.Now.AddDays(-1), QtyIDNo = firstFive.Count, CreatedBy = "seed"
            };
            foreach (var s in firstFive) hist.Lines.Add(new BoxHistoryLine { QrId = s.QrId });
            db.BoxHistories.Add(hist);
            await db.SaveChangesAsync();

            // 1 cặp tem mẫu (Map_StampPair) — ghép 2 tem đầu của lô thành 1 cặp 1:1
            var pairTwo = batch.Stamps.Take(2).ToList();
            if (pairTwo.Count == 2)
            {
                db.StampPairs.Add(new StampPair
                {
                    MainQrId = pairTwo[0].QrId, SubQrId = pairTwo[1].QrId,
                    Remark = "Cặp tem đôi mẫu", CreatedBy = "seed"
                });
                await db.SaveChangesAsync();
            }

            // 1 block mẫu (Map_Block) — gom 4 tem của lô thành 1 block loại MABOX
            var blockFour = batch.Stamps.Skip(8).Take(4).ToList();
            if (blockFour.Count > 0)
            {
                var blk = new Block
                {
                    BlockNo = "BLK-SEED-001", ProductId = p1.Id, BlockType = "MABOX",
                    BlockLocalID = "PALLET-01", ShiftCode = "CA1", LotCode = batch.LotNo,
                    Qty = 50, QtyVerified = blockFour.Count, Remark = "Block mẫu", CreatedBy = "seed"
                };
                foreach (var s in blockFour)
                    blk.Lines.Add(new BlockLine { QrId = s.QrId, AddedAt = DateTime.Now });
                db.Blocks.Add(blk);
                await db.SaveChangesAsync();
            }

            // 1 bản ghi tem trung tính mẫu (Inv_InventoryNeutralID) — tem nghi vấn xuất kho nhiều lần
            var neutralStamp = batch.Stamps.Skip(5).FirstOrDefault();
            if (neutralStamp != null)
            {
                db.NeutralStamps.Add(new NeutralStamp
                {
                    QrId = neutralStamp.QrId, FlagNeutral = false, OutCount = 2,
                    Note = "Tem bị xuất kho 2 lần — nghi vấn trùng/thất lạc.", CreatedBy = "seed"
                });
                await db.SaveChangesAsync();
            }

            // Serial bí mật mẫu (Inv_InventorySecret) — 3 serial gắn với 3 tem đầu lô
            var secretStamps = batch.Stamps.Take(3).ToList();
            for (int i = 0; i < secretStamps.Count; i++)
            {
                db.InventorySecrets.Add(new InventorySecret
                {
                    SerialNo = $"SEC-SEED-{i + 1:D3}",
                    QrSerialNo = secretStamps[i].QrId,
                    Mst = "2600123456",
                    GenTimesNo = batch.Code,
                    SecretNo = $"SCR{i + 1:D4}",
                    FlagMap = true,
                    FlagUsed = false,
                    Remark = "Serial bí mật mẫu",
                    CreatedBy = "seed"
                });
            }
            await db.SaveChangesAsync();

            // 1 phiếu tem rách/vỡ mẫu: ghi nhận 2 tem cuối của lô là lỗi (InvF_BrokenStamp)
            var broken = new BrokenStamp { BsNo = "BS-SEED-001", ProductId = p1.Id, Note = "Tem rách khi dán nhãn", CreatedBy = "seed" };
            db.BrokenStamps.Add(broken);
            await db.SaveChangesAsync();
            var lastTwo = batch.Stamps.Skip(28).Take(2).ToList();
            foreach (var s in lastTwo)
            {
                broken.Lines.Add(new BrokenStampLine { QrId = s.QrId, BrokenAt = DateTime.Now });
                s.Status = StampStatus.Broken;
            }
            broken.Quantity = lastTwo.Count;
            await db.SaveChangesAsync();

            // 1 phiếu nhập kho thành phẩm mẫu (InvF_InventoryInFG) — đã duyệt
            var fg = new InventoryInFG { InvInNo = "PNK-SEED-001", Remark = "Nhập kho lô sản xuất mẫu", CreatedBy = "seed", Status = "APPROVE", ApprovedAt = DateTime.Now, ApprovedBy = "seed" };
            fg.Lines.Add(new InventoryInFGDtl { ProductId = p1.Id, Qty = 1000, ProductionDate = DateTime.Today.AddDays(-10) });
            fg.Lines.Add(new InventoryInFGDtl { ProductId = p3.Id, Qty = 200, ProductionDate = DateTime.Today.AddDays(-5) });
            db.InventoryInFGs.Add(fg);
            await db.SaveChangesAsync();

            // 1 phiếu xuất kho thành phẩm mẫu (InvF_InventoryOutFG) — đã duyệt
            var outfg = new InventoryOutFG
            {
                InvOutFGNo = "PXKTP-SEED-001", Mst = "2600123456", FormOutType = "KHONGMAVACH",
                InvOutType = "THUONGMAI", InvCode = "KHO01", InvFOutType = "OUTTHUONGMAI",
                PlateNo = "29C-678.90", DriverName = "Trần Văn B", DriverPhoneNo = "0987654321",
                AgentCode = "DL002", CustomerName = "Đại lý Phân bón Trung Kiên",
                Remark = "Xuất bán thành phẩm lô mẫu", CreatedBy = "seed",
                Status = "APPROVE", ApprovedAt = DateTime.Now, ApprovedBy = "seed"
            };
            outfg.Lines.Add(new InventoryOutFGDtl { ProductId = p1.Id, PartCode = p1.Code, Qty = 300, SerialNo = "NPK-0001\nNPK-0002" });
            outfg.Lines.Add(new InventoryOutFGDtl { ProductId = p2.Id, PartCode = p2.Code, Qty = 120 });
            db.InventoryOutFGs.Add(outfg);
            await db.SaveChangesAsync();

            // 1 phiếu xuất kho theo tem mẫu (Inv_VerifiedIDInOut) — đã xuất 3 tem của lô
            var ship = new Shipment
            {
                ShipmentNo = "PXK-SEED-001", CustomerCode = "KH001", CustomerName = "Đại lý Vật tư Nông nghiệp Phú Thọ",
                CustomerAddress = "TP. Việt Trì, Phú Thọ", PlateNo = "29C-123.45", DriverName = "Nguyễn Văn A",
                DriverPhoneNo = "0912345678", TransportType = "Đường bộ", ReceivePlace = "Kho đại lý Phú Thọ",
                RefNoSys = "DH-SEED-001", RefType = "SALES", Remark = "Xuất bán lô mẫu",
                CreatedBy = "seed", Status = "SHIPPED", ShippedAt = DateTime.Now, ShippedBy = "seed"
            };
            db.Shipments.Add(ship);
            await db.SaveChangesAsync();
            var shipThree = batch.Stamps.Skip(5).Take(3).ToList();
            foreach (var s in shipThree)
            {
                ship.Lines.Add(new ShipmentLine { QrId = s.QrId, ProductId = s.ProductId, ShippedAt = DateTime.Now });
                s.ShipmentId = ship.Id;
                s.ShippedAt = DateTime.Now;
                s.CustomerCode = ship.CustomerCode;
            }
            await db.SaveChangesAsync();

            // 1 phiếu kích hoạt bán hàng mẫu (Inv_InvVerifiedID_ActivateSales) — đã bán 2 tem của lô
            var saTwo = batch.Stamps.Skip(20).Take(2).ToList();
            if (saTwo.Count > 0)
            {
                var sa = new SalesActivation
                {
                    SaNo = "PXKHT-SEED-001", RefNoSys = "PXKHT.SEED.0", RefType = "INVOUT",
                    ProductId = p1.Id, CustomerCode = "KH001", CustomerName = "Đại lý Vật tư Nông nghiệp Phú Thọ",
                    SalesDTime = DateTime.Now.AddDays(-1), Remark = "Kích hoạt bán lô mẫu", CreatedBy = "seed"
                };
                foreach (var s in saTwo)
                {
                    sa.Lines.Add(new SalesActivationLine { QrId = s.QrId, SalesDTime = sa.SalesDTime });
                    s.FlagSales = true;
                    s.SalesDTime = sa.SalesDTime;
                    s.CustomerCode = sa.CustomerCode;
                }
                sa.Quantity = saTwo.Count;
                db.SalesActivations.Add(sa);
                await db.SaveChangesAsync();
                foreach (var s in saTwo) s.SalesActivationId = sa.Id;
                await db.SaveChangesAsync();
            }

            // 1 phiếu kích hoạt thông tin sản xuất mẫu (InvF_ProductionActive)
            var life = await db.ProductLives.FirstOrDefaultAsync(x => x.Code == "1MONTH");
            if (life != null)
            {
                var pDate = DateTime.Today.AddDays(-5);
                db.ProductionActives.Add(new ProductionActive
                {
                    PaNo = "PA-SEED-001", RefNo = "DH-SEED-001", Origin = "Trang trại VietGAP 1030",
                    ProductId = p1.Id, QtyPlan = 1000, ProductDate = pDate,
                    ExpiryDate = pDate.AddDays(life.Value - 1), ProductLifeId = life.Id,
                    ListSerialInManufacture = "000001-001000", ListSerialOutManufacture = "000001-001000",
                    CreatedBy = "seed"
                });
                await db.SaveChangesAsync();
            }

            // 1 phiên sản xuất mẫu (InvF_ProductionSession) — gom 6 tem đầu của lô vào 1 phiên ca CA1
            var psSix = batch.Stamps.Skip(10).Take(6).ToList();
            if (psSix.Count > 0)
            {
                var ps = new ProductionSession
                {
                    PsNo = "PS-SEED-001", OrgCode = "ORG01", ShiftCode = "CA1", LotCode = batch.LotNo,
                    ProductId = p1.Id, QtyInput = 6, Remark = "Phiên sản xuất mẫu", CreatedBy = "seed"
                };
                int psIdx = 0;
                foreach (var s in psSix)
                    ps.Lines.Add(new ProductionSessionLine { QrId = s.QrId, Idx = ++psIdx });
                ps.QtyVerified = ps.Lines.Count;
                db.ProductionSessions.Add(ps);
                await db.SaveChangesAsync();
            }

            // Danh mục nguồn gốc mẫu (Mst_NguonGoc) — dùng cho trường Origin ở màn Kích hoạt SX
            db.OriginCatalogs.AddRange(
                new OriginCatalog
                {
                    Code = "CP1", Name = "Trang trại VietGAP 1030", DisplayName = "CP1 (VietGAP 1030)",
                    CertificateCode = "VietGAP", CertificateNo = "1030", CertificateName = "Chứng nhận VietGAP số 1030",
                    CertificateDateStart = DateTime.Today.AddYears(-1), CertificateDateEnd = DateTime.Today.AddYears(1),
                    Address = "Xã Phú Hộ, TX. Phú Thọ", GlnCode = "8930000000001", CreatedBy = "seed"
                },
                new OriginCatalog
                {
                    Code = "CP2", Name = "Hợp tác xã Rau an toàn Đà Lạt", DisplayName = "CP2 (GlobalGAP 2024)",
                    CertificateCode = "GlobalGAP", CertificateNo = "2024", CertificateName = "Chứng nhận GlobalGAP số 2024",
                    CertificateDateStart = DateTime.Today.AddMonths(-6), CertificateDateEnd = DateTime.Today.AddMonths(18),
                    Address = "TP. Đà Lạt, Lâm Đồng", CreatedBy = "seed"
                });
            await db.SaveChangesAsync();

            // Danh mục địa điểm GS1 mẫu (Mst_GLN) — dùng cho trường GLN ở sự kiện truy xuất / nguồn gốc
            db.Gs1Locations.AddRange(
                new Gs1Location
                {
                    Code = "8930000000001", Name = "Nhà máy Supe Lâm Thao", GpsLat = "21.3221", GpsLong = "105.4012",
                    OrgCode = "ORG01", Remark = "Địa điểm sản xuất chính", CreatedBy = "seed"
                },
                new Gs1Location
                {
                    Code = "8930000000002", Name = "Kho thành phẩm Việt Trì", GpsLat = "21.3010", GpsLong = "105.4300",
                    OrgCode = "ORG01", Remark = "Kho lưu trữ thành phẩm", CreatedBy = "seed"
                });
            await db.SaveChangesAsync();

            // Danh mục truy xuất GS1 (Mst_CTE / Mst_KDE / CTE_KDE) — master data cho màn Sự kiện truy xuất
            var cteHarvest = new TraceEventType { Code = "HARVEST", Name = "Thu hoạch", TplVECode = "TPL-HARVEST", TplVEDetail = "Mẫu hiển thị sự kiện thu hoạch", CreatedBy = "seed" };
            var ctePack = new TraceEventType { Code = "PACKING", Name = "Đóng gói", TplVECode = "TPL-PACKING", TplVEDetail = "Mẫu hiển thị sự kiện đóng gói", CreatedBy = "seed" };
            var cteShip = new TraceEventType { Code = "SHIPPING", Name = "Vận chuyển", TplVECode = "TPL-SHIPPING", TplVEDetail = "Mẫu hiển thị sự kiện vận chuyển", CreatedBy = "seed" };
            db.TraceEventTypes.AddRange(cteHarvest, ctePack, cteShip);
            await db.SaveChangesAsync();

            var kdeLot = new TraceKde { Code = "LOTNO", Name = "Số lô", DataType = "TEXT", IsKey = true, CreatedBy = "seed" };
            var kdeDate = new TraceKde { Code = "EVENTDATE", Name = "Ngày sự kiện", DataType = "DATE", IsKey = true, CreatedBy = "seed" };
            var kdeTemp = new TraceKde { Code = "TEMPERATURE", Name = "Nhiệt độ", DataType = "NUMBER", CreatedBy = "seed" };
            var kdeOperator = new TraceKde { Code = "OPERATOR", Name = "Người thực hiện", DataType = "TEXT", CreatedBy = "seed" };
            var kdeQty = new TraceKde { Code = "QUANTITY", Name = "Khối lượng", DataType = "NUMBER", CreatedBy = "seed" };
            var kdeItems = new TraceKde { Code = "ITEMLIST", Name = "Danh sách mặt hàng", DataType = "TEXT", IsList = true, CreatedBy = "seed" };
            db.TraceKdes.AddRange(kdeLot, kdeDate, kdeTemp, kdeOperator, kdeQty, kdeItems);
            await db.SaveChangesAsync();

            // Gán KDE vào từng CTE (CTE_KDE)
            db.TraceEventTypeKdes.AddRange(
                new TraceEventTypeKde { TraceEventTypeId = cteHarvest.Id, TraceKdeId = kdeLot.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = cteHarvest.Id, TraceKdeId = kdeDate.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = cteHarvest.Id, TraceKdeId = kdeQty.Id },
                new TraceEventTypeKde { TraceEventTypeId = cteHarvest.Id, TraceKdeId = kdeOperator.Id },
                new TraceEventTypeKde { TraceEventTypeId = ctePack.Id, TraceKdeId = kdeLot.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = ctePack.Id, TraceKdeId = kdeDate.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = ctePack.Id, TraceKdeId = kdeItems.Id, IsList = true },
                new TraceEventTypeKde { TraceEventTypeId = cteShip.Id, TraceKdeId = kdeLot.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = cteShip.Id, TraceKdeId = kdeDate.Id, IsKey = true },
                new TraceEventTypeKde { TraceEventTypeId = cteShip.Id, TraceKdeId = kdeTemp.Id },
                new TraceEventTypeKde { TraceEventTypeId = cteShip.Id, TraceKdeId = kdeOperator.Id });
            await db.SaveChangesAsync();

            // 1 sự kiện truy xuất mẫu (Event_Event) — thu hoạch lô L2026-001
            var ev = new TraceEvent
            {
                EventNo = "EV-SEED-001", CteCode = "HARVEST", UIStyleCode = "DEFAULT",
                GLNOrgCode = "8930000000001", TplVECode = "TPL-HARVEST", TplVEDetail = "Mẫu hiển thị sự kiện thu hoạch",
                Remark = "Thu hoạch lô mẫu", CreatedBy = "seed"
            };
            ev.Specs.Add(new TraceEventSpec { CteCode = "HARVEST", KdeCode = "LOTNO", KdeValue = "L2026-001" });
            ev.Specs.Add(new TraceEventSpec { CteCode = "HARVEST", KdeCode = "EVENTDATE", KdeValue = DateTime.Today.AddDays(-10).ToString("yyyy-MM-dd") });
            ev.Specs.Add(new TraceEventSpec { CteCode = "HARVEST", KdeCode = "QUANTITY", KdeValue = "1000" });
            ev.Specs.Add(new TraceEventSpec { CteCode = "HARVEST", KdeCode = "OPERATOR", KdeValue = "Nguyễn Văn A" });
            db.TraceEvents.Add(ev);
            await db.SaveChangesAsync();

            // 1 hóa đơn điện tử mẫu (Invoice_Invoice) — đã phát hành (cấp số)
            var inv = new Invoice
            {
                InvoiceCode = "HD-SEED-001", InvoiceNo = "1", Status = "ISSUED",
                TInvoiceCode = "MAU01", InvoiceNoStart = 1, InvoiceNoEnd = 100,
                RefNo = "DH-SEED-001", Mst = "2600123456", PaymentMethodCode = "TM",
                CustomerNntCode = "KH001", CustomerNntName = "Đại lý Vật tư Nông nghiệp Phú Thọ",
                CustomerNntAddress = "TP. Việt Trì, Phú Thọ", CustomerNntPhone = "0912345678",
                CustomerMst = "2600987654", InvoiceDate = DateTime.Today.AddDays(-3),
                TotalValVat = 500000, Remark = "Hóa đơn bán lô mẫu", CreatedBy = "seed",
                ApprovedAt = DateTime.Now.AddDays(-3), ApprovedBy = "seed",
                IssuedAt = DateTime.Now.AddDays(-3), IssuedBy = "seed"
            };
            inv.Lines.Add(new InvoiceDtl { ProductId = p1.Id, PartCode = p1.Code, UnitName = "Bao", Qty = 100, UnitPrice = 250000, Amount = 100 * 250000 });
            inv.Lines.Add(new InvoiceDtl { ProductId = p2.Id, PartCode = p2.Code, UnitName = "Chai", Qty = 20, UnitPrice = 150000, Amount = 20 * 150000 });
            inv.TotalValInvoice = inv.Lines.Sum(l => l.Amount);
            inv.TotalValPmt = inv.TotalValInvoice + inv.TotalValVat;
            db.Invoices.Add(inv);
            await db.SaveChangesAsync();

            // 1 yêu cầu xuất kho mẫu (InvF_ReqInvOut) — đã duyệt, gắn phiếu xuất theo tem
            var req = new ReqInvOut
            {
                ReqInvOutNo = "YCXK-SEED-001", RefNo = "YC-SEED-001", InvCode = "KHO01",
                InvOutType = "THUONGMAI", InvOutDate = DateTime.Today.AddDays(-2),
                TransportType = "Đường bộ", VehicleNumber = "29C-123.45",
                CustomerCodeSys = "KH001", ReceiveAddress = "Kho đại lý Phú Thọ",
                IVerifiedIDInOutNo = "PXK-SEED-001", Remark = "Yêu cầu xuất lô mẫu",
                CreatedBy = "seed", ReqStatus = "APPROVE", ApprovedAt = DateTime.Now.AddDays(-2), ApprovedBy = "seed"
            };
            req.Lines.Add(new ReqInvOutDtl { ProductId = p1.Id, ProductCode = p1.Code, UnitCode = "Bao", Qty = 300 });
            req.Lines.Add(new ReqInvOutDtl { ProductId = p2.Id, ProductCode = p2.Code, UnitCode = "Chai", Qty = 120, FlagDiscount = true });
            db.ReqInvOuts.Add(req);
            await db.SaveChangesAsync();

            // 1 lượt kích hoạt bảo hành bằng PIN mẫu (WarrantyDateStartFromPIN_Activate)
            // — kích hoạt tem đầu tiên của lô (đã có PIN) để demo màn Kích hoạt bảo hành
            var wStamp = batch.Stamps.First();
            var wDate = DateTime.Today.AddDays(-2);
            wStamp.ActivatedAt = wDate;
            wStamp.ActivatedPhone = "0912345678";
            wStamp.Status = StampStatus.Activated;
            wStamp.WarrantyNo = "BH-SEED-001";
            wStamp.WarrantyCount = 1;
            wStamp.WarrantyStartIp = "127.0.0.1";
            wStamp.WarrantyEnd = wDate.AddMonths(wStamp.Product?.WarrantyMonths ?? 12);
            db.WarrantyActivations.Add(new WarrantyActivation
            {
                QrId = wStamp.QrId, Pin = wStamp.Pin, PhoneNoUser = "0912345678", IpAddress = "127.0.0.1",
                WarrantyNo = "BH-SEED-001", WarrantyDateStart = wDate, IsFirstActivate = true,
                WarrantyCount = 1, CreatedBy = "seed"
            });
            await db.SaveChangesAsync();

            // 1 kỳ vòng đời tem mẫu (InvIVID_LifeCircleIDNoByPeriod) — chốt kỳ tháng trước
            var prevPeriod = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            db.StampLifecyclePeriods.Add(new StampLifecyclePeriod
            {
                PeriodMonth = prevPeriod,
                QtyVerifiedID = 30, QtySales = 3, QtyWarranty = 1, QtySearch = 0,
                DeltaVerifiedID = 30, DeltaSales = 3, DeltaWarranty = 1, DeltaSearch = 0,
                Remark = "Chốt số liệu vòng đời tem kỳ mẫu", CreatedBy = "seed"
            });
            await db.SaveChangesAsync();

            // 1 phiếu xuất kho theo hộp mẫu (Inv_InventoryVerifiedID_OutByBox) — quét 1 mã hộp
            // (BOX-SEED-001 chứa 5 tem đầu lô) + 1 tem lẻ (tem thứ 6) để demo nghiệp vụ bung hộp.
            var boxShip = new BoxShipment
            {
                BsNo = "PXKH-SEED-001", CustomerCode = "KH002", CustomerName = "Đại lý Vật tư Nông nghiệp Phú Thọ",
                CustomerAddress = "TP. Việt Trì, Phú Thọ", PlateNo = "29C-123.45", DriverName = "Nguyễn Văn A",
                DriverPhoneNo = "0912345678", TransportType = "Đường bộ", ReceivePlace = "Kho đại lý Phú Thọ",
                RefNoSys = "DH-SEED-002", RefType = "SALES", Remark = "Xuất theo hộp lô mẫu",
                CreatedBy = "seed", Status = "PENDING"
            };
            var boxStamps = batch.Stamps.Where(s => s.BoxId == box.Id).ToList();
            foreach (var s in boxStamps)
                boxShip.Lines.Add(new BoxShipmentLine { ScanCode = box.BoxNo, StampType = "BOX", QrId = s.QrId, ProductId = s.ProductId, ShippedAt = DateTime.Now });
            var looseStamp = batch.Stamps.Skip(6).FirstOrDefault();
            if (looseStamp != null)
                boxShip.Lines.Add(new BoxShipmentLine { ScanCode = looseStamp.QrId, StampType = "ID", QrId = looseStamp.QrId, ProductId = looseStamp.ProductId, ShippedAt = DateTime.Now });
            db.BoxShipments.Add(boxShip);
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Products", "Batches", "Boxes", "Cartons", "BoxHistories", "BoxHistoryLines", "StampPairs", "Stamps", "ScanLogs", "WarrantyActivations", "Rewards", "BrokenStamps", "BrokenStampLines", "InventoryInFGs", "InventoryInFGDtls", "InventoryOutFGs", "InventoryOutFGDtls", "Shipments", "ShipmentLines", "ProductLives", "ProductionActives", "ProductionSessions", "ProductionSessionLines", "OriginCatalogs", "Gs1Locations", "TraceEventTypes", "TraceKdes", "TraceEventTypeKdes", "TraceEvents", "TraceEventSpecs", "Invoices", "InvoiceDtls", "BlockTypes", "Blocks", "BlockLines", "NeutralStamps", "InventorySecrets", "ReqInvOuts", "ReqInvOutDtls", "StampLifecyclePeriods", "BoxShipments", "BoxShipmentLines" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS ministamp.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON ministamp.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables)
            sql.Add($"ALTER TABLE ministamp.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql)
            try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
