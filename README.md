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

## 🛠️ Installation Guide

### 1. Install Dependencies

Make sure you have the following dependencies installed in your Unity project:
- **Newtonsoft.Json** - [UPM](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.0/manual/index.html)
- **UniTask** - [GitHub](https://github.com/Cysharp/UniTask)
- **PrimeTween** - [Asset Store](https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960)
- **Odin Inspector and Serializer** - [Asset Store](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)

### 2. Adding NFramework to Your Project

You can integrate NFramework into your Unity project using one of the following methods:

**Method A: Manual Copy**
1. Create a folder named `NFramework` inside your target Unity project's `Assets` folder.
2. Copy the contents of the `Assets` folder from this repository into your newly created `Assets/NFramework` folder.

**Method B: Git Submodule**

If your target project uses Git, you can add this repository as a submodule. This allows you to easily pull updates from the framework:
```bash
git submodule add https://github.com/nghiaromvt/nframework.git Assets/NFramework
```

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
