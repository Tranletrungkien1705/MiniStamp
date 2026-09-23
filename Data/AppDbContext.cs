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
    public DbSet<BoxHistory> BoxHistories => Set<BoxHistory>();
    public DbSet<BoxHistoryLine> BoxHistoryLines => Set<BoxHistoryLine>();
    public DbSet<StampPair> StampPairs => Set<StampPair>();
    public DbSet<Stamp> Stamps => Set<Stamp>();
    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
    public DbSet<WarrantyActivation> WarrantyActivations => Set<WarrantyActivation>();
    public DbSet<LotteryReward> Rewards => Set<LotteryReward>();
    public DbSet<BrokenStamp> BrokenStamps => Set<BrokenStamp>();
    public DbSet<BrokenStampLine> BrokenStampLines => Set<BrokenStampLine>();
    public DbSet<InventoryInFG> InventoryInFGs => Set<InventoryInFG>();
    public DbSet<InventoryInFGDtl> InventoryInFGDtls => Set<InventoryInFGDtl>();
    public DbSet<InventoryOutFG> InventoryOutFGs => Set<InventoryOutFG>();
    public DbSet<InventoryOutFGDtl> InventoryOutFGDtls => Set<InventoryOutFGDtl>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<ProductLife> ProductLives => Set<ProductLife>();
    public DbSet<ProductionActive> ProductionActives => Set<ProductionActive>();
    public DbSet<ProductionSession> ProductionSessions => Set<ProductionSession>();
    public DbSet<ProductionSessionLine> ProductionSessionLines => Set<ProductionSessionLine>();
    public DbSet<OriginCatalog> OriginCatalogs => Set<OriginCatalog>();
    public DbSet<Gs1Location> Gs1Locations => Set<Gs1Location>();
    public DbSet<TraceEventType> TraceEventTypes => Set<TraceEventType>();
    public DbSet<TraceKde> TraceKdes => Set<TraceKde>();
    public DbSet<TraceEventTypeKde> TraceEventTypeKdes => Set<TraceEventTypeKde>();
    public DbSet<TraceEvent> TraceEvents => Set<TraceEvent>();
    public DbSet<TraceEventSpec> TraceEventSpecs => Set<TraceEventSpec>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDtl> InvoiceDtls => Set<InvoiceDtl>();
    public DbSet<SalesActivation> SalesActivations => Set<SalesActivation>();
    public DbSet<SalesActivationLine> SalesActivationLines => Set<SalesActivationLine>();
    public DbSet<BlockType> BlockTypes => Set<BlockType>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<BlockLine> BlockLines => Set<BlockLine>();
    public DbSet<NeutralStamp> NeutralStamps => Set<NeutralStamp>();
    public DbSet<ReqInvOut> ReqInvOuts => Set<ReqInvOut>();
    public DbSet<ReqInvOutDtl> ReqInvOutDtls => Set<ReqInvOutDtl>();
    public DbSet<StampLifecyclePeriod> StampLifecyclePeriods => Set<StampLifecyclePeriod>();

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
            e.HasOne(x => x.SalesActivation).WithMany().HasForeignKey(x => x.SalesActivationId);
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
        b.Entity<BoxHistory>(e =>
        {
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BoxHistoryLine>(e =>
        {
            e.HasOne(x => x.BoxHistory).WithMany(x => x.Lines).HasForeignKey(x => x.BoxHistoryId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StampPair>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.MainQrId }).IsUnique();   // 1 tem chính chỉ thuộc 1 cặp
            e.HasIndex(x => new { x.OrgId, x.SubQrId }).IsUnique();    // 1 tem phụ chỉ thuộc 1 cặp
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
        b.Entity<InventoryOutFG>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.InvOutFGNo }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InventoryOutFGDtl>(e =>
        {
            e.HasOne(x => x.InventoryOutFG).WithMany(x => x.Lines).HasForeignKey(x => x.InventoryOutFGId);
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
        b.Entity<ProductLife>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductionActive>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PaNo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.RefNo }).IsUnique();   // RefNo duy nhất trong tenant
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.ProductLife).WithMany().HasForeignKey(x => x.ProductLifeId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<OriginCatalog>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();   // NguonGocCode duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductionSession>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PsNo }).IsUnique();   // IF_PSNo duy nhất trong tenant
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductionSessionLine>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();   // 1 tem chỉ thuộc 1 phiên sản xuất
            e.HasOne(x => x.ProductionSession).WithMany(x => x.Lines).HasForeignKey(x => x.ProductionSessionId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Gs1Location>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();   // GLNCode duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TraceEventType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();   // CTECode duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TraceKde>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();   // KDECode duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TraceEventTypeKde>(e =>
        {
            e.HasIndex(x => new { x.TraceEventTypeId, x.TraceKdeId }).IsUnique();
            e.HasOne(x => x.TraceEventType).WithMany(x => x.Kdes).HasForeignKey(x => x.TraceEventTypeId);
            e.HasOne(x => x.TraceKde).WithMany().HasForeignKey(x => x.TraceKdeId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TraceEvent>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.EventNo }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TraceEventSpec>(e =>
        {
            e.HasOne(x => x.TraceEvent).WithMany(x => x.Specs).HasForeignKey(x => x.TraceEventId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Invoice>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.InvoiceCode }).IsUnique();   // mã hóa đơn duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InvoiceDtl>(e =>
        {
            e.HasOne(x => x.Invoice).WithMany(x => x.Lines).HasForeignKey(x => x.InvoiceId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SalesActivation>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SaNo }).IsUnique();   // mã phiếu duy nhất trong tenant
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SalesActivationLine>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();   // 1 tem chỉ kích hoạt bán 1 lần
            e.HasOne(x => x.SalesActivation).WithMany(x => x.Lines).HasForeignKey(x => x.SalesActivationId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BlockType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();   // BlockType duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Block>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BlockNo }).IsUnique();   // mã block duy nhất trong tenant
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BlockLine>(e =>
        {
            e.HasIndex(x => x.QrId).IsUnique();   // 1 tem chỉ thuộc 1 block
            e.HasOne(x => x.Block).WithMany(x => x.Lines).HasForeignKey(x => x.BlockId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<NeutralStamp>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.QrId }).IsUnique();   // 1 tem chỉ có 1 bản ghi trung tính
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ReqInvOut>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ReqInvOutNo }).IsUnique();   // mã yêu cầu duy nhất trong tenant
            e.HasIndex(x => new { x.OrgId, x.RefNo }).IsUnique();          // số yêu cầu duy nhất trong tenant
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ReqInvOutDtl>(e =>
        {
            e.HasOne(x => x.ReqInvOut).WithMany(x => x.Lines).HasForeignKey(x => x.ReqInvOutId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ScanLog>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<StampLifecyclePeriod>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PeriodMonth }).IsUnique();   // mỗi kỳ tháng chỉ chốt 1 lần
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyActivation>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.QrId });   // tra lịch sử kích hoạt theo tem
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
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
