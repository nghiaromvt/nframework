# UIManager

Hệ thống quản lý UI phân lớp (layer-based) cho Unity, hỗ trợ cả **Resources** và **Addressables**, tích hợp mượt mà với cả **Legacy Input Manager** và **New Input System**.

## Kiến Trúc & Cấu Trúc Module

UI System được tách thành module độc lập với 2 Assembly Definitions:
- `NFramework.UI.asmdef` (Runtime)
- `NFramework.UI.Editor.asmdef` (Editor)

```
Assets/NFramework/UI/
├── Runtime/
│   ├── NFramework.UI.asmdef
│   ├── UIManager.cs
│   ├── UIView.cs
│   ├── UIViewExtensions.cs
│   ├── IViewAnimation.cs
│   ├── FadeViewAnimation.cs
│   ├── SafeArea.cs
│   ├── InvertedMaskImage.cs
│   └── GreyscaleHandler/
│       ├── UIGreyscaleHandler.cs
│       ├── GraphicGreyscaleHandler.cs
│       ├── ImageGreyscaleHandler.cs
│       ├── RawImageGreyscaleHandler.cs
│       ├── TMPGreyscaleHandler.cs
│       └── GroupObjectGreyscaleHandler.cs
├── Editor/
│   ├── NFramework.UI.Editor.asmdef
│   ├── UIConfigSO.cs
│   ├── UIValidationHelper.cs
│   ├── UIEditorWindow.cs
│   ├── UIViewCreator.cs
│   ├── UIViewAssetPostprocessor.cs
│   └── UIScriptDefineEditor.cs
└── Doc/
    └── UIManager.md
```

### Phân lớp Layer trong Scene:

```
UIManager (SingletonMono)
├── Background Layer    ← Canvas + GraphicRaycaster + CanvasGroup
├── Screen Layer        ← Quản lý các màn hình chính (Screen/Page)
├── Popup Layer
├── Loading Layer
└── AlwaysOnTop Layer
```

Mỗi layer là một `RectTransform` con chứa `Canvas`, `GraphicRaycaster`, và `CanvasGroup`, được tạo tự động trong `Awake()` dựa trên cấu hình `_uiLayerOrders`.

## Các Lớp Liên Quan

| Lớp | Mô tả |
|---|---|
| `UIManager` | Singleton quản lý toàn bộ vòng đời và tương tác UI |
| `UIView` | Base class cho mỗi màn hình/popup UI (không cần input) |
| `UIView<TInput>` | Generic base class cho màn hình/popup UI có input dữ liệu strongly typed |
| `UIViewExtensions` | Các extension method tiện ích cho `UIView` (`WaitForOpenComplete`, `WaitForCloseComplete`) |
| `UIViewAssetPostprocessor` | Tự động cập nhật và validate Key của UIView khi di chuyển file hoặc thay đổi Addressable |
| `UILayer` | Enum định nghĩa thứ tự layer (Background → AlwaysOnTop) |
| `UIInputData` | Base class dữ liệu truyền vào khi mở view (chứa `PauseStatus`, có thể kế thừa) |
| `InvertedMaskImage` | Image hỗ trợ inverted mask stencil không gây leak bộ nhớ |
| `GraphicGreyscaleHandler<T>` | Generic base class cho các component chuyển màu xám (greyscale) |

---

## Setup

1. Tạo một **GameObject** trong scene với các component: `Canvas`, `CanvasScaler`, `GraphicRaycaster`.
2. Gắn component **UIManager** vào GameObject đó.
3. Cấu hình trong Inspector:
   - `UI Layer Orders` — Thêm các layer cần dùng, có thể override sorting order.
   - *Không cần thiết lập Root Folder*: Hệ thống tự động xác định đường dẫn tài nguyên dựa trên vị trí tương đối trong `Resources/` hoặc Addressable Address. File UI prefab có thể đặt ở bất kỳ thư mục nào trong dự án.

---

## Quy Trình Tạo 1 UI Mới (Step-by-Step Flow)

Có 2 cách để thêm một UI mới vào dự án: **Tự động qua Editor Tool (Khuyên dùng)** hoặc **Thủ công**.

```
[B1: Cấu hình UIConfigSO (Namespace & Save Path)]
          │
          ▼
[B2: Tạo View (Tự động qua Tool hoặc Viết Script + Prefab)]
          │
          ▼
[B3: Thiết kế Prefab & Gắn Component (UIView, Animation, Button)]
          │
          ▼
[B4: Sinh hằng số UIDefine (Generate Script Define)]
          │
          ▼
[B5: Gọi mở View trong Code (UIManager.OpenResources / OpenAddressables)]
```

### Cách 1: Tự Động Bằng `UIViewCreator` (Khuyên Dùng)

1. **Bước 1 — Mở Editor Window**:
   - Chọn menu: **`Tools > NFramework > UI > Window`**.
2. **Bước 2 — Điền thông tin View**:
   - **View Name**: Tên của View (VD: `ShopPopup`).
   - **Target Folder**: Chọn thư mục lưu Prefab (hỗ trợ bất kỳ thư mục nào trong dự án).
   - **Script Name**: Tự động đồng bộ theo View Name.
   - **Script Folder Path**: Chọn folder muốn lưu file `.cs` (VD: `Scripts/UI/Shop`).
   - **UI Layer**: Chọn layer tương ứng (`Screen`, `Popup`, `Loading`, `Background`, v.v.).
   - Tick chọn **Generate Script Define** (để tự động sinh constant vào `UIDefine.cs`).
3. **Bước 3 — Bấm Create**:
   - Tool tự động:
     1. Tạo file C# script kế thừa `UIView`.
     2. Tạo Prefab trống gắn sẵn component trong thư mục cấu hình (`_targetFolder`).
     3. Tự động gắn script vào Prefab sau khi Unity compile xong.
     4. Tự động cập nhật hằng số vào file `UIDefine.cs`.
4. **Bước 4 — Thiết kế Prefab**:
   - Mở Prefab vừa sinh trong thư mục Prefab.
   - Thêm CanvasGroup, hình ảnh, text, nút bấm.
   - Kéo thả các reference serialized field vào Script Component.
   - Thêm animation đóng mở (như component `FadeViewAnimation`) nếu muốn.
5. **Bước 5 — Sử dụng**:
   - Mở view: `UIManager.OpenResources(UIDefine.ShopPopup);` (hoặc generic `UIManager.OpenResources<ShopPopup>();`).

---

### Cách 2: Thủ Công

1. **Bước 1 — Viết C# Script**:
   - Nếu View cần dữ liệu đầu vào: Kế thừa `UIView<TInput>` (strongly typed, không cần ép kiểu).
   - Nếu View tĩnh không cần dữ liệu: Kế thừa `UIView` (override `OnViewOpened(UIInputData inputData)`).
2. **Bước 2 — Tạo Prefab**:
   - Tạo GameObject UI với `RectTransform` (stretch full parent hoặc căn giữa).
   - Gắn component script vừa tạo, chọn `UI Layer` tương ứng trong Inspector.
   - Gắn `CanvasGroup` và `FadeViewAnimation` (nếu có hiệu ứng đóng mở).
   - Đặt Prefab vào thư mục Resources (VD: `Assets/.../Resources/UI/ShopPopup.prefab`) hoặc gán Addressable Address.
3. **Bước 3 — Sinh UIDefine**:
   - Chọn menu **`Tools > NFramework > UI > Generate Script Define`** để cập nhật constant string cho view mới.
4. **Bước 4 — Gọi trong Game**:
   - Dùng `await UIManager.OpenResourcesAsync<ShopPopup>();` hoặc `UIManager.OpenResources(UIDefine.ShopPopup);`.

---

## Code Mẫu UIView

### 1. View có dữ liệu đầu vào (Kế thừa `UIView<TInput>`)

```csharp
public class SettingsView : UIView<SettingsView.InputData>
{
    public class InputData : UIInputData
    {
        public float currentVolume;
    }

    private float _volume;

    protected override void OnViewOpened(InputData data)
    {
        base.OnViewOpened(data);

        if (data != null)
            _volume = data.currentVolume;
    }

    protected override void OnOpenAnimationCompleted()
    {
        // Gọi khi Open Animation kết thúc (ViewStatus chuyển sang EViewStatus.Open)
    }

    protected override void OnViewClosed()
    {
        // Gọi khi View hoàn tất đóng (sau animation nếu có, trước khi deactive/destroy)
    }
}
```

### 2. View không cần dữ liệu (Kế thừa `UIView`)

```csharp
public class MainMenuView : UIView
{
    protected override void OnViewOpened(UIInputData inputData)
    {
        base.OnViewOpened(inputData);
        // Khởi tạo menu tĩnh
    }
}
```

### View Status & Animation Flow

Mỗi `UIView` theo dõi trạng thái qua enum `EViewStatus`:
- `Closed`: Đã đóng hoàn toàn.
- `Opening`: Đang mở, đang chạy open animation (nếu có).
- `Open`: Mở xong hoàn toàn.
- `Closing`: Đang đóng, đang chạy close animation (nếu có).

```csharp
// Kiểm tra trạng thái:
if (view.ViewStatus == EViewStatus.Open) { ... }

// Await open animation:
await view.WaitForOpenComplete();

// Đóng view (Close/CloseSelf đã tự động await animation đóng hoàn tất):
await view.CloseSelf();
// Hoặc bên thứ 3 có thể chờ view tự đóng:
await view.WaitForCloseComplete();
```

Để gắn animation cho view:
- Thêm một hoặc nhiều component animation (ví dụ `FadeViewAnimation`, `ScaleViewAnimation`, hoặc custom component implement `IOpenViewAnimation` / `ICloseViewAnimation`) vào cùng GameObject của `UIView`.
- Cả hai interface đều sử dụng mô hình async và có thuộc tính `BlockInteract`:
  - `IOpenViewAnimation`: `bool BlockInteract { get; }` và `UniTask PlayOpen(CancellationToken cancellationToken = default);`
  - `ICloseViewAnimation`: `bool BlockInteract { get; }` và `UniTask PlayClose(CancellationToken cancellationToken = default);`
- `UIView.Initialize` sẽ tự động thu thập danh sách qua `GetComponents`. Khi mở/đóng, tất cả animation tương ứng sẽ được chạy song song (`UniTask.WhenAll`).
- Nếu có bất kỳ animation nào có `BlockInteract == true`, `UIView` sẽ tự động quản lý chặn tương tác (`UIManager.DisableInteract(this)`) và mở lại khi tất cả animation hoàn tất (kèm cơ chế fail-safe an toàn).


---

## API Reference

### Mở View

Mọi phương thức async đều hỗ trợ tham số `CancellationToken cancellationToken = default`. Nếu không truyền, hệ thống sẽ tự động dùng `destroyCancellationToken` của UIManager để hủy an toàn khi chuyển scene/destroy.

> [!NOTE]
> Khi mở bất kỳ view nào thuộc layer **`UILayer.Screen`**:
> - Nếu Screen đó **đang mở hoặc đang trong quá trình mở** (`Open` hoặc `Opening`), UIManager sẽ **skip** việc mở lại và trả về instance đang mở.
> - Việc đóng screen cũ khi chuyển đổi sang screen mới được điều phối tường minh (explicit) bởi Caller / State Machine (ví dụ: `await UIManager.Close(...)` trước hoặc sau khi mở screen mới) thay vì tự động đóng ngầm, giúp kiểm soát hoàn toàn thứ tự transition và animation.

```csharp
// === Resources (đồng bộ) ===
UIManager.OpenResources<SettingsView>(UIDefine.SettingsPopup);
UIManager.OpenResources<SettingsView>(UIDefine.SettingsPopup, new SettingsView.InputData { currentVolume = 0.8f });
UIManager.OpenResources(UIDefine.SettingsPopup);

// === Resources (bất đồng bộ) ===
var view = await UIManager.OpenResourcesAsync<SettingsView>(UIDefine.SettingsPopup, cancellationToken: ct);
var view = await UIManager.OpenResourcesAsync<SettingsView>(UIDefine.SettingsPopup, inputData, cancellationToken: ct);

// === Addressables (bất đồng bộ, cần #define ADDRESSABLES) ===
var view = await UIManager.OpenAddressables<SettingsView>(UIDefine.SettingsPopup, cancellationToken: ct);
var view = await UIManager.OpenAddressables<SettingsView>(UIDefine.SettingsPopup, inputData, controlInteract: true, cancellationToken: ct);
```

### Đóng View

Toàn bộ các API đóng View là bất đồng bộ (`UniTask`), hỗ trợ tham số `CancellationToken` (mặc định lấy `destroyCancellationToken`). Sự kiện `OnClosedView` sẽ được bắn và View sẽ được deactive/cache chỉ sau khi animation đóng đã hoàn tất trọn vẹn.

```csharp
// Đóng theo ID (cache lại vào stack, không destroy)
await UIManager.Close("SettingsView");

// Đóng theo instance
await UIManager.Close(viewInstance);

// Đóng và destroy (giải phóng hoàn toàn bộ nhớ)
await UIManager.Close(viewInstance, destroy: true);

// Đóng view trên cùng của một layer
await UIManager.CloseCurrentInLayer(UILayer.Popup);

// Đóng tất cả view trong layer (chạy song song và đợi hoàn tất, có thể ignore một số view)
await UIManager.CloseAllInLayer(UILayer.Popup, ignoreList: keepTheseViews);

// Đóng tất cả view (toàn bộ các layer)
await UIManager.CloseAll();

// Tự đóng từ bên trong view (await hoặc .Forget() trong button click event)
await CloseSelf();
await CloseSelf(destroy: true);

// Gọi trong Button onClick:
_closeButton.onClick.AddListener(() => CloseSelf().Forget());
```

### Cache View (Pre-load)

```csharp
// Cache trước để mở tức thì
await UIManager.TryCacheViewResources("SettingsView", cancellationToken: ct);
await UIManager.TryCacheViewAddressables("SettingsView", cancellationToken: ct);

// Force cache thêm instance mới
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

// Kiểm tra layer có view nào đang mở không
if (UIManager.IsAnyOpenedViewInLayer(UILayer.Popup)) { ... }

// Lấy danh sách view đang mở
var allViews = UIManager.GetOpenedViews(null);
var allPopups = UIManager.GetOpenedViewsInLayer(UILayer.Popup);
```

### Interact Control

Cơ chế register-based: nhiều nguồn có thể yêu cầu lock interact, hệ thống chỉ mở lại khi tất cả register đã enable (hoặc force enable).

```csharp
// Disable interact (chặn raycast toàn bộ layer)
UIManager.DisableInteract(register: this);

// Enable interact
UIManager.EnableInteract(register: this);

// Force enable (bỏ qua mọi register đang giữ)
UIManager.EnableInteract(force: true);

// Trạng thái interact hiện tại
if (UIManager.Interactable) { ... }

// Lắng nghe sự kiện thay đổi tương tác
UIManager.OnInteractableChanged += isInteractable => { ... };
```

### Utilities & Input System Support

Hàm `IsPointerOverUIObject()` hỗ trợ đầy đủ và an toàn cho cả **Legacy Input Manager** và **New Input System** (`com.unity.inputsystem`):

```csharp
// Kiểm tra pointer/touch có đang nằm trên UI không
if (UIManager.IsPointerOverUIObject())
{
    // Pointer đang tương tác với UI, bỏ qua xử lý raycast gameplay
}

// Truy cập Root Canvas & UI Camera
var rootCanvas = UIManager.I.RootCanvas;
UIManager.UICamera = myCamera;
```

---

## Editor Tools

Toàn bộ công cụ UI được đặt dưới menu chuẩn: **`Tools > NFramework > UI`**.

```
Tools/
└── NFramework/
    ├── UI/
        ├── Window                      → UIEditorWindow (browse + create views)
        ├── Config                      → UIConfigSO (cấu hình thư mục views & namespace)
        ├── Generate Script Define      → Generate UIDefine.cs (const string)
        └── Locate Script Define        → Ping UIDefine.cs trong Project
    └── Save Editor                     → Save System Editor
```

### UIDefine — Hằng số sinh tự động

`UIScriptDefineEditor` sinh các constant dạng `public const string`, hỗ trợ switch-case và tối ưu compile-time:

```csharp
// This file is auto-generated.
// Do not modify this file manually.

namespace MyGame
{
    public static class UIDefine
    {
        // Popup
        public const string SettingsPopup = "SettingsPopup";
        public const string ShopPopup = "ShopPopup";

        // Screen
        public const string MainMenu = "MainMenu";
    }
}
```

Sử dụng:
```csharp
UIManager.OpenResources(UIDefine.SettingsPopup);
```

---

## Sample Scene

Scene mẫu tại: `Assets/NFramework/UI/Sample/UISample.unity`

Bao gồm:
- **`SampleMenuView`**: Kế thừa `UIView` thuộc layer `UILayer.Screen`, hiển thị menu với nút "Open Popup", nút "Action", và dòng text status.
- **`SamplePopupView`**: Kế thừa `UIView` thuộc layer `UILayer.Popup`, tích hợp `FadeViewAnimation`, nền tối mờ (dimmer) chặn raycast, hiển thị dialog modal và nút "Close".
- **`UISampleTest`**: MonoBehaviour trong scene để tự động mở Screen (Menu) lúc khởi động và cung cấp các nút kiểm thử nhanh qua Inspector (`OpenMenu`, `OpenPopup`, `ClosePopup`, `CloseAll`).
- **Prefabs trong `Editor/Resources/UI/`**: Toàn bộ UI prefab mẫu được đặt tại `Assets/NFramework/UI/Sample/Editor/Resources/UI/` để vừa hỗ trợ `Resources.Load` khi test trong Editor, vừa tự động loại trừ (exclude) hoàn toàn khỏi build thực tế của dự án.
- **`UISampleConfigSO.asset`**: Tách riêng cấu hình sample (`Assets/NFramework/UI/Sample/UISampleConfigSO.asset`) để tránh xung đột với `UIConfigSO` của dự án thật. Hệ thống Editor tự động ưu tiên file `UIConfigSO` không nằm trong thư mục `Sample`.
- Menu Editor: **`Tools > NFramework > UI > Setup UISample Scene`** để tự động tái tạo hoặc cấu hình lại scene và prefab mẫu bất kỳ lúc nào.

---

## Changelog

### 2026-10-23
- **`UIView.Initialize`**: Chuyển thành non-virtual method để bảo toàn logic cốt lõi (ID, flag `_initialized`, cache animations).
- **`UIView.OnInitialized`**: Thêm lifecycle hook `protected virtual void OnInitialized()` được gọi ở cuối hàm `Initialize`.
- **`UIView.OnViewClosed`**: Đổi tên từ `OnCloseAnimationCompleted` thành `protected virtual void OnViewClosed()` cho đối xứng trực tiếp với `OnViewOpened`, là hook dọn dẹp chính thức khi view đóng hoàn tất.
- **`UIView.OnDisable`**: Giữ `private` để đảm bảo logic dọn dẹp cốt lõi của framework (`CancelOpenAnimation()`, `UnblockInteract()`) không bị subclass ghi đè làm mất.


