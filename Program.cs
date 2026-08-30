using Microsoft.EntityFrameworkCore;
using MiniStamp.Data;
using MiniStamp.Models;
using MiniStamp.Services;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FleetObs.ConfigureLogger("ministamp");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ministamp.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IStampService, StampService>();
builder.Services.AddHttpClient();   // đồng bộ danh mục từ MiniPIM
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddFleetObs();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.UseFleetObs();
app.UseCors();

// Multi-tenant (admin): org = cookie org_key / header X-Api-Key. Trang tra cứu công khai KHÔNG cần.
app.Use(async (ctx, next) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) ctx.Request.Cookies.TryGetValue(TenantContext.CookieName, out key);
    if (!string.IsNullOrWhiteSpace(key))
    {
        using var lookup = app.Services.CreateScope();
        var ldb = lookup.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = await ldb.Orgs.FirstOrDefaultAsync(o => o.ApiKey == key);
        if (org != null) ctx.RequestServices.GetRequiredService<ITenantContext>().OrgId = org.Id;
    }
    await next();
});

app.UseStaticFiles();
app.MapGet("/healthz", () => "ok");

// API tra cứu tem công khai (cho app/ứng dụng bên thứ 3)
app.MapGet("/api/verify", async (string code, IStampService svc, HttpContext ctx) =>
{
    var r = await svc.VerifyAsync(code, ctx.Connection.RemoteIpAddress?.ToString());
    return Results.Ok(new
    {
        found = r.Found, genuine = r.Genuine, title = r.Title, message = r.Message,
        product = r.Product?.Name, manufacturer = r.Product?.Manufacturer,
        lot = r.Batch?.LotNo, mfgDate = r.Batch?.MfgDate.ToString("yyyy-MM-dd"),
        scanCount = r.Stamp?.ScanCount, warnings = r.Warnings
    });
});

// API tích hợp: MiniService tra tình trạng bảo hành xe theo VIN
app.MapGet("/api/warranty", async (string vin, IStampService svc) =>
{
    var w = await svc.WarrantyByVinAsync(vin);
    if (w is null || !w.Found) return Results.NotFound(new { vin, found = false });
    return Results.Ok(new
    {
        vin, found = true, active = w.Active, product = w.Product, qrId = w.QrId,
        warrantyEnd = w.WarrantyEnd?.ToString("yyyy-MM-dd"), daysLeft = w.DaysLeft
    });
});

// API tích hợp: MiniWMS nhập kho → phát nguyên lô tem chính hãng cho lô hàng (LotNo = số phiếu)
app.MapPost("/api/ext/wh-batch", async (WhBatchDto dto, IStampService svc, HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(dto.Product) || string.IsNullOrWhiteSpace(dto.LotNo))
        return Results.BadRequest(new { error = "Cần Product và LotNo." });
    if (dto.Quantity <= 0) return Results.BadRequest(new { error = "Số lượng phải > 0." });
    var (batchCode, quantity, firstQrId, product) = await svc.CreateWarehouseBatchAsync(
        dto.Product, dto.LotNo, dto.Quantity, dto.Manufacturer);
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Ok(new
    {
        batchCode, quantity, product, firstQrId,
        sampleVerifyUrl = firstQrId is null ? null : $"{baseUrl}/Verify?code={firstQrId}"
    });
});

// API tích hợp: MiniShowroom giao xe → phát tem chính hãng + kích hoạt bảo hành theo VIN
app.MapPost("/api/ext/vehicle-stamp", async (VehicleStampDto dto, IStampService svc, HttpContext ctx) =>
{
    var (qrId, pin, product, warrantyEnd) = await svc.CreateVehicleStampAsync(
        dto.VehicleModel, dto.Vin, dto.Plate, dto.BuyerPhone);
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Ok(new
    {
        qrId, pin, product,
        warrantyEnd = warrantyEnd?.ToString("yyyy-MM-dd"),
        verifyUrl = $"{baseUrl}/Verify?code={qrId}"
    });
});

// Đăng ký nhà sản xuất mới (nhận khách)
app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "stp_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org);
    await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

record RegisterOrgDto(string Name);
record VehicleStampDto(string VehicleModel, string? Vin, string? Plate, string? BuyerPhone);
record WhBatchDto(string Product, string LotNo, int Quantity, string? Manufacturer);
