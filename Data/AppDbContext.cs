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
    public DbSet<Box> Boxes => Set<Box>();
    public DbSet<Carton> Cartons => Set<Carton>();
    public DbSet<Stamp> Stamps => Set<Stamp>();
    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
    public DbSet<LotteryReward> Rewards => Set<LotteryReward>();
    public DbSet<BrokenStamp> BrokenStamps => Set<BrokenStamp>();
    public DbSet<BrokenStampLine> BrokenStampLines => Set<BrokenStampLine>();
    public DbSet<InventoryInFG> InventoryInFGs => Set<InventoryInFG>();
    public DbSet<InventoryInFGDtl> InventoryInFGDtls => Set<InventoryInFGDtl>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();

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
            e.HasOne(x => x.Box).WithMany(x => x.Stamps).HasForeignKey(x => x.BoxId);
            e.HasOne(x => x.Shipment).WithMany().HasForeignKey(x => x.ShipmentId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Box>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BoxNo }).IsUnique();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.Carton).WithMany(x => x.Boxes).HasForeignKey(x => x.CartonId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Carton>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CanNo }).IsUnique();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BrokenStamp>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BsNo }).IsUnique();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BrokenStampLine>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();   // 1 tem chỉ ghi nhận lỗi 1 lần
            e.HasOne(x => x.BrokenStamp).WithMany(x => x.Lines).HasForeignKey(x => x.BrokenStampId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InventoryInFG>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.InvInNo }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InventoryInFGDtl>(e =>
        {
            e.HasOne(x => x.InventoryInFG).WithMany(x => x.Lines).HasForeignKey(x => x.InventoryInFGId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Shipment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ShipmentNo }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ShipmentLine>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();   // 1 tem chỉ xuất 1 lần
            e.HasOne(x => x.Shipment).WithMany(x => x.Lines).HasForeignKey(x => x.ShipmentId);
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
