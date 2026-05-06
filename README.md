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

## Dependencies

- **UniTask & PrimeTween**: Installed via the `manifest.json` setup below.
- **Odin Inspector**: NFramework heavily relies on [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89200) for its editor tools and serialization. Since it is a paid asset, you must purchase and import it manually into your project from the Unity Asset Store.

## Installation

To install NFramework along with UniTask and PrimeTween, open your project's `Packages/manifest.json` file and add the following lines:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.kyrylokuzyk.primetween": "1.3.8",
    "com.nghia.nframework": "https://github.com/nghiaromvt/nframework.git?path=Assets/RootPackage"
  },
  "scopedRegistries": [
    {
      "name": "npm",
      "url": "https://registry.npmjs.org/",
      "scopes": [
        "com.kyrylokuzyk"
      ]
    }
  ]
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
