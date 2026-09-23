using Microsoft.EntityFrameworkCore;
using MiniStamp.Models;

namespace MiniStamp.Data;

public class AppDbContext : DbContext
{
    private readonly Guid _orgId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant) : base(options)
        => _orgId = tenant.OrgId;

    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StampBatch> Batches => Set<StampBatch>();
    public DbSet<Stamp> Stamps => Set<Stamp>();
    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
    public DbSet<LotteryReward> Rewards => Set<LotteryReward>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("ministamp");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();

        b.Entity<Product>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StampBatch>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Stamp>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();          // QR duy nhất TOÀN CỤC (tra cứu công khai theo QrId)
            e.HasOne(x => x.Batch).WithMany(x => x.Stamps).HasForeignKey(x => x.BatchId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ScanLog>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<LotteryReward>().HasQueryFilter(x => x.OrgId == _orgId);
    }

    public override int SaveChanges() { StampOrg(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { StampOrg(); return base.SaveChangesAsync(ct); }

    private void StampOrg()
    {
        foreach (var entry in ChangeTracker.Entries<IOrgOwned>())
            if (entry.State == EntityState.Added && entry.Entity.OrgId == Guid.Empty)
                entry.Entity.OrgId = _orgId;
    }
}
