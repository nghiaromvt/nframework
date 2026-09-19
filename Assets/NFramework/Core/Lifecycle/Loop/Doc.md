# GameLoop Module

Module điều phối vòng lặp (`Update`, `FixedUpdate`, `LateUpdate`) tập trung trong `NFramework.Core`, thay thế việc các component riêng lẻ tự cài đặt các phương thức lifecycle của Unity (`Update`, `FixedUpdate`, `LateUpdate`).

---

## 1. Mục đích và Kiến trúc
- **Giảm Native-to-Managed Overhead**: Giảm thiểu chi phí reflection C++ sang C# từ Unity Engine khi có nhiều `Update()` trong scene.
- **Fail Fast & An Toàn**:
  - Tự động phát hiện các `UnityEngine.Object` đã bị `Destroy` mà quên unregister (`o == null`), ghi nhận lỗi qua `NLogger.LogError` và tự động gỡ bỏ.
  - Sử dụng hàng đợi pending cho các đăng ký (`Register`) phát sinh trong lúc đang duyệt vòng lặp.
  - Khi `Unregister` giữa khung hình, slot trong danh sách thực thi ngay lập tức được gán `null` để đảm bảo đối tượng không bao giờ được gọi thêm một lần nào trong frame đó, sau đó tiến hành dọn dẹp (compaction) sau khi hoàn tất phase duyệt.
- **Cách ly ngoại lệ (Exception Isolation)**: Bắt exception trên từng đối tượng và ghi log qua `NLogger.LogException`, không làm gián đoạn các đối tượng khác trong danh sách lặp.
- **Zero GC Allocation in Tick**: Sử dụng vòng lặp chỉ số `for` trực tiếp trên danh sách định sẵn dung lượng, không cấp phát heap (`GC.Alloc`), không dùng LINQ hay foreach trong `Update/FixedUpdate/LateUpdate`.

---

## 2. Các Interface Hỗ Trợ

Tất cả các interface thuộc namespace `NFramework`:

```csharp
namespace NFramework
{
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    public interface IFixedUpdatable
    {
        void OnFixedUpdate(float fixedDeltaTime);
    }

    public interface ILateUpdatable
    {
        void OnLateUpdate(float deltaTime);
    }
}
```

Một class có thể implement bất kỳ một hoặc nhiều interface trên. Khi truyền vào `GameLoop.Register(target)`, hệ thống sẽ tự động gán vào các vòng lặp tương ứng.

---

## 3. Cách sử dụng (Sample & Usage)

### Đăng ký và Hủy đăng ký từ C# Class hoặc MonoBehaviour

```csharp
using UnityEngine;
using NFramework;

public class MovementController : MonoBehaviour, IUpdatable, IFixedUpdatable
{
    private void OnEnable()
    {
        GameLoop.Register(this);
    }

    private void OnDisable()
    {
        GameLoop.Unregister(this);
    }

    public void OnUpdate(float deltaTime)
    {
        // Xử lý logic frame (Time.deltaTime)
    }

    public void OnFixedUpdate(float fixedDeltaTime)
    {
        // Xử lý physics (Time.fixedDeltaTime)
    }
}
```

### Sử dụng với Plain C# Object (POCO / Pure Service)

```csharp
using NFramework;

public class NetworkSyncService : IUpdatable
{
    public void Start()
    {
        GameLoop.Register(this);
    }

    public void Stop()
    {
        GameLoop.Unregister(this);
    }

    public void OnUpdate(float deltaTime)
    {
        // Xử lý heartbeat / buffer gói tin
    }
}
```

---

## 4. Verification & Testing

Script mẫu kiểm thử toàn diện đã được tạo tại:
`Assets/NFramework/Core/GameLoop/Sample/GameLoopVerificationTest.cs`

Bao gồm các kịch bản:
1. Đăng ký đối tượng C# thuần (Plain C# Object).
2. Kiểm tra cô lập lỗi (throw exception không làm văng luồng của các listener khác).
3. Hủy đăng ký giữa chừng (Mid-frame unregister) đảm bảo không nhận callback ngay trong chính frame đó.
4. Tự động dọn dẹp các `GameObject` / `MonoBehaviour` bị `Destroy` ngoài ý muốn mà quên `Unregister`.
5. Đăng ký trùng lặp (Double register) hoặc đăng ký đối tượng không implement interface tương ứng.
