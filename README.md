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
*   [**PrimeTween**](https://github.com/KyryloKuzyk/PrimeTween) - High-performance, zero-allocation animation library.
*   **Newtonsoft.Json** - Required for robust JSON serialization (especially within the Save system).

---

## 🛠️ Installation Guide

### 1. Adding NFramework to Your Project

You can integrate NFramework into your Unity project using one of the following methods:

**Method A: Git Submodule (Recommended)**
If your target project uses Git, you can add this repository as a submodule. This allows you to easily pull updates from the framework:
```bash
git submodule add <repository-url> Assets/NFramework
```

**Method B: Manual Copy**
1. Create a folder named `NFramework` inside your target Unity project's `Assets` folder.
2. Copy the contents of the `Assets` folder from this repository into your newly created `Assets/NFramework` folder.

### 2. Installing Newtonsoft.Json (via UPM)

NFramework uses Unity's official wrapper for Newtonsoft.Json. You must install it via the Unity Package Manager (UPM):

1. Open Unity Editor.
2. Go to **Window > Package Manager**.
3. Click the **`+`** drop-down button in the top left corner.
4. Select **Add package by name...**.
5. Enter the following name: `com.unity.nuget.newtonsoft-json`
6. Click **Add**.

### 3. Core Dependencies (UniTask & PrimeTween)

Make sure you have both **UniTask** and **PrimeTween** imported into your project. You can install them via the Asset Store, UPM, or their respective GitHub repositories. 

### 4. Hidden Packages (`.HiddenFromEditor`)

This repository contains some useful third-party packages and tools stored inside the `.HiddenFromEditor` folder. These are hidden from Unity's default Asset database to reduce clutter and keep the workspace clean.

To install a package from the `.HiddenFromEditor` folder:

1. Open the **Package Manager** in Unity.
2. Click the **`+`** drop-down button.
3. Select **Add package from disk...**.
4. Navigate to the `.HiddenFromEditor/[Package_Name]` directory inside your project root.
5. Select the `package.json` file to install it.

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
