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
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Products", "Batches", "Stamps", "ScanLogs", "Rewards" };
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
