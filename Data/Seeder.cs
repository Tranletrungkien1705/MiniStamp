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
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Products", "Batches", "Boxes", "Cartons", "Stamps", "ScanLogs", "Rewards", "BrokenStamps", "BrokenStampLines", "InventoryInFGs", "InventoryInFGDtls", "InventoryOutFGs", "InventoryOutFGDtls", "Shipments", "ShipmentLines", "ProductLives", "ProductionActives", "OriginCatalogs", "TraceEventTypes", "TraceKdes", "TraceEventTypeKdes", "TraceEvents", "TraceEventSpecs" };
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
