# NFramework

A custom Unity framework featuring modular systems (UI, Save, Sound, etc.) and a collection of helper classes to support rapid game development.

---

## 🚀 Installation (UPM via Git URL)

NFramework is distributed as a clean **Unity Package Manager (UPM)** package containing only core framework modules and can be installed directly from GitHub into any Unity project.

### 1. Prerequisites (Must be installed in host project)

Before installing NFramework, ensure the following packages/assets are installed in your host project:

*   [**UniTask**](https://github.com/Cysharp/UniTask) (Install via Git URL or OpenUPM):
    ```text
    https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
    ```
*   [**Odin Inspector & Serializer**](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) (Unity Asset Store)
*   [**PrimeTween**](https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960) (Unity Asset Store)

*(Note: Newtonsoft.Json and UGUI are declared as package dependencies in `package.json` and will be resolved automatically by Unity).*

---

### 2. Install via Unity Package Manager Window

1. First, make sure you have added **UniTask** via Git URL in Package Manager.
2. In Unity, open **Window** > **Package Manager**.
3. Click the **`+`** icon in the top-left corner and select **Add package from git URL...**.
4. Enter one of the following URLs:

*   **Stable Release (Recommended)**:
    ```text
    https://github.com/nghiaromvt/nframework.git?path=Assets/NFramework#v1.0.0
    ```
*   **Latest (main branch)**:
    ```text
    https://github.com/nghiaromvt/nframework.git?path=Assets/NFramework
    ```

---

### 3. Install via `Packages/manifest.json`

Alternatively, you can add UniTask and NFramework directly to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.neo.nframework": "https://github.com/nghiaromvt/nframework.git?path=Assets/NFramework#v1.0.0"
  }
}
```

---

## 📦 Dependencies & Optional Features

| Dependency | Type | Source |
| :--- | :--- | :--- |
| **UniTask** | Prerequisite | `https://github.com/Cysharp/UniTask.git` (Git URL or OpenUPM) |
| **Odin Inspector** | Prerequisite | Unity Asset Store |
| **PrimeTween** | Prerequisite | Unity Asset Store |
| **Newtonsoft.Json** | Automatic | `com.unity.nuget.newtonsoft-json` (Unity Registry) |
| **UGUI / TextMeshPro** | Automatic | `com.unity.ugui` (Unity Registry) |
| **Addressables** | **Optional** | Automatically activated if `com.unity.addressables` is present via `#if ADDRESSABLES`. |

---

## 🏗️ Core Modules

### 🎨 UI System
A robust view-based UI manager. Handles screen navigation, popups, overlays, and complex UI states efficiently.

### 💾 Save System
A modular save/load architecture utilizing `Newtonsoft.Json` or `OdinSerializer`. Supports multiple save profiles, automatic serialization, and easy data management.

### 🔊 Sound System
An easy-to-use audio manager for playing BGM, SFX, and UI sounds with support for object pooling, volume control, and fading.

### 🧰 Helpers & Utilities
A wide array of static helper classes and extension methods for standard Unity types (`Transform`, `GameObject`, `Vector3`, etc.) to streamline your coding process.

---

## 🎮 Sandbox & Examples

The framework package itself (`Assets/NFramework`) is kept completely clean of demo scenes and test assets. 

For developers exploring or contributing to NFramework, practical sample scenes and code demonstrations are maintained in the [`Assets/Sandbox`](file:///Users/nghia/Projects/nframework/Assets/Sandbox) directory of this repository:
*   **`Assets/Sandbox/UI`**: UI navigation, popups, and view examples.
*   **`Assets/Sandbox/Save`**: Save/load profiling and serialization tests.
*   **`Assets/Sandbox/Sound`**: Audio playback, sound groups, and mixer setup.

---

## 🏷️ Versioning

*   **Current Version**: `1.0.0`
*   **In C#**: [`NFrameworkVersion.VERSION`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/NFrameworkVersion.cs)
*   **In Unity Editor**: Menu `Tools` > `NFramework` > `About NFramework`
*   **Package Manifest**: [`package.json`](file:///Users/nghia/Projects/nframework/Assets/NFramework/package.json)
