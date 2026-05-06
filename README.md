# NFramework

A lightweight Unity framework for rapid game development.

## Features

- 🎨 **UI System** — Layer-based UI management with caching, Addressables support, and editor tools for rapid view creation
- 🏊 **Object Pooling** — Efficient object pool system to reduce runtime allocations
- 🔄 **State Machine** — Flexible state machine pattern for game logic
- 🎵 **Sound Manager** — BGM/SFX management with AudioMixer, pooling, and save/load integration
- 💾 **Local Save** — Simple local data persistence with ISaveable interface
- 📡 **Observer Pattern** — Event-driven communication between systems
- ⏱️ **Delay Action Invoker** — UniTask-based delayed action system
- 🧩 **Singleton** — MonoBehaviour and plain C# singleton base classes
- 🔧 **Extensions & Helpers** — Rich set of utility extensions and helper classes
- 📱 **Vibration** — Haptic feedback support

## Prerequisites

NFramework depends on the following packages. **Install them first** before installing NFramework:

### UniTask
```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

### PrimeTween
```
https://github.com/KyryloKuzyk/PrimeTween.git
```

> Install via **Window → Package Manager → + → Add package from git URL**

## Installation

After installing the prerequisites, add NFramework via git URL:

```
https://github.com/nghiaromvt/nframework.git?path=Assets/RootPackage
```

Or add directly to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.nghia.nframework": "https://github.com/nghiaromvt/nframework.git?path=Assets/RootPackage"
  }
}
```

## Quick Start

### UI System

```csharp
using NFramework;

// Open a view from Resources
UIManager.OpenResources("MainMenu");

// Open with input data
UIManager.OpenResources<SettingsView>("SettingsView", new SettingsView.InputData
{
    currentVolume = 0.8f
});

// Close a view
UIManager.Close("SettingsView");
```

### Sound Manager

```csharp
using NFramework;

// Cache sound group first
await SoundManager.CacheSoundGroupResources("CommonSFX");

// Play SFX
SoundManager.PlaySfx("button_click");

// Play BGM
SoundManager.PlayBgm("main_theme");
```

### Object Pool

```csharp
using NFramework;

// Get from pool
var obj = PoolManager.Get("BulletPool");

// Return to pool
PoolManager.Return(obj);
```

## Documentation

- [UI System](Assets/RootPackage/Documentation~/UIManager.md)
- [Sound Manager](Assets/RootPackage/Documentation~/SoundManager.md)

## License

MIT
