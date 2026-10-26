# Gemini AI Coding Guidelines

## 1. Core Principles & Error Handling
- **SOLID & YAGNI**: Do not write speculative properties, backing fields, or boilerplate code until actively needed.
- **Fail Fast & No Fallbacks**:
  - Do **NOT** write defensive null checks for required references; let C#/Unity throw `NullReferenceException` naturally.
  - Only check for `null` to skip logic (`return`, `continue`) when a component/reference is explicitly optional.
  - Never implement implicit fallback logic, default substitute values, or silent recovery unless requested. Ask the user if unsure.
- **Strict Error Handling**: Never log warnings (`Debug.LogWarning`). If runtime requirements or states are invalid, log an error (`Debug.LogError`) and immediately abort execution (return early or throw).

## 2. C# Conventions (Microsoft Standard)
- **Naming**:
  - `PascalCase`: Classes, Structs, Enums, Interfaces (prefix with `I`), Methods, Public Fields, Properties, Events.
  - `_camelCase`: Private/protected fields (e.g., `_moveSpeed`).
  - `camelCase`: Parameters and local variables.
  - `UPPER_CASE`: Constants (`const`).
- **Formatting & Structure**:
  - Use Allman brace style (opening `{` on a new line).
  - Explicitly declare access modifiers (`public`, `private`, `protected`, `internal`).
  - No magic numbers; use `const`, `readonly`, or serialized config values.
  - Remove unused `using` directives.

## 3. Unity & MonoBehaviour Rules
- **Class Member Ordering**:
  1. Fields (Serialized, constants, private)
  2. Properties
  3. Unity Lifecycle (`Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, `OnDestroy`)
  4. Custom Methods (Public APIs first, then private helpers)
- **Strict `[SerializeField]` Encapsulation**:
  - Use `[SerializeField] private` instead of `public` fields for Inspector config.
  - `[SerializeField]` references **MUST** be assigned in the Inspector. **NEVER** use lazy initialization or auto-recovery fallbacks (`GetComponent`, `FindObjectOfType`, `FindAnyObjectByType`, `AddComponent`) for unassigned serialized fields.
- **Performance**:
  - Cache component/Camera references in `Awake()` or `Start()`.
  - Never use `GetComponent()`, `LINQ`, string concatenation, or cause `GC.Alloc` inside `Update()` / `FixedUpdate()`.
- **Runtime Checks over Preprocessor Directives**: Prefer runtime checks (`Application.isEditor`, `Application.isPlaying`) over `#if UNITY_EDITOR` whenever possible so all code branches are compiled and syntax-checked.
- **Singleton Check**: Always use `TSingleton.HasInstance` (e.g., `if (SaveService.HasInstance)`) instead of `TSingleton.Instance != null` to prevent accidental instantiation during teardown.

## 4. Comments & Documentation
- **English Only**: All comments, XML docs, and code explanations MUST be in English.
- **Why Over What**: Do NOT document obvious/self-explanatory code (getters/setters, basic setup). Only comment to explain *why* non-obvious logic exists.
- **Keep Synced**: Always update corresponding XML comments and `.md` documentation when modifying or refactoring code.

## 5. Unity CLI, MCP & Codebase Memory
- **Codebase Knowledge Graph First (`codebase-memory-mcp`)**:
  - Prioritize `codebase-memory-mcp` over grep/glob for architecture discovery and dependency tracing. Activate the `codebase-memory` skill for query workflows, evidence tiers, and delegation rules.
  - Fall back to grep/glob only for string literals, non-code files, or when graph results are insufficient.
- **Direct Unity MCP/CLI Usage (No Setup Scripts)**:
  - Use Unity MCP tools (`create_gameobject`, `create_prefab`, `add_component`, `set_serialized_field`, etc.) to inspect scenes, logs, settings, and manipulate assets.
  - **NEVER** create throwaway C# Editor setup scripts (e.g., `SetupM0Artifacts.cs`) to generate scenes or prefabs.
  - Never switch, unload, or dirty the active Editor scene without explicit user permission.
  - **Multi-Editor Disambiguation**: When running `unity` CLI commands, **ALWAYS** explicitly pass `--project-path "."` or the current workspace root path to ensure commands target this specific project and never route to another concurrently running Unity Editor.
- **Manual Recompile Required**: Auto-refresh and domain reload on play are disabled. After creating, editing, or deleting any C# script or asset, **ALWAYS** trigger a manual recompile/refresh via Unity MCP and verify compilation status before running tests, entering Play mode, or finishing the task.

## 6. Workflow & Execution
- **Clarify First**: Ask the user before proceeding whenever requirements or edge cases are ambiguous.
- **Short Tasks**: Explain the approach, changes, and rationale after implementation.
- **Complex Tasks**: Present a step-by-step implementation plan and wait for explicit user approval before modifying code.