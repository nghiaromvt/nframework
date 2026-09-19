# Lifecycle Module

Module quản lý và điều phối toàn bộ vòng đời ứng dụng trong `NFramework.Core`, bao gồm:
1. **Loop** (`GameLoop`): Điều phối các vòng lặp khung hình (`Update`, `FixedUpdate`, `LateUpdate`).
2. **Application** (`ApplicationLifecycle`): Điều phối các sự kiện cấp ứng dụng của Unity (`OnApplicationPause`, `OnApplicationFocus`, `OnApplicationQuit`).

---

## 1. ApplicationLifecycle

### Mục đích và Tính năng
- Bắt các sự kiện vòng đời cấp ứng dụng từ Unity Engine và phân phối đến các class C# thuần (pure C# classes) hoặc MonoBehaviour.
- **Tự động khởi tạo (Lazy Persistent Singleton)**: Tự tạo GameObject persistent (`DontDestroyOnLoad`) ngay khi có đối tượng đầu tiên gọi đăng ký.
- **An toàn & Re-entrancy**:
  - Hỗ trợ an toàn khi `Register` hoặc `Unregister` ngay bên trong callback dispatch mà không gây xung đột duyệt danh sách.
  - Tự động phát hiện và unregister các `UnityEngine.Object` đã bị `Destroy`.
- **Cách ly ngoại lệ (Exception Isolation)**:
  - Từng listener được bảo vệ bằng khối try-catch và ghi log qua `NLogger.LogException`. Nếu một listener bị lỗi (crash), các listener còn lại (như lưu game, dừng nhạc) vẫn được thực thi đầy đủ.
- **Zero GC Allocation in Dispatch**:
  - Sử dụng vòng lặp `for` theo chỉ số trên mảng danh sách định sẵn dung lượng, không cấp phát heap trong quá trình dispatch.

### Các Interface Hỗ Trợ

```csharp
namespace NFramework
{
    public interface IApplicationPauseListener
    {
        void OnApplicationPause(bool isPaused);
    }

    public interface IApplicationFocusListener
    {
        void OnApplicationFocus(bool hasFocus);
    }

    public interface IApplicationQuitListener
    {
        void OnApplicationQuit();
    }
}
```

### Cách Sử Dụng

#### Với Pure C# Class

```csharp
using NFramework;

public class SaveService : IApplicationPauseListener, IApplicationQuitListener
{
    public void Initialize()
    {
        ApplicationLifecycle.Register(this);
    }

    public void Dispose()
    {
        ApplicationLifecycle.Unregister(this);
    }

    public void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveGameData();
        }
    }

    public void OnApplicationQuit()
    {
        SaveGameData();
    }

    private void SaveGameData()
    {
        // Thực hiện lưu dữ liệu
    }
}
```

#### Kiểm Tra Trạng Thái Trực Tiếp

```csharp
if (ApplicationLifecycle.IsPaused)
{
    // Ứng dụng đang bị pause
}

if (ApplicationLifecycle.IsFocused)
{
    // Ứng dụng đang có focus
}

if (ApplicationLifecycle.IsQuitting)
{
    // Ứng dụng đang trong quá trình thoát
}
```

---

## 2. GameLoop

Chi tiết về module GameLoop xem tại [Doc.md](Loop/Doc.md).
