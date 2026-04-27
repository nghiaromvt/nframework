# UIManager

Hệ thống quản lý UI phân lớp (layer-based) cho Unity, hỗ trợ cả **Resources** và **Addressables**.

## Kiến Trúc

```
UIManager (SingletonMono)
├── Background Layer    ← Canvas + GraphicRaycaster + CanvasGroup
├── Menu Layer
├── Popup Layer
├── Loading Layer
└── AlwaysOnTop Layer
```

Mỗi layer là một `RectTransform` con chứa `Canvas`, `GraphicRaycaster`, và `CanvasGroup`, được tạo tự động trong `Awake()` dựa trên cấu hình `_uiLayerOrders`.

## Các Lớp Liên Quan

| Lớp | Mô tả |
|---|---|
| `UIManager` | Singleton quản lý toàn bộ UI lifecycle |
| `UIView` | Base class cho mỗi màn hình/popup UI |
| `UILayer` | Enum định nghĩa thứ tự layer (Background → AlwaysOnTop) |
| `UIInputData` | Dữ liệu truyền vào khi mở view (có thể kế thừa) |
| `UIOutputData` | Dữ liệu trả về khi đóng view (có thể kế thừa) |

## Setup

1. Tạo một **GameObject** trong scene với các component: `Canvas`, `CanvasScaler`, `GraphicRaycaster`
2. Gắn component **UIManager** vào GameObject đó
3. Cấu hình trong Inspector:
   - `UI Layer Orders` — Thêm các layer cần dùng, có thể override sorting order
   - `Resources Root Folder` — Đường dẫn thư mục chứa UI prefab trong Resources (VD: `UI/`)
   - `Ref Path Addressable` — Đường dẫn Addressable cho UI prefab (VD: `Assets/Addressables/UI`)

## Tạo UIView Mới

```csharp
public class SettingsView : UIView
{
    // Dữ liệu đầu vào
    public class InputData : UIInputData
    {
        public float currentVolume;
    }

    // Dữ liệu trả về khi đóng
    public class OutputData : UIOutputData
    {
        public float newVolume;
    }

    private float _volume;

    public override void OnOpen(UIInputData inputData)
    {
        base.OnOpen(inputData); // Bắt buộc gọi base

        if (inputData is InputData data)
            _volume = data.currentVolume;
    }

    public override UIOutputData OnClose()
    {
        base.OnClose(); // Bắt buộc gọi base
        return new OutputData { newVolume = _volume };
    }
}
```

## API Reference

### Mở View

```csharp
// === Resources (đồng bộ) ===
UIManager.OpenResources("SettingsView");
UIManager.OpenResources<SettingsView>("SettingsView", new SettingsView.InputData { currentVolume = 0.8f });

// === Resources (bất đồng bộ) ===
var view = await UIManager.OpenResourcesAsync("SettingsView");
var view = await UIManager.OpenResourcesAsync<SettingsView>("SettingsView", inputData);

// === Addressables (bất đồng bộ, cần #define ADDRESSABLES) ===
var view = await UIManager.OpenAddressables("SettingsView");
var view = await UIManager.OpenAddressables<SettingsView>("SettingsView", inputData, controlInteract: true);
```

### Đóng View

```csharp
// Đóng theo ID (cache lại, không destroy)
UIManager.Close("SettingsView");

// Đóng theo instance
UIManager.Close(viewInstance);

// Đóng và destroy (giải phóng bộ nhớ)
UIManager.Close(viewInstance, destroy: true);

// Đóng view trên cùng của một layer
UIManager.CloseCurrentInLayer(UILayer.Popup);

// Đóng tất cả view trong layer (có thể exclude một số view)
UIManager.CloseAllInLayer(UILayer.Popup, ignoreList: keepTheseViews);

// Đóng tất cả view (toàn bộ layer)
UIManager.CloseAll();

// Tự đóng từ bên trong view
CloseSelf();
CloseSelf(destroy: true);
```

### Cache View (Pre-load)

```csharp
// Cache trước để mở nhanh hơn (không cần load lại)
await UIManager.TryCacheViewResources("SettingsView");
await UIManager.TryCacheViewAddressables("SettingsView");

// Force cache thêm một instance nữa (cho trường hợp cần mở nhiều bản)
await UIManager.TryCacheViewResources("ItemSlot", forceCacheMultiple: true);

// Xóa cached view
UIManager.DestroyCachedViews("SettingsView");
```

### Truy Vấn View

```csharp
// Kiểm tra view có đang mở không
if (UIManager.IsSpecificViewShown("SettingsView", out var view)) { ... }

// Lấy view đang mở theo ID
var view = UIManager.GetOpenedView("SettingsView");
var view = UIManager.GetOpenedView<SettingsView>("SettingsView");

// Lấy view trên cùng
var topView = UIManager.GetTopmostOpenedView();
var topPopup = UIManager.GetTopmostOpenedViewInLayer(UILayer.Popup);

// Kiểm tra layer có view nào không
if (UIManager.IsAnyOpenedViewInLayer(UILayer.Popup)) { ... }

// Lấy tất cả view đang mở
var allViews = UIManager.GetOpenedViews(null);           // Tất cả
var allPopups = UIManager.GetOpenedViewsInLayer(UILayer.Popup); // Theo layer
```

### Interact Control

Cơ chế register-based: nhiều nguồn có thể yêu cầu disable, phải tất cả đều enable mới thực sự mở lại.

```csharp
// Disable interact (chặn mọi raycast trên tất cả layer)
UIManager.DisableInteract(register: this);

// Enable interact (khi tất cả register đã enable)
UIManager.EnableInteract(register: this);

// Force enable (bỏ qua tất cả register)
UIManager.EnableInteract(force: true);

// Kiểm tra
if (UIManager.Interactable) { ... }

// Lắng nghe sự kiện
UIManager.OnInteractableChanged += (isInteractable) => { ... };
```

### Events

```csharp
// Khi bất kỳ view nào được mở
UIManager.OnOpenedView += (UIView view, UIInputData data) => { ... };

// Khi bất kỳ view nào được đóng
UIManager.OnClosedView += (UIView view, UIOutputData data) => { ... };
```

### Utilities

```csharp
// Kiểm tra pointer có đang trên UI không (hỗ trợ cả touch và mouse)
if (UIManager.IsPointerOverUIObject()) { ... }

// Truy cập root canvas
var canvas = UIManager.I.RootCanvas;

// Thay đổi camera
UIManager.UICamera = myCamera;
```

## Addressables

Khi dùng Addressables, cần:
1. Thêm `ADDRESSABLES` vào **Scripting Define Symbols**
2. Cấu hình `Ref Path Addressable` trong Inspector
3. Khi view được destroy và không còn cached, Addressable asset sẽ tự động unload

```csharp
// Unload thủ công
await UIManager.UnloadAddressableUI("SettingsView");

// Force unload (đóng view đang mở + destroy cached)
await UIManager.UnloadAddressableUI("SettingsView", force: true);
```

## Caching & Performance

- View khi **Close** (không destroy) sẽ được **deactivate và push vào Stack cache**
- Lần mở tiếp theo sẽ **pop từ cache**, không cần load/instantiate lại
- Dùng `TryCacheView` để pre-load trong loading screen, giúp gameplay mượt hơn
- `IsPointerOverUIObject()` tái sử dụng `PointerEventData` và `List<RaycastResult>` để giảm GC allocation

## Pause Game

`UIView` hỗ trợ tự động pause game khi mở:
- Cấu hình `_pauseGameStatus` trên prefab (mặc định)
- Override bằng `UIInputData.pauseStatus` khi gọi Open
- Sử dụng `PauseGameHandler.Pause/Unpause` nội bộ
