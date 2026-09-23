using MiniStamp.Models;

namespace MiniStamp.Services;

public static class Ui
{
    public static (string text, string css) Status(StampStatus s) => s switch
    {
        StampStatus.Generated => ("Chưa kích hoạt", "secondary"),
        StampStatus.Activated => ("Đã kích hoạt", "success"),
        StampStatus.Void => ("Vô hiệu", "danger"),
        _ => (s.ToString(), "secondary")
    };
}
