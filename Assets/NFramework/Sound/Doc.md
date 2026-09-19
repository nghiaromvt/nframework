# SoundManager

Hệ thống quản lý âm thanh cho Unity, hỗ trợ **BGM** (Background Music) và **SFX** (Sound Effects) với AudioMixer, pooling (`PoolService`), `GameLoop` (`IUpdatable`), và quản lý phân tán SoundGroup.

## Kiến Trúc

```
SoundManager (SingletonMono)
├── BGM Emitter         ← Dedicated SoundEmitter cho nhạc nền (không pool)
├── SFX Emitter Pool    ← PoolService.Register<SoundEmitter>(_soundEmitterPrefab)
│   ├── SoundEmitter 0  (IPoolable, IUpdatable on-demand via GameLoop)
│   ├── SoundEmitter 1
│   └── ... (maxSize = _soundEmitterCount, channel stealing khi đầy)
├── AudioMixer
│   ├── BgmMixerGroup
│   └── SfxMixerGroup
└── Sound Group Cache   ← Dictionary<string, SoundGroupSO> (Resources / Addressables)
```

## Các Lớp Liên Quan

| Lớp | Mô tả |
|---|---|
| `SoundManager` | Singleton quản lý toàn bộ audio lifecycle, mixer, và pool |
| `SoundEmitter` | Component phát âm thanh, implement `IPoolable` và `IUpdatable` (đăng ký `GameLoop` khi phát) |
| `SoundGroupSO` | ScriptableObject phân tán, tự động tính `key` theo relative Resources path hoặc Addressables address |
| `SoundEntry` | Một entry trong SoundGroup: key + AudioClip + SoundPlaySettings |
| `SoundPlaySettings` | Cấu hình phát: volume, loop, pitch, fade, overlap handling |
| `EAudioOverlapType` | Enum: `None`, `StopPrevious`, `Skip` |

## Setup

1. Tạo **GameObject** hoặc dùng prefab `Assets/NFramework/Sound/Prefab/SoundManager.prefab`
2. Cấu hình trong Inspector:
   - `Sound Emitter Prefab` — Gán prefab `Assets/NFramework/Sound/Prefab/SoundEmitter.prefab`
   - `Sound Emitter Count` — Số lượng SFX emitter trong pool (mặc định 10)
   - `Audio Mixer` — Gán AudioMixer asset (phải có exposed params: `BgmVolume`, `BgmChildVolume`, `SfxVolume`, `SfxChildVolume`)
   - `Bgm Mixer Group` — AudioMixerGroup cho BGM
   - `Sfx Mixer Group` — AudioMixerGroup cho SFX
   - `Initialize On Awake` — Tự động khởi tạo khi Awake

## Tạo & Quản Lý SoundGroup Phân Tán

`SoundGroupSO` có thể được đặt ở bất kỳ thư mục feature nào trong `Assets/`:
- **Resources**: Đặt file trong thư mục `Resources/...` (ví dụ `Assets/Features/Audio/Resources/Audio/BattleSoundGroup.asset`). `key` sẽ tự động là đường dẫn tương đối không bao gồm đuôi file (`Audio/BattleSoundGroup`).
- **Addressables**: Đánh dấu asset là Addressable. `key` sẽ tự động là address của asset trong AddressableAssetSettings.
- **Fail-Fast Validation**: Tên file asset `SoundGroupSO` phải duy nhất toàn bộ project. Không được vừa nằm trong `Resources/` vừa đánh dấu Addressable.

Menu Editor:
- Mở cửa sổ quản lý: `Tools > NFramework > Sound > Window` (hiển thị danh sách flat tất cả SoundGroup trong project, hỗ trợ tạo mới tại thư mục tùy chọn).
- Sinh code định nghĩa hằng số: `Tools > NFramework > Sound > Generate Script Define` (sinh file `SoundDefine.cs`).

## API Reference

### Khởi Tạo

```csharp
// Tự động khởi tạo trong Awake nếu _initializeOnAwake = true
// Hoặc gọi thủ công:
await SoundManager.Initialize();
```

### Cache SoundGroup

**Phải cache SoundGroup trước khi phát âm thanh.**

```csharp
// Từ Resources (dùng relative path)
await SoundManager.CacheSoundGroupResources(SoundDefine.SoundGroupKey.SAMPLE_SOUND_GROUP);

// Từ Addressables (cần #define ADDRESSABLES, dùng address)
await SoundManager.CacheSoundGroupAddressables(SoundDefine.SoundGroupKey.SAMPLE_SOUND_GROUP);

// Xóa cache
SoundManager.ClearSoundGroup(SoundDefine.SoundGroupKey.SAMPLE_SOUND_GROUP); // Xóa 1 group
SoundManager.ClearAllSoundGroup();                                          // Xóa tất cả
```

### Phát SFX

```csharp
// Phát bằng key (từ SoundGroup đã cache)
int handle = SoundManager.PlaySfx(SoundDefine.SoundEntryKey.SFX_OPEN_CHEST);

// Phát với callback khi dừng
SoundManager.PlaySfx(SoundDefine.SoundEntryKey.SFX_OPEN_CHEST, onStop: () => Debug.Log("Done"));

// Phát với custom settings
var settings = new SoundPlaySettings
{
    volume = 0.5f,
    pitch = 1.2f,
    loop = false,
    overlapType = EAudioOverlapType.StopPrevious,
    fadeTime = 0.3f
};
int handle = SoundManager.PlaySfx(SoundDefine.SoundEntryKey.SFX_OPEN_CHEST, settings);
```

### Phát BGM

```csharp
// Phát bằng key
SoundManager.PlayBgm(SoundDefine.SoundEntryKey.BGM_INGAME);

// Phát với custom settings
SoundManager.PlayBgm(SoundDefine.SoundEntryKey.BGM_INGAME, new SoundPlaySettings
{
    volume = 0.7f,
    loop = true,
    fadeTime = 1f
});
```

### Dừng Âm Thanh

```csharp
// Dừng BGM
SoundManager.StopBGM();
SoundManager.StopBGM(fadeTime: 1.5f);  // Fade out trước khi dừng

// Dừng SFX theo handle (trả về từ PlaySfx, SoundManager.INVALID_HANDLE nếu không phát)
bool stopped = SoundManager.Stop(handle);

// Dừng tất cả
SoundManager.StopAll();                     // Bao gồm BGM
SoundManager.StopAll(includeBgm: false);    // Chỉ SFX
```

### Mixer Volume Control

```csharp
// Đặt volume BGM mixer (0.0001 → 1.0)
SoundManager.SetBgmMixerVolume(0.8f);
SoundManager.SetBgmMixerVolume(0.5f, fadeTime: 1f);  // Fade

// Đặt volume SFX mixer
SoundManager.SetSFXMixerVolume(1f);
SoundManager.SetSFXMixerVolume(0f, fadeTime: 0.5f);   // Fade to mute
```

### Bật/Tắt Âm Thanh

```csharp
// Toggle trạng thái
SoundManager.BgmStatus = true;   // Bật BGM
SoundManager.SfxStatus = false;  // Tắt SFX

// Lắng nghe thay đổi
SoundManager.OnBgmStatusChanged += (bool isOn) => { ... };
SoundManager.OnSfxStatusChanged += (bool isOn) => { ... };
```

## EAudioOverlapType

Xử lý khi cùng một AudioClip đang phát mà được yêu cầu phát lại:

| Type | Hành vi |
|---|---|
| `None` | Cho phép phát chồng (overlap) — nhiều instance cùng lúc |
| `StopPrevious` | Dừng tất cả instance đang phát của clip đó, rồi phát mới |
| `Skip` | Bỏ qua yêu cầu phát mới nếu clip đang phát |

## SoundPlaySettings

```csharp
[Serializable]
public class SoundPlaySettings
{
    public float volume = 1f;           // 0.0 → 1.0
    public bool loop;                   // Lặp lại
    public float pitch = 1f;            // Tốc độ phát
    public bool ignorePause;            // Phát cả khi AudioListener.pause = true
    public EAudioOverlapType overlapType; // Xử lý overlap
    public float fadeTime;              // Thời gian fade in khi bắt đầu phát
}
```

## Pooling & Channel Stealing

- SFX emitters được quản lý thông qua **`PoolService`** (`PoolScope.Global`).
- `SoundEmitter` implement `IPoolable` và `IUpdatable`. Khi bắt đầu phát (`Play`), emitter tự động đăng ký vào `GameLoop`. Khi kết thúc hoặc dừng, emitter tự động hủy đăng ký khỏi `GameLoop`, trả về pool và reset trạng thái.
- Khi số lượng emitter hoạt động đạt giới hạn `_soundEmitterCount`, kỹ thuật **Channel Stealing** tự động dừng emitter cũ nhất để nhường kênh cho âm thanh mới.
- BGM có **emitter riêng biệt**, không qua pool.

## Sample Scene

- Scene: `Assets/NFramework/Sound/Sample/SoundSample.unity`
- Script: `Assets/NFramework/Sound/Sample/Scripts/SoundSampleTest.cs`
