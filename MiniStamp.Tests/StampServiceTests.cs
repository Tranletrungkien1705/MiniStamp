using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniStamp.Data;
using MiniStamp.Models;
using MiniStamp.Services;
using Xunit;

namespace MiniStamp.Tests;

/// <summary>Test tem chống giả: sinh lô (QR duy nhất), verify thật/giả + đếm quét, kích hoạt BH, quay thưởng (1 lần, trừ kho).</summary>
public class StampServiceTests
{
    private static (AppDbContext db, IStampService svc, SqliteConnection conn) NewSvc()
    {
        var conn = new SqliteConnection("DataSource=:memory:"); conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(opt, new TenantContext { OrgId = TenantContext.DefaultOrgId });
        db.Database.EnsureCreated();
        return (db, new StampService(db), conn);
    }

    private static async Task<string> GenOneStamp(AppDbContext db, IStampService svc, int months = 12)
    {
        var pid = await svc.CreateProductAsync(new Product { Code = "P1", Name = "SP1", WarrantyMonths = months });
        var bid = await svc.GenerateBatchAsync(new StampBatch { ProductId = pid }, 1);
        var b = await svc.GetBatchAsync(bid);
        return b!.Stamps[0].QrId;
    }

    [Fact]
    public async Task Generate_CreatesUniqueQrIds()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var pid = await svc.CreateProductAsync(new Product { Code = "P1", Name = "SP" });
            var bid = await svc.GenerateBatchAsync(new StampBatch { ProductId = pid }, 50);
            var b = await svc.GetBatchAsync(bid);
            Assert.Equal(50, b!.Stamps.Count);
            Assert.Equal(50, b.Stamps.Select(s => s.QrId).Distinct().Count());  // toàn bộ QR duy nhất
        }
    }

    [Fact]
    public async Task Verify_Genuine_ForRealStamp()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var qr = await GenOneStamp(db, svc);
            var v = await svc.VerifyAsync(qr, "1.2.3.4");
            Assert.True(v.Found);
            Assert.True(v.Genuine);
        }
    }

    [Fact]
    public async Task Verify_Fake_ForUnknownQr()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var v = await svc.VerifyAsync("KHONGCOTHAT", null);
            Assert.False(v.Found);
            Assert.False(v.Genuine);
        }
    }

    [Fact]
    public async Task Verify_IncrementsScanCount()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var qr = await GenOneStamp(db, svc);
            await svc.VerifyAsync(qr, null);
            await svc.VerifyAsync(qr, null);
            var s = await db.Stamps.IgnoreQueryFilters().FirstAsync(x => x.QrId == qr);
            Assert.Equal(2, s.ScanCount);
        }
    }

    [Fact]
    public async Task Activate_SetsWarrantyEnd_AndBlocksSecond()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var qr = await GenOneStamp(db, svc, 24);
            var (ok, _) = await svc.ActivateAsync(qr, "0900000000");
            Assert.True(ok);
            var s = await db.Stamps.IgnoreQueryFilters().FirstAsync(x => x.QrId == qr);
            Assert.Equal(StampStatus.Activated, s.Status);
            Assert.NotNull(s.WarrantyEnd);
            var (ok2, _) = await svc.ActivateAsync(qr, "0900000000");
            Assert.False(ok2);   // không kích hoạt lần 2
        }
    }

    [Fact]
    public async Task Spin_AwardsOnce_AndDecrementsStock()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var qr = await GenOneStamp(db, svc);
            db.Rewards.Add(new LotteryReward { OrgId = TenantContext.DefaultOrgId, Name = "Voucher 50k", Weight = 1, Stock = 5 });
            await db.SaveChangesAsync();
            var (ok, prize) = await svc.SpinAsync(qr);
            Assert.True(ok);
            Assert.Equal("Voucher 50k", prize);
            var reward = await db.Rewards.IgnoreQueryFilters().FirstAsync();
            Assert.Equal(4, reward.Stock);           // trừ kho
            var (ok2, _) = await svc.SpinAsync(qr);
            Assert.False(ok2);                        // không quay lần 2
        }
    }
}
