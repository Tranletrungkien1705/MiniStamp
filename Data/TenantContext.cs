namespace MiniStamp.Data;

/// <summary>Ngữ cảnh tenant (nhà sản xuất). Middleware set OrgId (cookie org_key / header X-Api-Key). Trang tra cứu công khai KHÔNG lọc tenant.</summary>
public interface ITenantContext { Guid OrgId { get; set; } }

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DefaultOrgId = new("66666666-6666-6666-6666-666666666666");
    public const string DefaultApiKey = "demo-stamp";
    public const string CookieName = "org_key";

    public Guid OrgId { get; set; } = DefaultOrgId;
}
