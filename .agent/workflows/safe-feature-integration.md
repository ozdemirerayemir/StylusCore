---
description: 
---

# WORKFLOW: SAFE FEATURE INTEGRATION (HIERARCHY + PERFORMANCE)

This workflow must be followed for ANY task that changes code structure, adds a feature, or refactors UI/logic.

## 0) RULES FIRST (MANDATORY)
- Read workspace rule documents (e.g., `.agent/`, constitution/red-lines, architecture docs) before proposing changes.
- If any rule conflicts with this workflow, workspace rules win.

## 1) CLASSIFY THE TASK (REQUIRED)
Before coding, classify the request into one of:
- UI-only (layout/styles/resources)
- UI + ViewModel (bindings/state)
- Domain/Engine logic (core behavior, algorithms)
- Data/Persistence/Database
- Integration (AI, network, external services)
- Performance / threading / rendering

If classification is unclear, STOP and ask for direction.

## 2) HIERARCHY PLACEMENT CHECK (NO WRONG LAYER)
For any new feature or file, you MUST justify placement:

### A) Layering (generic)
- UI layer: Views, Controls, visual states, resources, converters (no domain logic)
- State layer: ViewModels, Commands, app navigation state (no heavy rendering work)
- Domain/Engine layer: core behavior, algorithms, canvas model, stroke model
- Data layer: persistence, repositories, database adapters
- Integration layer: AI, network clients, external APIs (behind interfaces)

### B) Placement Rule
- Put each concern in the lowest appropriate layer.
- Do NOT place persistence/network/AI logic inside UI or ViewModels.
- Do NOT place heavy computation on the UI thread.
- Avoid “helper dumping grounds” (random Utils classes) unless a clear module exists.

If a feature requires a new module, propose it, but do NOT create large new architectures without approval.

## 3) PERFORMANCE & THREADING GATE (MANDATORY)
For any change that affects rendering, canvas, input, or frequent updates:
- Identify hot paths (pointer/stylus events, render loop, layout passes, timers).
- Avoid allocations in hot paths.
- Prefer incremental updates over full recompute.
- Ensure UI thread remains responsive:
  - heavy work -> background thread
  - UI updates -> marshalled to UI thread
- If workspace defines threading rules (e.g., “Pen Thread vs UI Thread”), follow them strictly.

If performance impact is uncertain, STOP and provide 2 options: safe/simple vs optimized.

## 4) RESOURCE / REFERENCE DISCIPLINE (NO HARDCODE)
- User-facing strings must come from localization/resources.
- Visual tokens (colors, spacing, sizes, typography) must come from theme/resources if available.
- Icons must be referenced from the centralized icon resources (no inline geometry).
- Shared styles must be centralized (no local redefinitions).

If a required token is missing:
- Prefer reusing existing tokens.
- Only add new tokens with naming consistent to existing system.
- If adding a new semantic token could affect theming, STOP and request approval.

## 5) PLAN → PREFLIGHT → IMPLEMENT → VERIFY (REQUIRED FLOW)

### Step 5.1: Plan (no code)
Output:
- Scope (files that will change)
- What will be added/removed
- Layer placement justification
- Risks + mitigations
- Verification checklist

Wait for explicit approval if the task is structural or multi-file.

### Step 5.2: Preflight Checks (before edits)
- Search for existing patterns/modules that already solve this.
- Search for existing tokens/brushes/strings/icons.
- Confirm no duplication of concepts.
- Confirm dependencies and references (grep/search).

### Step 5.3: Implementation
- Minimal diff
- Preserve contracts (bindings/APIs)
- Avoid broad rewrites
- Remove dead code only after verifying it’s unused

### Step 5.4: Verification (must report results)
- Build/compile success
- Behavior checks relevant to the feature
- UI: interaction states, focus, input, drag, hover, transitions
- Regression risks (what could break)
- If tests exist: run them; if not, provide manual test steps

## 6) STOP CONDITIONS (STRICT)
STOP and ask before continuing if:
- You cannot determine correct layer placement.
- The change requires new tokens but existing palette/semantics are unclear.
- The change touches red-line rules (architecture boundaries, forbidden dependencies).
- The change risks UI performance responsiveness without clear mitigation.
- The request expands beyond the user’s described scope.

## 7) OUTPUT REQUIREMENTS
Always output:
- Files touched
- Key changes (diff-like or quoted sections)
- Layer placement justification (1–3 bullets)
- Verification checklist + what was actually verified
