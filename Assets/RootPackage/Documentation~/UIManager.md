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

> 💡 Có thể dùng **UIViewCreator** để tự động tạo script + prefab (xem phần [Editor Tools](#editor-tools)).

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

    // Khởi tạo view — gọi 1 lần khi view được load/instantiate
    // ID và IsFromResources có private setter, chỉ gán được qua Initialize()
    public override void Initialize(string id, bool isFromResources = false)
    {
        base.Initialize(id, isFromResources); // Bắt buộc gọi base (có guard chống gọi lại)
        // Setup one-time resources ở đây
    }

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

## Editor Tools

### Cấu hình NFrameworkConfigSO

Mở menu `NFramework > Open Config` để mở/tạo asset cấu hình. Cần điền:

| Field | Mô tả | Ví dụ |
|---|---|---|
| `scriptDefineNamespace` | Namespace cho code generated | `MyGame` |
| `uiViewsFolderPath` | Folder chứa prefab UIView (relative to `Assets/`) | `Prefabs/UI` |
| `uiScriptDefineSavePath` | Folder lưu `UIDefine.cs` generated | `Scripts/Generated` |

> ⚠️ Phải cấu hình `uiViewsFolderPath` trước khi dùng các tool bên dưới.

### Tạo View bằng UIViewCreator

1. Mở menu `NFramework > UI > Window`
2. Chọn tab **"Create New View"** ở sidebar trái
3. Điền thông tin:

| Field | Mô tả |
|---|---|
| `View Name` | Tên view / tên prefab. VD: `SettingsPopup` |
| `Script Name` | Tên class C# (auto-sync theo View Name, có thể đổi) |
| `Script Folder Path` | Folder lưu file `.cs` (relative to `Assets/`) |
| `UI Layer` | Layer: `Background`, `Menu`, `Popup`, `Loading`, `AlwaysOnTop` |
| `Generate Script Define` | Tick để auto-gen `UIDefine.cs` sau khi tạo |

4. Bấm nút **Create** (nút xanh lá lớn)

**Khi bấm Create, tool sẽ:**
1. Validate — kiểm tra trùng key, file script đã tồn tại
2. Sinh file `.cs` kế thừa `UIView` (có sẵn override `Initialize`, `OnOpen`, `OnClose`)
3. Tạo prefab stretch-full trong `uiViewsFolderPath`
4. Instantiate prefab instance trong scene hiện tại
5. Lưu pending info vào `EditorPrefs`
6. Sau khi Unity **recompile** xong → `[InitializeOnLoadMethod]` tự động:
   - Tìm type vừa tạo trong assemblies
   - `AddComponent` script vào prefab
   - Set `_uiLayer`, `key`, `defineKeyConstName`
   - Save prefab asset
   - Nếu tick `Generate Script Define` → auto-gen `UIDefine.cs`

**Script generated mẫu:**

```csharp
using UnityEngine;
using NFramework;

namespace MyGame
{
    public class SettingsPopup : UIView
    {
        public override void Initialize(string id, bool isFromResources = false)
        {
            base.Initialize(id, isFromResources);
        }

        public override void OnOpen(UIInputData inputData)
        {
            base.OnOpen(inputData);
        }

        public override UIOutputData OnClose()
        {
            return base.OnClose();
        }
    }
}
```

**Tạo thủ công (không dùng tool):**
1. Tạo script kế thừa `UIView`
2. Tạo prefab, attach script, đặt vào `uiViewsFolderPath`
3. Chạy `NFramework > UI > Generate Script Define` để update `UIDefine.cs`

### UIEditorWindow — Quản lý View

Mở `NFramework > UI > Window`:

| Thao tác | Cách làm |
|---|---|
| Browse views | Sidebar trái hiển thị tất cả UIView prefabs, nhóm theo `Layer X / ViewName` |
| Inspect view | Click vào view → inspector bên phải |
| Locate prefab | Chọn view → toolbar **Locate** |
| Delete view | Chọn view → toolbar **Delete** → xác nhận |
| Generate UIDefine | Toolbar **Generate ScriptDefine** |
| Locate UIDefine | Toolbar **Locate ScriptDefine** |

### UIDefine — Script Define tự sinh

Chạy `NFramework > UI > Generate Script Define` để sinh file `UIDefine.cs`:

```csharp
// This file is auto-generated.
// Do not modify this file manually.

namespace MyGame
{
    public static class UIDefine
    {
        // Popup
        public static string SettingsPopup = "SettingsPopup";
        public static string ShopPopup = "ShopPopup";
        // Menu
        public static string MainMenu = "MainMenu";
    }
}
```

Sử dụng:

```csharp
UIManager.OpenResources(UIDefine.SettingsPopup);
var view = await UIManager.OpenAddressables<SettingsPopup>(UIDefine.SettingsPopup, inputData);
```

### Menu Reference

```
NFramework/
├── Open Config                     → Mở NFrameworkConfigSO
└── UI/
    ├── Window                      → UIEditorWindow (browse + create views)
    ├── Generate Script Define      → Generate UIDefine.cs
    └── Locate Script Define        → Ping UIDefine.cs
```
