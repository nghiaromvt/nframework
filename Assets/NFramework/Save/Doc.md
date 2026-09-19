# Save System

Save System is a high-performance, modular, and secure save/load framework for Unity under namespace `NFramework`. It is decoupled into interface-driven components, uses **OdinSerializer** for binary serialization, supports **AES encryption**, embeds **SHA-256 checksums** for integrity checks, provides **schema version migrations**, writes files **atomically with automatic backups**, and supports background processing via **UniTask**.

---

## 1. Key Features

- **Decoupled Interface Architecture**: Every step (serialization, encryption, checksum, storage, migration) is abstracted behind interfaces (`ISaveService`, `ISaveable`, `ISerializer`, `IEncryptor`, `IChecksumGenerator`, `IStorageService`, `IMigration`).
- **Atomic File Operations**: Protects save data against game crashes and sudden power loss by writing to `.tmp` first, rotating previous save to `.backup`, and finalizing via atomic rename.
- **Embedded Checksum Header**: 37-byte file header includes a 4-byte magic signature (`NFSV`), 1-byte format version, and 32-byte raw SHA-256 checksum of the payload. Eliminates out-of-sync issues between data and checksum files.
- **Auto-Recovery Fallback**: If the main save file is corrupted or fails checksum verification, the system automatically falls back to `.backup`.
- **Background Async Operations**: Captures state on the main thread, then offloads serialization, encryption, and disk I/O to background threadpool threads via **UniTask**.
- **Schema Migration Pipeline**: Per-entity schema versioning. Supports sequential migration chains (e.g., `v1 -> v2 -> v3`) when legacy saves are loaded.
- **Dirty Tracking**: Only writes to disk when at least one registered `ISaveable` has modified state (`IsDirty == true`).
- **Mobile Interrupt Handling**: Automatically saves on application pause/focus loss with built-in deduplication guards (`_autoSaveOnInterrupt`).
- **ServiceLocator Integration**: Registered into `ServiceLocator` as `ISaveService` and `LocalSaveManager`.
- **Editor Tooling**: Visual inspection and management window built with Odin Inspector (`Tools > NFramework > Save Editor`).

---

## 2. Architecture & File Layout

```
Save System (LocalSaveManager : ISaveService, IApplicationFocusListener, IApplicationPauseListener, IApplicationQuitListener, IDisposable)
├── ServiceLocator Registry    ← ServiceLocator.Register<ISaveService>(localSaveManager)
├── ApplicationLifecycle Hooks ← Automatically registered for Pause, Focus, Quit
├── Event Hooks                ← OnBeforeSave, OnAfterSave, OnBeforeLoad, OnAfterLoad
├── Registered Entities        ← List<ISaveable>
│   ├── PlayerData             ← ISaveable (SchemaVersion, IsDirty, Migrations)
│   ├── InventoryData          ← ISaveable
│   └── SettingsData           ← ISaveable
├── Serializer                 ← ISerializer (OdinSerializerHandler)
├── Encryptor                  ← IEncryptor (AesEncryptor, AES-256-CBC)
├── Checksum Generator         ← IChecksumGenerator (Sha256ChecksumGenerator)
└── Storage Service            ← IStorageService
    ├── FileStorageService     ← Desktop/Mobile (.tmp -> .sav + .backup)
    └── PlayerPrefsStorageService ← WebGL fallback (Base64)
```

### Save File Binary Structure

Every save file follows a fixed header layout followed by the encrypted/serialized payload:

```
[0..3]   Magic Bytes      : 4 Bytes  ('N', 'F', 'S', 'V')
[4]      Format Version   : 1 Byte   (Current = 1)
[5..36]  SHA-256 Hash     : 32 Bytes (Raw hash of payload)
[37..N]  Payload Data     : Variable (AES encrypted SaveFileData)
```

---

## 3. Core Interfaces

| Interface | Purpose | Key Methods / Properties |
|---|---|---|
| `ISaveService` | Main entry point for save/load operations | `Save()`, `SaveAsync()`, `Load()`, `LoadAsync()`, `Register()`, `UnRegister()`, `DeleteSave()` |
| `ISaveable` | Implemented by components/data models that persist state | `SaveKey`, `SchemaVersion`, `IsDirty`, `Migrations`, `CaptureState()`, `RestoreState()`, `SetClean()` |
| `ISerializer` | Serializes objects to/from byte arrays | `Serialize<T>()`, `Deserialize<T>()` |
| `IEncryptor` | Handles payload encryption and decryption | `Encrypt(byte[])`, `Decrypt(byte[])` |
| `IChecksumGenerator` | Computes and validates data integrity hashes | `ComputeHash()`, `VerifyHash()`, `GenerateChecksum()`, `VerifyChecksum()` |
| `IStorageService` | Reads and writes files to disk or storage | `WriteAtomic()`, `Read()`, `ReadBackup()`, `Exists()`, `HasBackup()`, `Delete()` |
| `IMigration` | Transforms data between schema versions | `FromVersion`, `ToVersion`, `Migrate(byte[] oldData, ISerializer serializer)` |

---

## 4. Setup & Quick Start

### Step 1: Initialize LocalSaveManager (POCO)
Initialize `LocalSaveManager` at your game's entry point / composition root:
```csharp
using NFramework;
using NFramework.Save;

// Create instance (automatically hooks into ApplicationLifecycle)
var saveManager = new LocalSaveManager(
    autoLoad: true,
    autoSaveOnQuit: true,
    autoSaveOnInterrupt: true
);

// Register to ServiceLocator
ServiceLocator.Register<ISaveService>(saveManager);
```

### Step 2: Access via ServiceLocator
```csharp
using NFramework;

// Resolving the save service
ISaveService saveService = ServiceLocator.Get<ISaveService>();

// Or safely try get
if (ServiceLocator.TryGet(out ISaveService service))
{
    service.Save();
}
```

---

## 5. Implementing `ISaveable`

Create your game state class and implement `ISaveable`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using NFramework;

public class PlayerProfile : MonoBehaviour, ISaveable
{
    [Serializable]
    public class Data
    {
        public string PlayerName = "Player";
        public int Level = 1;
        public int Coins = 0;
    }

    [SerializeField] private Data _data = new();

    public string SaveKey => "PlayerProfile";
    public int SchemaVersion => 1;
    public bool IsDirty { get; private set; }
    public IReadOnlyList<IMigration> Migrations => null;

    private void Start()
    {
        // Register to the save service
        if (ServiceLocator.TryGet(out ISaveService saveService))
        {
            saveService.Register(this);
        }
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet(out ISaveService saveService))
        {
            saveService.UnRegister(this);
        }
    }

    public void AddCoins(int amount)
    {
        _data.Coins += amount;
        IsDirty = true; // Mark dirty so save will write to disk
    }

    public byte[] CaptureState(ISerializer serializer)
    {
        return serializer.Serialize(_data);
    }

    public void RestoreState(ISerializer serializer, byte[] data)
    {
        _data = serializer.Deserialize<Data>(data) ?? new Data();
    }

    public void SetClean()
    {
        IsDirty = false;
    }
}
```

---

## 6. Schema Migrations

When game updates modify data structure, implement `IMigration` to upgrade legacy save files safely.

### Migration Example: Version 1 to Version 2

```csharp
[Serializable]
public class PlayerDataV1
{
    public string Name;
}

[Serializable]
public class PlayerDataV2
{
    public string Name;
    public int Level; // Added in v2
}

public class PlayerDataMigrationV1ToV2 : IMigration
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public byte[] Migrate(byte[] oldData, ISerializer serializer)
    {
        PlayerDataV1 v1 = serializer.Deserialize<PlayerDataV1>(oldData);
        PlayerDataV2 v2 = new PlayerDataV2
        {
            Name = v1.Name,
            Level = 1 // Default migration value
        };
        return serializer.Serialize(v2);
    }
}
```

In your `ISaveable` implementation:
```csharp
public int SchemaVersion => 2;

public IReadOnlyList<IMigration> Migrations { get; } = new List<IMigration>
{
    new PlayerDataMigrationV1ToV2()
};
```

The `SchemaMigrationRunner` automatically links migration steps sequentially (e.g. `v1 -> v2 -> v3`).

---

## 7. Lifecycle Events

`ISaveService` provides lifecycle hooks for UI feedback or custom prep/post-processing:

```csharp
ISaveService saveService = ServiceLocator.Get<ISaveService>();

saveService.OnBeforeSave += () => Debug.Log("Saving started...");
saveService.OnAfterSave += () => Debug.Log("Saving completed!");

saveService.OnBeforeLoad += () => Debug.Log("Loading started...");
saveService.OnAfterLoad += () => Debug.Log("Loading completed!");
```

---

## 8. Asynchronous Operations (UniTask)

Use `SaveAsync()` and `LoadAsync()` to avoid frame drops during large data serialization or disk writes:

```csharp
using Cysharp.Threading.Tasks;

public async UniTask SaveGameAsync()
{
    ISaveService saveService = ServiceLocator.Get<ISaveService>();
    await saveService.SaveAsync();
}
```

- **Thread Safety**: State capturing (`CaptureState`) executes synchronously on the Unity main thread.
- **Background Execution**: Serialization, encryption, hash generation, and atomic file I/O execute on worker threads via `UniTask.RunOnThreadPool`.

---

## 9. Odin Editor Window

Access via Unity Menu: **`Tools > NFramework > Save Editor`**.

- **Runtime Tab**:
  - Displays registered `ISaveable` entities in real-time during Play Mode.
  - Shows each entity's `SaveKey`, `SchemaVersion`, `IsDirty` flag, and Type.
  - Buttons to manually trigger `Save()` and `Load()`.
- **File Management Tab**:
  - Displays the current save directory path.
  - **Backup Management**:
    - **Capture Backup**: Saves/copies current save file (`gamedata.sav`) to a custom named backup file (`<name>.sav`). Automatically saves runtime state first if running in Play Mode.
    - **Load Backup**: Overwrites the active save file (`gamedata.sav`) with the selected backup file from the dropdown. Disabled during Play Mode.
    - **Delete Backup**: Deletes the selected backup file after confirmation.
  - **File Overview**: Lists all existing save files, file sizes, and last modified timestamps.
  - **Open Save Folder**: Opens directory in file explorer (`Finder` / `Explorer`).
  - **Delete All Save Files**: Cleans all save and backup files safely.
