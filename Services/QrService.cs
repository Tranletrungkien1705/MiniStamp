using QRCoder;

namespace MiniStamp.Services;

/// <summary>Sinh ảnh QR (PNG) từ nội dung — dùng cho tem &amp; link tra cứu.</summary>
public static class QrService
{
    public static byte[] PngBytes(string content, int pixelsPerModule = 6)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }

    public static string DataUri(string content, int pixelsPerModule = 6)
        => "data:image/png;base64," + Convert.ToBase64String(PngBytes(content, pixelsPerModule));
}
