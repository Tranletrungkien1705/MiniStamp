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
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Products", "Batches", "Boxes", "Cartons", "Stamps", "ScanLogs", "Rewards", "BrokenStamps", "BrokenStampLines", "InventoryInFGs", "InventoryInFGDtls", "Shipments", "ShipmentLines", "ProductLives", "ProductionActives" };
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
