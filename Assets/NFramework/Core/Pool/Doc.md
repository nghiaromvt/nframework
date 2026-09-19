# High-Performance Component Pooling System

The `NFramework.Core` Pooling System provides a lightweight, fail-fast, high-performance object pooling architecture specifically designed for Unity GameObjects and Components.

It encapsulates Unity 6's `UnityEngine.Pool.ObjectPool<T>` within a pure C# generic container (`Pool<T>`), provides a centralized static facade (`PoolService`), supports explicit lifecycle scopes (`Scene` vs. `Global`), offers asynchronous prewarming via `UniTask` across frame budgets, and ensures zero garbage collection overhead during regular gameplay operations.

---

## 1. Architecture & Core Components

```
+-----------------------------------------------------------+
|                        PoolService                        |
|   - Static central registry (Dictionary<Component, IPool>)|
|   - Auto-creates pools on demand or via Register()        |
+-----------------------------+-----------------------------+
                              | manages
                              v
+-----------------------------------------------------------+
|                          Pool<T>                          |
|   - Pure C# container wrapping UnityEngine.Pool.ObjectPool|
|   - O(1) active tracking via HashSet<T>                   |
|   - Dedicated PoolRoot GameObject per prefab              |
|   - Inactive container suppresses Awake during prewarm    |
+--------------+-----------------------------+--------------+
               | owns                        | attaches
               v                             v
       +---------------+             +----------------+
       |   IPoolable   |             |  PooledObject  |
       |  (Root only)  |             |  Back-reference|
       |  - OnSpawn()  |             |  - Release()   |
       |  - OnDespawn()|             +----------------+
       +---------------+
```

### Core Types

- **`PoolScope`**: Enum defining the pool's lifetime scope:
  - `PoolScope.Scene`: Tied to a Unity scene. When the scene is unloaded, the pool and its instances are automatically unregistered and destroyed.
  - `PoolScope.Global`: Marked with `DontDestroyOnLoad`. Persists across scene transitions.
- **`IPoolable`**: Interface containing `OnSpawn()` and `OnDespawn()`. Queried strictly on the root GameObject and cached during instantiation for zero-allocation dispatching.
- **`IPool`**: Non-generic polymorphic interface enabling `PoolService` to manage heterogeneous generic pools uniformly (`Despawn(Component instance)`, `DespawnAll()`, `Clear()`).
- **`Pool<T>`**: Pure C# class (`where T : Component`) managing instance allocation, active tracking, prewarming, and lifecycle notifications.
- **`PooledObject`**: Helper component attached to every pooled GameObject root. Maintains an internal back-reference to its owning `IPool` and exposes `Release()` and `ReleaseVoid()` (for UnityEvents/Animation Events).
- **`PoolRootMarker`**: Component attached to the dedicated pool root GameObject. On `OnDestroy()`, it triggers pool disposal and unregisters from `PoolService` when a scene unloads.
- **`PoolService`**: Static facade providing registration, lookup, spawning, despawning, and bulk clearing.

---

## 2. Instantiation & Lifecycle Order

To prevent premature initialization side effects during prewarming, instances are created under an inactive container:

1. **Instantiation (Prewarm or On-Demand Creation)**:
   - GameObject is instantiated under an inactive child container (`_inactiveContainer`).
   - Unity suppresses `Awake()` and `OnEnable()`.
   - `PooledObject` is attached/cached, and root `IPoolable` components are queried once.
   - Instance is explicitly deactivated (`SetActive(false)`), then reparented to the pool root transform.
2. **Spawn**:
   - Instance is retrieved from `ObjectPool<T>`.
   - Added to internal active `HashSet<T>`.
   - Reparented to target parent (or remains under pool root if `parent == null`).
   - Position and rotation are applied.
   - GameObject is activated (`SetActive(true)`). Unity executes `Awake()` (first time only) and `OnEnable()`.
   - Cached `IPoolable.OnSpawn()` callbacks are executed.
3. **Despawn**:
   - Instance membership in active `HashSet<T>` is verified (throws if not active or already returned).
   - Cached `IPoolable.OnDespawn()` callbacks are executed.
   - Reparented back to the pool root transform.
   - GameObject is deactivated (`SetActive(false)`). Unity executes `OnDisable()`.
   - Instance is returned to `ObjectPool<T>`.

---

## 3. Usage & Code Examples

### Defining a Poolable Component

Implement `IPoolable` on the root GameObject component:

```csharp
using UnityEngine;
using NFramework;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float _speed = 20f;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void OnSpawn()
    {
        _rigidbody.linearVelocity = transform.forward * _speed;
    }

    public void OnDespawn()
    {
        _rigidbody.linearVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Despawn via PoolService or via attached PooledObject
        PoolService.Despawn(this);
    }
}
```

### Spawning via PoolService

You can spawn directly without explicit pre-registration (auto-registration creates a scene-scoped pool with default capacity):

```csharp
using UnityEngine;
using NFramework;

public class Gun : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    public void Shoot()
    {
        // Auto-creates pool if not already registered
        Bullet bullet = PoolService.Spawn(_bulletPrefab, _firePoint.position, _firePoint.rotation);
    }
}
```

### Explicit Registration & Prewarm

Register pools ahead of time during scene initialization to configure capacity, scope, and prewarm:

```csharp
using UnityEngine;
using NFramework;
using Cysharp.Threading.Tasks;

public class GameplayInitializer : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Enemy _enemyPrefab;

    private async void Start()
    {
        // Synchronous prewarm: 20 bullets ready immediately
        PoolService.Register(_bulletPrefab, prewarm: 20, maxSize: 100, scope: PoolScope.Scene);

        // Asynchronous prewarm: instantiate 50 enemies over time, 5 per frame
        var enemyPool = PoolService.Register(_enemyPrefab, prewarm: 0, maxSize: 50, scope: PoolScope.Scene);
        await enemyPool.PrewarmAsync(count: 50, perFrame: 5, cancellationToken: destroyCancellationToken);
    }
}
```

### Returning Objects to Pool

Objects can be returned via `PoolService`, direct `Pool<T>` reference, or the instance's `PooledObject`:

```csharp
// Method 1: Static PoolService
PoolService.Despawn(bulletInstance);

// Method 2: Direct Pool<T>
bulletPool.Despawn(bulletInstance);

// Method 3: Attached PooledObject helper
bulletInstance.GetComponent<PooledObject>().Release();

// Method 4: In UnityEvents / Animation Events
// Wire button or animation event directly to PooledObject.ReleaseVoid()
```

### Standalone Pool (Without PoolService)

`Pool<T>` can also be used as a standalone object without global registration:

```csharp
var localPool = new Pool<Bullet>(bulletPrefab, prewarm: 10, maxSize: 30, scope: PoolScope.Scene);

Bullet b = localPool.Spawn(transform.position, Quaternion.identity);
localPool.Despawn(b);

// Clean up all instances and root GameObject
localPool.Dispose();
```

---

## 4. Asynchronous Prewarming (`PrewarmAsync`)

Large prewarms can cause frame rate hitches if instantiated synchronously in a single frame. `Pool<T>.PrewarmAsync` distributes instantiations across frames using `UniTask`:

```csharp
await PoolService.PrewarmAsync(
    prefab: vfxPrefab,
    count: 100,
    perFrame: 10,
    cancellationToken: destroyCancellationToken
);
```

- **Frame Budget**: Instantiates `perFrame` items, then awaits `UniTask.Yield()` to allow Unity to render the next frame.
- **Concurrent Spawn Safety**: If `Spawn()` is called while an async prewarm is running, items in the prewarm staging list are immediately provided without allocating duplicate instances.
- **Cancellation**: Linked with the pool's lifetime token. If the pool is disposed or the caller cancels the operation, excess items are cleaned up without leaking.

---

## 5. Fail-Fast Guarantees & Strict Error Handling

In compliance with the framework's strict error-handling policy, the pool does **not** log warnings or hide invalid states. Any invariant violation immediately throws an exception:

| Condition | Thrown Exception | Rationale |
|-----------|------------------|-----------|
| Despawning an inactive or already despawned object | `InvalidOperationException` | Prevents double returns and pool corruption. |
| Despawning an object not belonging to any pool | `InvalidOperationException` | Exposes attempts to despawn unmanaged instances. |
| Prewarm count > `maxSize` | `InvalidOperationException` | Prevents exceeding configured pool capacity. |
| Duplicate pool registration for the same prefab | `InvalidOperationException` | Catches duplicate setup bugs immediately. |
| `perFrame <= 0` in `PrewarmAsync` | `ArgumentOutOfRangeException` | Prevents infinite frame loops. |
| Spawning or prewarming on a disposed pool | `InvalidOperationException` | Prevents operating on destroyed hierarchy. |

---

## 6. Domain Reload & Cleanup

- **Domain Reload Disabled Support**: `PoolService` automatically resets its static dictionary on Play Mode entry via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Scene Unloading**: When a scene unloads, all scene-scoped pool root GameObjects are destroyed by Unity. The attached `PoolRootMarker` executes `Dispose()`, which cleans up all tracked active instances to prevent orphaned GameObjects and unregisters the pool from `PoolService`.
- **Explicit Cleanup**: Call `PoolService.Clear(prefab)` or `PoolService.ClearAll()` to immediately destroy all idle and active instances and reset pool capacity before major scene transitions.
