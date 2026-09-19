# TSV Configuration System

The TSV Configuration System is an editor-friendly, fail-fast data pipeline for Unity under namespace `NFramework`. It converts Tab-Separated Values (TSV) from local disk files or remote web endpoints (such as Google Sheets) directly into strongly typed Unity `ScriptableObject` instances with Odin Inspector integration.

---

## 1. Key Features

- **Generic POCO Data Mapping**: Deserializes rows into plain C# objects (`where T : class, new()`) without requiring boilerplate serialization schemas.
- **Flexible Column Attribute**: Maps table headers to fields or properties using `[TSVColumn("HeaderName")]`. Automatically falls back to member names (case-insensitive, ignoring leading `_`).
- **Rich Type Support**: Supports primitives (`int`, `long`, `float`, `double`, `decimal`, `bool`, `string`), Enums (by name or number), and delimited collections (`T[]`, `List<T>` separated by `,` or `;`).
- **Google Sheets URL Auto-Normalization**: Automatically converts standard Google Sheets URLs (e.g. `https://docs.google.com/spreadsheets/d/{id}/edit#gid={gid}`) into direct TSV export endpoints.
- **Fail-Fast Error Handling**: Strictly conforms to `GEMINI.md`. Skips comment lines (`#`) and empty rows, but immediately logs an error and halts execution if type conversion fails, preventing corrupt game configurations.
- **Configurable Disk Caching**: Allows caching fetched web TSV files locally (`_enableCache`), defaulting to `Assets/DataCache/TSV/{SOName}.tsv`.
- **Centralized Odin Editor Window**: Includes `TsvManagerEditorWindow` under `Tools > NFramework > TSV Table Manager` featuring dynamic category tabs, search filtering, inline path edits with automatic dirty marking, pinging, and batch fetching with a progress bar and summary dialog.

---

## 2. Architecture & Class Hierarchy

```
TSV Configuration Pipeline
├── TSVColumnAttribute              ← [TSVColumn("name")] decoration for POCO fields/properties
├── TsvParser                       ← Static parsing, type conversion, URL normalization, web/file fetcher
├── TsvConfigSOBase                 ← Non-generic base ScriptableObject (Metadata, Odin buttons, Fetch abstraction)
│   └── TsvConfigSO<T>              ← Generic container (List<T> _items, IReadOnlyList<T> Items, OnAfterFetched)
│       └── ConcreteConfigSO        ← Game-specific table (e.g., ItemTableSO, QuestTableSO)
└── TsvManagerEditorWindow          ← Odin Editor Window (Tools > NFramework > TSV Table Manager)
```

### Core Components

| Class / Struct | Role | Key APIs / Properties |
|---|---|---|
| [`TSVColumnAttribute`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/TSVConfigSO/TSVColumnAttribute.cs) | Custom header mapping | `string Name` |
| [`TsvParser`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/TSVConfigSO/TsvParser.cs) | Parsing & network/file fetching | `Parse<T>()`, `NormalizeGoogleSheetUrl()`, `FetchContent()`, `FetchContentAsync()`, `SaveToCache()` |
| [`TsvConfigSOBase`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/TSVConfigSO/TsvConfigSOBase.cs) | Non-generic base for discovery & tooling | `Category`, `TsvPath`, `EnableCache`, `CachePath`, `LastSyncedTime`, `Fetch()`, `FetchAsync()`, `OpenSource()` |
| [`TsvConfigSO<T>`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/TSVConfigSO/TsvConfigSO.cs) | Generic data holder | `IReadOnlyList<T> Items`, `OnAfterFetched()` |
| [`TsvManagerEditorWindow`](file:///Users/nghia/Projects/nframework/Assets/NFramework/Core/Editor/TSVConfigSO/TsvManagerEditorWindow.cs) | Batch management tool | `Tools > NFramework > TSV Table Manager` |

---

## 3. Usage Guide

### Step 1: Define the POCO Item Class

Decorate fields or properties using `[TSVColumn("Header")]` if header names differ from field names. Private/protected fields marked with `[SerializeField]` are supported.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using NFramework;

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public class ItemData
{
    [TSVColumn("Item ID")]
    public int Id;

    [TSVColumn("Item Name")]
    public string Name;

    [SerializeField]
    private float _price;
    public float Price => _price;

    public bool IsAvailable;
    public ItemRarity Rarity;
    public List<string> Tags; // Parsed from comma- or semicolon-separated text
}
```

### Step 2: Create the ScriptableObject Subclass

Inherit from `TsvConfigSO<T>`. Optionally override `OnAfterFetched()` to build runtime lookup dictionaries or index data.

```csharp
using System.Collections.Generic;
using UnityEngine;
using NFramework;

[CreateAssetMenu(fileName = "ItemTableSO", menuName = "Game/Configs/Item Table")]
public class ItemTableSO : TsvConfigSO<ItemData>
{
    private readonly Dictionary<int, ItemData> _lookup = new();

    public ItemData GetItemById(int id) => _lookup.GetValueOrDefault(id);

    protected override void OnAfterFetched()
    {
        base.OnAfterFetched();
        _lookup.Clear();
        foreach (var item in Items)
        {
            _lookup[item.Id] = item;
        }
    }
}
```

### Step 3: Configure Source & Fetch

1. Right-click in the Project view and select **Create > Game > Configs > Item Table**.
2. In the Inspector:
   - **Category**: Set group name (e.g., `Items`, `Economy`, `Quests`).
   - **Tsv Path**: Paste a Google Sheets share URL or local file path (e.g., `Assets/Data/items.tsv`).
   - **Enable Cache**: Check if you want to store a local copy of downloaded web data.
3. Click **Fetch / Sync** to download and populate the table immediately.
4. Click **Open Source** to open the Google Sheet in your web browser or reveal the local file on disk.

### Step 4: Runtime Access

Access items through the read-only `Items` property:

```csharp
[SerializeField] private ItemTableSO _itemTable;

private void Start()
{
    foreach (var item in _itemTable.Items)
    {
        Debug.Log($"Loaded {item.Name} with Price {item.Price}");
    }
}
```

---

## 4. Google Sheets Setup & URL Normalization

`TsvParser` automatically handles Google Sheets URL conversions. Ensure your sheet sharing settings are set to **"Anyone with the link can view"**.

Supported URL patterns:
- Standard browser URL:
  `https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit#gid={GRID_ID}`
- Browser URL with query parameters:
  `https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit?gid={GRID_ID}`
- Base sheet URL (automatically targets tab 0):
  `https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit`
- Direct export URL:
  `https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=tsv&gid={GRID_ID}`

All Google Sheets URLs are normalized internally into:
`https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=tsv&gid={GRID_ID}`

---

## 5. Odin Editor Window (`TSV Table Manager`)

Open via menu: **Tools > NFramework > TSV Table Manager**

### Window Capabilities
- **Category Toolbar**: Dynamically scans all `TsvConfigSOBase` assets and generates tabs (`All`, `Default / Uncategorized`, and any custom category strings).
- **Search Bar**: Live filters table entries by asset name or TSV path.
- **Select All / Deselect All**: Toggles checkboxes for visible filtered entries only.
- **Inline Editing**: Allows updating `TsvPath` directly from the window; automatically invokes `EditorUtility.SetDirty`.
- **Ping**: Selects and highlights the target asset in the Project window.
- **Status Icons**: Each entry displays a real-time vector status icon with hover tooltips:
  - `Idle` (Gray circle): Not yet fetched in the current session.
  - `Fetching` (Blue rotating arrows): Actively fetching data.
  - `Success` (Green checkmark): Fetch completed successfully.
  - `Failed` (Red X): Fetch failed (hover tooltip reveals the error message).
- **Non-Blocking Asynchronous Fetching**: Fetching runs asynchronously via `FetchAsync()` and `HttpClient`, keeping the Unity Editor fully interactive without modal lockups.
- **In-Window Progress Bar**: Renders a progress bar directly within the editor window showing progress percentage and the currently syncing table, accompanied by a **Cancel Fetch** button.
- **In-Window Fetch Logs Console**: Embedded scrollable console displaying timestamped, color-coded log entries (`[INFO]`, `[SUCCESS]`, `[ERROR]`) with auto-scroll, clear, and copy-to-clipboard actions.
- **Individual Fetch**: Asynchronously syncs an individual table, immediately updating its status icon, Last Synced timestamp, and window logs.
- **Batch Fetch Selected**: Asynchronously syncs all checked tables in sequence with real-time progress and logging.
