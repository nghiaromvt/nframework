# SoundManager

Hệ thống quản lý âm thanh cho Unity, hỗ trợ **BGM** (Background Music) và **SFX** (Sound Effects) với AudioMixer, pooling, và tích hợp save/load trạng thái.

## Kiến Trúc

```
SoundManager (SingletonMono, ISaveable)
├── BGM Emitter         ← Dedicated SoundEmitter cho nhạc nền
├── SFX Emitter Pool    ← Queue<SoundEmitter> cho hiệu ứng âm thanh
│   ├── SoundEmitter 0
│   ├── SoundEmitter 1
│   └── ... (mặc định 10)
├── AudioMixer
│   ├── BgmMixerGroup
│   └── SfxMixerGroup
└── Sound Group Cache   ← Dictionary<string, SoundGroupSO>
```

## Các Lớp Liên Quan

| Lớp | Mô tả |
|---|---|
| `SoundManager` | Singleton quản lý toàn bộ audio lifecycle |
| `SoundEmitter` | Component phát âm thanh, quản lý AudioSource |
| `SoundGroupSO` | ScriptableObject chứa danh sách audio clips theo nhóm |
| `SoundEntry` | Một entry trong SoundGroup: key + AudioClip + SoundPlaySettings |
| `SoundPlaySettings` | Cấu hình phát: volume, loop, pitch, fade, overlap handling |
| `EAudioOverlapType` | Enum: `None`, `StopPrevious`, `Skip` |

## Setup

1. Tạo **GameObject** trong scene, gắn component **SoundManager**
2. Cấu hình trong Inspector:
   - `Audio Mixer` — Gán AudioMixer asset (phải có exposed params: `BgmVolume`, `BgmChildVolume`, `SfxVolume`, `SfxChildVolume`)
   - `Bgm Mixer Group` — AudioMixerGroup cho BGM
   - `Sfx Mixer Group` — AudioMixerGroup cho SFX
   - `Sound Emitter Count` — Số lượng SFX emitter trong pool (mặc định 10)
   - `Resources Root Folder` — Đường dẫn Resources cho SoundGroup
   - `Ref Path Addressable` — Đường dẫn Addressable cho SoundGroup

3. Tạo **SoundGroupSO** qua menu: `Create > NFramework > Sound > SoundGroup`

## Tạo SoundGroup

```
Assets/
└── SoundGroups/
    ├── CommonSFX.asset      ← SoundGroupSO
    ├── BattleSFX.asset
    └── MainBGM.asset
```

Trong Inspector của `SoundGroupSO`:
- Thêm các `SoundEntry` vào danh sách `soundEntries`
- Mỗi entry gồm: `key` (string unique), `clip` (AudioClip), `playSettings`
- Key tự động lấy từ tên AudioClip khi kéo clip vào
- Hệ thống tự validate trùng key giữa các SoundGroup

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
// Từ Resources
await SoundManager.CacheSoundGroupResources("CommonSFX");

// Từ Addressables (cần #define ADDRESSABLES)
await SoundManager.CacheSoundGroupAddressables("CommonSFX");

// Xóa cache
SoundManager.ClearSoundGroup("CommonSFX");    // Xóa 1 group
SoundManager.ClearAllSoundGroup();             // Xóa tất cả
```

### Phát SFX

```csharp
// Phát bằng key (từ SoundGroup đã cache)
string guid = SoundManager.PlaySfx("button_click");

// Phát với callback khi dừng
SoundManager.PlaySfx("explosion", onStop: () => Debug.Log("Done"));

// Phát với custom settings
var settings = new SoundPlaySettings
{
    volume = 0.5f,
    pitch = 1.2f,
    loop = false,
    overlapType = EAudioOverlapType.StopPrevious,
    fadeTime = 0.3f
};
string guid = SoundManager.PlaySfx("footstep", settings);

// Phát trực tiếp bằng AudioClip
string guid = SoundManager.PlaySfx(myClip, settings);
```

### Phát BGM

```csharp
// Phát bằng key
SoundManager.PlayBgm("main_theme");

// Phát với custom settings
SoundManager.PlayBgm("battle_theme", new SoundPlaySettings
{
    volume = 0.7f,
    loop = true,
    fadeTime = 1f
});

// Phát trực tiếp bằng AudioClip
SoundManager.PlayBgm(bgmClip, settings);
```

### Dừng Âm Thanh

```csharp
// Dừng BGM
SoundManager.StopBGM();
SoundManager.StopBGM(fadeTime: 1.5f);  // Fade out trước khi dừng

// Dừng SFX theo guid (trả về từ PlaySfx)
bool stopped = SoundManager.Stop(guid);

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
// Toggle trạng thái (tích hợp ISaveable, tự lưu)
SoundManager.BgmStatus = true;   // Bật BGM
SoundManager.SfxStatus = false;  // Tắt SFX

// Lắng nghe thay đổi
SoundManager.OnBgmStatusChanged += (bool isOn) => { ... };
SoundManager.OnSfxStatusChanged += (bool isOn) => { ... };

// Pause/Unpause toàn bộ AudioListener
SoundManager.Pause();
SoundManager.Unpause();
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
    float volume = 1f;           // 0.0 → 1.0
    bool loop;                   // Lặp lại
    float pitch = 1f;            // Tốc độ phát
    bool ignorePause;            // Phát cả khi AudioListener.pause = true
    EAudioOverlapType overlapType; // Xử lý overlap
    float fadeTime;              // Thời gian fade in khi bắt đầu phát
}
```

## ISaveable Integration

SoundManager tích hợp `ISaveable` để lưu trạng thái BGM/SFX:

```csharp
// SaveKey: "SoundManager"
// Dữ liệu lưu:
public class SaveData
{
    public bool bgmStatus = true;
    public bool sfxStatus = true;
}
```

Khi dùng với `LocalSaveManager`, trạng thái bật/tắt BGM/SFX sẽ tự động được lưu và khôi phục.

## AudioMixer Setup

AudioMixer cần expose 4 parameters (đúng tên):

| Parameter | Mô tả |
|---|---|
| `BgmVolume` | Volume gốc của BGM group (dùng cho on/off) |
| `BgmChildVolume` | Volume con của BGM (dùng cho fade) |
| `SfxVolume` | Volume gốc của SFX group (dùng cho on/off) |
| `SfxChildVolume` | Volume con của SFX (dùng cho fade) |

**Cấu trúc AudioMixer gợi ý:**
```
Master
├── BGM
│   └── BgmChild (exposed: BgmChildVolume)
│   (exposed: BgmVolume)
└── SFX
    └── SfxChild (exposed: SfxChildVolume)
    (exposed: SfxVolume)
```

## Pooling

- SFX emitters được quản lý bằng **Queue-based pool** (mặc định 10)
- Khi emitter phát xong, tự động trả về pool qua `ReturnSoundEmitter()`
- Nếu pool hết, log warning và trả về `null`
- BGM có **emitter riêng**, không dùng pool

## Lưu Ý

- **Phải cache SoundGroup trước khi phát** — `PlaySfx("key")` sẽ log error nếu key chưa được cache
- Khi `ClearSoundGroup`, tất cả âm thanh đang phát thuộc group đó sẽ bị dừng
- `fadeTime` trong `SoundPlaySettings` là fade-in khi phát; `StopBGM(fadeTime)` là fade-out khi dừng
