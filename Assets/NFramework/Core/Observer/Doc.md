# Observer & Reactive Event System

The `NFramework.Core` Observer module provides a zero-allocation, type-safe event bus architecture and observable data collections for Unity games.

It eliminates string-based event mapping, boxing/unboxing overhead, and defensive fallback logic, strictly adhering to fail-fast principles, modern C# conventions, and zero-allocation runtime performance.

---

## 1. Architecture & Components

```
+-----------------------------------------------------------+
|                        EventBus<T>                        |
|   - Type-safe, zero GC event dispatcher (where T : IEvent)|
|   - RAII subscription lifecycle via EventSubscription<T>  |
|   - SubsystemRegistration reset support for disabled reload|
+-----------------------------+-----------------------------+
                              | registers to
                              v
+-----------------------------------------------------------+
|                   EventBusResetRegistry                   |
|   - Static domain reload lifecycle tracker                |
|   - Clears static handlers on SubsystemRegistration       |
+-----------------------------------------------------------+

+-----------------------------+ +---------------------------+
|     ObservableValue<T>      | |     ObservableList<T>     |
| - [Serializable]            | | - [Serializable]          |
| - Value change notification | | - Implements IList<T>     |
| - Previous value callback   | | - Granular change events  |
| - Immediate UI binding      | | - Struct enumerator (0 GC)|
+-----------------------------+ +---------------------------+
```

### Core Types

- **`IEvent`**: Marker interface for all event payloads. Events are typically defined as `readonly struct` to guarantee zero heap allocation upon dispatch.
- **`EventBus<T>`**: High-performance generic event bus for event type `T : IEvent`. Uses static delegate multicasting with `O(1)` dispatch and no dictionary lookups.
- **`EventSubscription<T>`**: Lightweight `IDisposable` struct returned by `EventBus<T>.Subscribe()` enabling automatic unsubscription via `using` or member disposables.
- **`EventBusResetRegistry`**: Internal static registry automatically resetting static event handlers on domain reload / subsystem registration.
- **`ObservableValue<T>`**: Serializable observable container wrapping value `T`. Dispatches `OnValueChanged` and `OnValueChangedWithPrevious`, with support for immediate subscription execution for UI binding.
- **`ObservableList<T>`**: Serializable collection implementing `IList<T>` and `IReadOnlyList<T>`. Dispatches granular `ObservableListEvent<T>` (with `Add`, `Remove`, `Replace`, `Clear`, `Reset` event types) and provides a struct enumerator for zero-allocation `foreach` iteration.

---

## 2. Usage Examples

### Type-Safe Event Bus

Define an event:
```csharp
using NFramework;

public readonly struct PlayerHealthChangedEvent : IEvent
{
    public readonly int OldHealth;
    public readonly int NewHealth;

    public PlayerHealthChangedEvent(int oldHealth, int newHealth)
    {
        OldHealth = oldHealth;
        NewHealth = newHealth;
    }
}
```

Subscribe and raise events:
```csharp
using System;
using UnityEngine;
using NFramework;

public class PlayerHUD : MonoBehaviour
{
    private IDisposable _subscription;

    private void OnEnable()
    {
        _subscription = EventBus<PlayerHealthChangedEvent>.Subscribe(OnHealthChanged);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void OnHealthChanged(PlayerHealthChangedEvent evt)
    {
        Debug.Log($"Health: {evt.OldHealth} -> {evt.NewHealth}");
    }
}

// In Player logic:
EventBus<PlayerHealthChangedEvent>.Raise(new PlayerHealthChangedEvent(100, 80));
```

### Observable Value

```csharp
[SerializeField] private ObservableValue<int> _coins = new(0);

private IDisposable _coinSub;

private void Start()
{
    // invokeImmediately = true updates the label immediately on bind
    _coinSub = _coins.Subscribe(coins => _coinText.text = coins.ToString(), invokeImmediately: true);
}

private void OnDestroy()
{
    _coinSub?.Dispose();
}
```

### Observable List

```csharp
[SerializeField] private ObservableList<string> _inventory = new();

private void OnEnable()
{
    _inventory.OnChanged += HandleInventoryChanged;
}

private void OnDisable()
{
    _inventory.OnChanged -= HandleInventoryChanged;
}

private void HandleInventoryChanged(ObservableListEvent<string> evt)
{
    switch (evt.Type)
    {
        case ObservableListEventType.Add:
            CreateSlotView(evt.Index, evt.Item);
            break;
        case ObservableListEventType.Remove:
            RemoveSlotView(evt.Index);
            break;
        case ObservableListEventType.Replace:
            UpdateSlotView(evt.Index, evt.Item);
            break;
        case ObservableListEventType.Clear:
        case ObservableListEventType.Reset:
            RebuildAllSlots();
            break;
    }
}
```
