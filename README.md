# NFramework

A custom Unity framework featuring modular systems (UI, Save, Sound, etc.) and a collection of helper classes to support rapid game development.

## 🌟 Features

*   **Modular Architecture**: Isolated and manageable systems for UI, Save/Load, Sound, and more.
*   **Rapid Development**: A comprehensive collection of helper classes and extension methods to speed up everyday Unity tasks.
*   **Modern Async**: Fully integrated with `UniTask` to replace traditional Coroutines for better performance and readability.
*   **High-Performance Animations**: Utilizes `PrimeTween` for zero-allocation, fast, and easy tweening.

## 📦 Dependencies

NFramework relies on a few essential external libraries to function optimally. 

*   [**UniTask**](https://github.com/Cysharp/UniTask) - Provides an efficient allocation-free async/await integration for Unity.
*   [**PrimeTween**](https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960) - High-performance, zero-allocation animation library.
*   [**Odin Inspector and Serializer**](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) - High-performance, zero-allocation animation library.
*   [**Newtonsoft.Json**](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.0/manual/index.html) - Required for robust JSON serialization (especially within the Save system).

---

## 🏗️ Core Modules

### 🎨 UI System
A robust view-based UI manager. Handles screen navigation, popups, overlays, and complex UI states efficiently.

### 💾 Save System
A modular save/load architecture utilizing `Newtonsoft.Json`. Supports multiple save profiles, automatic serialization, and easy data management.

### 🔊 Sound System
An easy-to-use audio manager for playing BGM, SFX, and UI sounds with support for object pooling, volume control, and fading.

### 🧰 Helpers & Utilities
A wide array of static helper classes and extension methods for standard Unity types (`Transform`, `GameObject`, `Vector3`, etc.) to streamline your coding process.

---

## 🏷️ Versioning

- **Current Version**: `1.0.0`
- **In C#**: [`NFrameworkVersion.VERSION`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/NFrameworkVersion.cs)
- **In Unity Editor**: Menu `Tools` > `NFramework` > `About NFramework`
- **Package Manifest**: [`package.json`](file:///Users/nghia/Projects/nframework/Assets/NFramework/package.json)
