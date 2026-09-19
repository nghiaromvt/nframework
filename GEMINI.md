# Gemini AI Coding Guidelines

---

## 1. General Principles
- Keep code concise, clean, modular, and maintainable.
- Follow **SOLID** and **YAGNI** principles. Do not write redundant properties, fields, or boilerplate code until actively needed.
- **Fail Fast Principle**: Do not write defensive null checks for objects or references that are strictly required to exist. Let Unity or C# fail fast (e.g., throwing `NullReferenceException`) so missing setup or dependencies are exposed immediately.
  - *No Implicit Fallbacks*: Never arbitrarily implement fallback logic, default substitute values, or silent recovery mechanisms unless explicitly requested. Always ask the user if fallback behavior is needed.
  - *Exception*: Only check for null to skip logic if the code explicitly supports multiple cases (e.g., an optional component). The skipping mechanism (e.g., `return`, `continue`, or bypassing a block) depends on the context.
- **Strict Error Handling**: Do **NOT** log warnings. Everything is supposed to work as intended. If requirements are not met, log an error and immediately stop execution (e.g., return early or throw an exception) to prevent the game from running in an invalid state.
- Always prefer explicit design and clear code over overly clever or obscure implementations.

---

## 2. C# & .NET Code Conventions (Microsoft Standard)
- **Naming Conventions**:
  - `PascalCase`: Classes, Structs, Enums, Interfaces (prefix with `I`), Methods, Public Fields, Properties, Events.
  - `_camelCase`: Private and protected fields (e.g., `_moveSpeed`, `_playerHealth`).
  - `camelCase`: Method parameters and local variables.
  - `ALL_CAPS` / `UPPER_CASE`: Constants (`const`).
- **Code Structure & Formatting**:
  - Use Allman brace style (opening brace `{` on a new line).
  - Explicitly specify access modifiers (`public`, `private`, `protected`, `internal`).
  - Avoid magic numbers; use `const`, `readonly`, or serialized configuration values.
  - Do not create backing properties or fields speculatively unless required by current logic.
  - Remove unused or redundant `using` directives to keep code headers clean and avoid namespace pollution.

---

## 3. Unity & MonoBehaviour Rules
- **Class Member Ordering**: Members inside `MonoBehaviour` classes **MUST** strictly follow this sequence:
  1. **Fields** (Serialized, constant, and private fields)
  2. **Properties**
  3. **Unity Lifecycle Functions** (e.g., `Awake()`, `OnEnable()`, `Start()`, `Update()`, `FixedUpdate()`, `LateUpdate()`, `OnDisable()`, `OnDestroy()`)
  4. **Custom Methods** (Public APIs, followed by private helper functions)
- **Encapsulation**: Use `[SerializeField] private` instead of `public` fields for Inspector configuration.
- **Performance**:
  - Cache references (`GetComponent`, Camera references) in `Awake()` or `Start()`. Never call `GetComponent()` in frame updates (`Update`/`FixedUpdate`).
  - Avoid memory allocations (`GC.Alloc`), string concatenations, or `LINQ` operations inside `Update()` / `FixedUpdate()`.
- **Avoid Preprocessor Directives (Compiler Flags)**: Minimize using preprocessor directives (e.g., `#if UNITY_EDITOR`) when runtime conditional checks (e.g., `if (Application.isEditor)`, `if (Application.isPlaying)`) can be used instead. This ensures all code branches are compiled and syntax-checked across environments.
- **Singleton Existence Check**: Always check if a Singleton instance exists using `TSingleton.HasInstance` (e.g., `if (SaveService.HasInstance)`) instead of checking `TSingleton.Instance != null`. This avoids accidental auto-instantiation or side-effects during teardown.

---

## 4. Comments & Documentation
- **Language**: **All comments, XML documentation, and code explanations MUST be written in English.**
- **No Obvious Comments**: Do **NOT** write comments or XML documentation for simple, self-explanatory, or obvious functions/properties (e.g., standard getters/setters, basic setup methods).
- Write meaningful comments explaining *why* non-obvious logic exists rather than stating obvious *what* code does.
- **Documentation Synchronization**: When implementing, refactoring, or modifying code, always update corresponding XML comments, method descriptions, and project documentation (`.md` files) to ensure documentation stays strictly up to date with code changes.

---

## 5. Unity MCP Usage
- Leverage **Unity MCP (Model Context Protocol)** tools whenever context about the Unity project layout, scene hierarchy, project settings, console logs, or asset paths is required.
- Use MCP integrations to inspect project status, resolve compilation errors, or locate relevant code references before writing or refactoring code.
- **Manual Asset Refresh & Compilation**: Auto-refresh and domain reload on play are disabled in this project. After creating, updating, or deleting C# scripts or assets, ALWAYS trigger a recompile/refresh (e.g., via Unity MCP `recompile` or appropriate refresh tools) and check the compilation status before running tests, entering play mode, or finalizing the task.

---

## 6. Workflow & Task Execution
- **Proactive Clarification**: Whenever requirements, edge cases, or intentions are unclear or ambiguous, ask the user for clarification before assuming or proceeding.
- **Short Tasks**: Always explain the approach, proposed changes, and rationale to the user after modifying or writing code.
- **Long / Complex Tasks**: Break down the task into a detailed, step-by-step implementation plan. Wait for explicit user review and approval before executing changes.

---

## 7. Codebase Knowledge Graph (codebase-memory-mcp)
- **Graph First**: ALWAYS prioritize `codebase-memory-mcp` tools over standard grep, glob, or text search for structural discovery, architecture exploration, and dependency tracing.
- **Skill Usage**: Refer to and activate the `codebase-memory` skill for detailed query workflows, evidence tiers (Scout/Verify/Auditor), Cypher examples, and subagent delegation rules.
- **Fallback**: Only fall back to grep/glob when querying string literals, non-code files (config, Docker, scripts), or when graph results are insufficient.
