---
description: StylusCore Technical Constitution — 07_UNDO_REDO_COMMANDS
---

# 🔁 07 — UNDO / REDO COMMAND SYSTEM  
(Headless, Deterministic, Fast)

StylusCore’s Undo/Redo is a production-grade command system for a stylus-first infinite canvas editor. It MUST remain **headless**, **deterministic**, **memory-bounded**, and integrated with the Canvas Engine (QuadTree + invalidation), the ink pipeline (Wet→Dry), and autosave (snapshot-only).

---

## 0) Normative Language
- **MUST**: required  
- **MUST NOT / FORBIDDEN**: prohibited  
- **SHOULD**: recommended  
- **MAY**: optional  

---

## 1) RED LINES (Executive Summary)
- **MUST:** Every document mutation is performed via **IUndoableCommand** (Command Pattern Mandate).
- **MUST:** Commands and payloads are **headless** (no `System.Windows.*`, no WPF UI objects).
- **MUST:** Undo stack has a **hard limit of 50 steps**.
- **MUST:** A new command after any Undo **clears Redo immediately**.
- **MUST:** Undo/Redo affects **document content only** (not selection, not camera, not tool overlays).
- **MUST:** Wet ink is **not undoable** mid-stroke; ink becomes undoable only after **commit** (pen-up → Dry).
- **MUST:** Rapid ink commits within **500ms** are grouped into one undo unit.
- **MUST:** Continuous manipulations (drag/resize/rotate) are grouped via **pointer capture** into one undo unit.
- **MUST:** Commands are **atomic**: Apply/Unapply cannot leave partial state; failures must not corrupt the document.
- **MUST:** Undo history is **NOT persisted**; crash recovery uses autosaved snapshots only.

---

## 2) Scope & Non-Goals

### 2.1 Scope (Undoable Domain Mutations)
Undo/Redo applies to document state:
- Add/remove/modify strokes
- Move/resize/rotate objects
- Property changes (color, thickness, style, visibility, z-order)
- Group/ungroup
- Layer operations (create/remove/move objects between layers)
- Text/table/media object edits (via commit policy)

### 2.2 Non-Goals (STRICT)
- **FORBIDDEN:** Undoing pure UI transient state:
  - selection highlights
  - caret blink / hover
  - tool UI overlays
  - pointer hover states
- **FORBIDDEN (default):** Undoing camera pan/zoom.
- **FORBIDDEN:** Persisting Undo/Redo history to disk (no command serialization).
- **FORBIDDEN:** Multi-user collaboration requirements (CRDT/OT) driving current complexity.

---

## 3) Core Concepts

### 3.1 Headless Document Model
- **MUST:** Domain types use primitives (e.g., `double x,y`, `uint argb`, point arrays).
- **MUST:** Commands never reference WPF rendering objects.

### 3.2 Object Identity (Stable IDs)
- **MUST:** Every document object has a stable **ObjectId** (recommended `ulong`).
- **MUST:** IDs are assigned by a monotonic allocator and **never reused** within the document lifetime.
- **SHOULD:** Keep `DocumentId` as `Guid` for external referencing, but object addressing in commands uses fast `ulong`.
- **MUST:** Commands reference targets by `(LayerId, ObjectId)` or `(ObjectId)` where layer is derivable.
- **MUST:** Moving objects across layers/spatial chunks MUST NOT change `ObjectId`.

**Recommended ID strategy**
- `ObjectId : ulong` monotonic counter persisted in document metadata
- `LayerId : ulong` monotonic counter
- On merge/import, remap incoming IDs into the target document’s ID space

---

## 4) Command Interfaces (Headless)

### 4.1 Required Interface
- **MUST:** Implement:

~~~csharp
public interface IUndoableCommand
{
    string DebugName { get; }                 // recommended
    bool TryApply(DocumentState doc, out ChangeSet changes);
    bool TryUnapply(DocumentState doc, out ChangeSet changes);
}
~~~

### 4.2 ChangeSet Bridge (Pure Data)
- **MUST:** Apply/Unapply returns a ChangeSet for Engine integration (QuadTree + invalidation).
- **MUST:** ChangeSet contains ONLY headless data (IDs + world bounds hints), no WPF types.

~~~csharp
public readonly struct ChangeSet
{
    public readonly IReadOnlyList<ChangeItem> Items;
    public readonly bool AffectsPersistence;
}

public readonly struct ChangeItem
{
    public readonly ulong ObjectId;
    public readonly ulong LayerId;
    public readonly AabbD BeforeWorldBounds; // optional
    public readonly AabbD AfterWorldBounds;  // optional
    public readonly ChangeKind Kind;         // Added/Removed/Updated/Moved
}
~~~

### 4.3 Atomicity Contract (CRITICAL)
- **MUST:** `TryApply` / `TryUnapply` are atomic:
  - if returning `false`, `DocumentState` MUST remain unchanged
- **MUST:** Validate prerequisites before mutating state
- **SHOULD:** Use internal two-phase pattern: Validate → ApplyCore

---

## 5) Undo Manager & Stack Policy

### 5.1 Per-Document UndoManager
- **MUST:** Each open document/canvas has its own UndoManager.
- **MUST:** No cross-document transactions.

### 5.2 Stack Behavior
- **MUST:** Maintain:
  - undo stack (max 50)
  - redo stack
- **MUST:** On new committed command:
  - push to undo stack
  - clear redo stack
- **MUST:** When undo exceeds 50, drop the **oldest** undo unit.
- **SHOULD:** Implement undo stack as a fixed-capacity ring buffer to avoid allocations.

### 5.3 SavePoint & DirtyFlag
- **MUST:** Track a SavePoint (stack position at last successful save/autosave).
- **MUST:** DirtyFlag is true iff current position != SavePoint.
- **MUST:** Undo/Redo updates DirtyFlag deterministically.
- **MUST:** Redo-clearing due to new command must preserve SavePoint correctness.

---

## 6) Grouping & Transactions

### 6.1 Transaction API
- **MUST:** Provide a transaction mechanism:
  - BeginTransaction(name)
  - AddToTransaction(cmd)
  - CommitTransaction() → pushes 1 CompositeCommand
  - RollbackTransaction() → cancels without pushing stack

### 6.2 Ink Grouping (500ms Rule)
- **MUST:** After dry stroke commit, if another stroke commits within **500ms**, merge into the same undo unit.
- **MUST:** Grouping applies only to committed dry strokes (never wet points).

### 6.3 Drag/Resize/Rotate Grouping (Pointer Capture)
- **MUST:** Continuous manipulation produces one command:
  - capture “before”
  - update live visuals during drag (not pushed)
  - on release capture “after” and commit one command
- **MUST:** If before == after, do not push.

---

## 7) Undoability Policy Matrix (STRICT)
- **MUST:** Maintain a policy table (source of truth) defining what is undoable.

### 7.1 Default Policy
**Undoable (YES):**
- Add/Delete objects (strokes, text, tables, images, shapes)
- Move/Resize/Rotate objects
- Group/Ungroup
- Z-order changes
- Layer assignment changes
- Style/property changes (stroke width/color, text style, table formatting)
- Paste/Cut operations (content changes)

**Not Undoable (NO):**
- Selection changes
- Hover/pressed visual states
- Tool changes (pen vs select)
- Camera pan/zoom (default NO)
- Temporary overlays that do not commit content

**Conditional:**
- Enter/Exit edit mode is undoable only if it commits/reverts content (see §11)

### 7.2 Multi-Document Focus Rule
- **MUST:** Ctrl+Z/Ctrl+Y applies to the focused editor/document.
- **MUST:** Modal dialogs may intercept (local text undo) only if explicitly designed; otherwise route to document.

---

## 8) Ink Commands (Wet→Dry)

### 8.1 Wet Ink (Not Undoable)
- **MUST:** Wet ink is ephemeral and not added to undo stack.
- **MUST:** Pen thread never creates commands.

### 8.2 Dry Ink Commit (Undoable)
On pen-up (authoritative UI/Domain thread):
- simplify (e.g., RDP)
- optionally smooth for rendering (render-side only; not persisted as WPF objects)
- create headless StrokePayload
- commit AddStrokeCommand

### 8.3 Required Ink Commands
- **MUST:** `AddStrokeCommand(ObjectId id, StrokePayload stroke, LayerId layer, AabbD bounds)`
- **MUST:** `DeleteStrokeCommand(ObjectId id, StrokePayload snapshot, LayerId layer, AabbD bounds)`
- **SHOULD:** Eraser gesture creates a composite delete (multi-stroke) grouped by session.

### 8.4 Payload Rules (Memory Safety)
- **MUST:** Store post-simplification points in command payload.
- **MUST NOT:** Store raw stylus point arrays indefinitely.
- **SHOULD:** Pool arrays and release when commands are dropped from the stack.

---

## 9) Transform & Property Commands

### 9.1 Transform Commands
- **MUST:** Store minimal deltas (before/after transform).
- **MUST:** Compute bounds before/after for ChangeSet hints.

### 9.2 Property Commands
- **MUST:** Store old + new value (delta), not full object when avoidable.
- **SHOULD:** Use typed commands for common operations:
  - ChangeStrokeStyleCommand
  - ChangeTextStyleCommand
  - ChangeVisibilityCommand
  - ChangeZOrderCommand

### 9.3 Group/Ungroup
- **MUST:** Group operations are deterministic:
  - grouping creates a new Group object (new ObjectId)
  - children keep their ObjectIds
- **MUST:** Undo restores exact membership and ordering.

---

## 10) Delete / Cut / Paste Semantics

### 10.1 Delete Must Carry Restore Payload
- **MUST:** Delete commands include enough snapshot data to restore the object exactly.
- **MUST:** Multi-object delete is atomic (transaction/composite).

### 10.2 Paste
- **MUST:** Paste creates new IDs (no collisions).
- **MUST:** Paste is a single undo unit (composite if multiple objects).

---

## 11) View-to-Edit Overlay Commit Policy  
(Text / Tables / Rich Objects)

### 11.1 Session-Based Editing (Global Stack)
- **MUST:** Text/table editing is session-based, not per-keystroke in the global undo stack.
- **MUST:** Enter edit captures a baseline snapshot (headless).
- **MUST:** Exit edit commits one domain command if content changed.

### 11.2 Editing Commands
- **MUST:** Use commands like:
  - `UpdateTextObjectCommand(ObjectId id, TextPayload before, TextPayload after)`
  - `UpdateTableObjectCommand(ObjectId id, TablePayload before, TablePayload after)`
- **SHOULD:** Prefer deltas if practical; otherwise store before/after snapshots with pooling.

### 11.3 Autosave with Active Edit Sessions
- **MUST:** Autosave snapshots must be consistent.
- **SHOULD:** If an edit session is active, request a fast headless snapshot on the UI thread; if not quickly available, skip the tick rather than blocking UI.

---

## 12) QuadTree & Rendering Integration

### 12.1 Commands Do Not Touch WPF
- **FORBIDDEN:** WPF invalidation/redraw calls from commands.

### 12.2 Engine Consumes ChangeSet
- **MUST:** Engine uses ChangeSet to:
  - update QuadTree incrementally (insert/remove/update)
  - request dirty-rect invalidation from bounds before/after
- **SHOULD:** Prefer incremental QuadTree updates; rebuild only as fallback.

---

## 13) Autosave / Persistence Interaction

### 13.1 Undo Stack Persistence Policy
- **MUST:** Undo/Redo history is not serialized.
- **MUST:** Crash recovery uses autosaved snapshots only.
- **MUST:** After restart, undo stack starts empty.

### 13.2 Snapshot Safety
- **MUST:** Autosave reads from an immutable snapshot of DocumentState.
- **MUST:** Snapshot creation must not block UI beyond a small bounded time.
- **SHOULD:** Use structural sharing/copy-on-write where feasible.

---

## 14) Concurrency & Threading Boundaries (CRITICAL)
- **MUST:** Document mutations (Apply/Unapply) happen on the authoritative UI/Domain thread only.
- **MUST:** Background threads may compress/serialize autosave snapshots and run post-stroke jobs but MUST NOT mutate live document.
- **FORBIDDEN:** Pen thread mutating document state or undo stacks.

---

## 15) Memory & Performance Budget

### 15.1 Bounded Memory
- **MUST:** Undo memory is bounded by:
  - 50-step limit
  - pooled allocations for large payloads
- **SHOULD:** Use `ArrayPool<T>` for point arrays and payload buffers.
- **MUST:** Avoid LOH churn by pooling large arrays and avoiding massive temporary allocations.

### 15.2 Delta vs Full-State Rule of Thumb
- **MUST:** Prefer delta storage for transforms, property changes, z-order, visibility.
- **MUST:** Use full snapshots when required for correctness (delete/restore, complex objects where delta is unreliable).

---

## 16) Error Handling & Recovery

### 16.1 Redo Invalidation
- **MUST:** Any new committed command clears redo stack.
- **MUST:** Only committed transactions become undo units.

### 16.2 Failure Policy (Atomicity)
- **MUST:** If Apply/Unapply fails:
  - document remains unchanged
  - no stack mutation occurs
  - diagnostics include DebugName + reason

---

## 17) Testing Checklist (MANDATORY)

### 17.1 Invariants
- **MUST:** For every command type:
  - Apply → Unapply returns identical state
  - Apply → Unapply → Apply returns identical state
- **MUST:** Equality includes:
  - same object IDs and properties
  - same post-simplification stroke data
  - same layer membership and z-order
  - same spatial query results for identical viewport

### 17.2 Determinism Tests
- **MUST:** Same snapshot + same command sequence → identical state.

### 17.3 Property-Based / Fuzz Testing
- **MUST:** Random sequences of operations with random undo/redo interleaving.
- **MUST:** Validate invariants after each step.

### 17.4 Stress Tests
- **MUST:** Large documents with many strokes and large payloads.
- **MUST:** Ensure undo/redo latency stays acceptable and memory remains bounded.

---

## 18) Anti-Patterns (FORBIDDEN)
- **FORBIDDEN:** UI objects inside commands (WPF types, brushes, geometries).
- **FORBIDDEN:** Persisting undo history to disk (current scope).
- **FORBIDDEN:** Undoing selection/camera/tool state by default.
- **FORBIDDEN:** Pushing commands for every move during drag (must use pointer-capture grouping).
- **FORBIDDEN:** Mid-stroke ink undo (wet ink is ephemeral).

---

## 19) Minimal Pseudocode (Reference)

~~~csharp
public sealed class UndoManager
{
    private readonly RingBuffer<IUndoableCommand> _undo = new(capacity: 50);
    private readonly Stack<IUndoableCommand> _redo = new();
    private int _savePointVersion;

    public bool TryDo(IUndoableCommand cmd, DocumentState doc, out ChangeSet changes)
    {
        if (!cmd.TryApply(doc, out changes)) return false;

        _undo.Push(cmd);
        _redo.Clear();
        UpdateDirtyFlag();
        return true;
    }

    public bool TryUndo(DocumentState doc, out ChangeSet changes)
    {
        if (!_undo.TryPop(out var cmd)) { changes = default; return false; }
        if (!cmd.TryUnapply(doc, out changes)) { return false; }

        _redo.Push(cmd);
        UpdateDirtyFlag();
        return true;
    }

    public bool TryRedo(DocumentState doc, out ChangeSet changes)
    {
        if (_redo.Count == 0) { changes = default; return false; }
        var cmd = _redo.Pop();
        if (!cmd.TryApply(doc, out changes)) return false;

        _undo.Push(cmd);
        UpdateDirtyFlag();
        return true;
    }

    private void UpdateDirtyFlag()
    {
        // Dirty iff current version != _savePointVersion (implementation-specific)
    }
}
~~~

---

## 20) Self-Audit Checklist
- [ ] Headless: no WPF types in commands/payloads
- [ ] 50-step cap enforced + memory bounded
- [ ] 500ms stroke grouping + pointer-capture grouping implemented
- [ ] Redo cleared on new command after undo
- [ ] Wet ink not undoable; dry commit generates commands
- [ ] Delete commands carry restore snapshot
- [ ] View-to-edit session commit is single undo unit
- [ ] ChangeSet contract exists (IDs + world bounds)
- [ ] Autosave uses snapshots; undo history not persisted
- [ ] Invariant + determinism + fuzz + stress tests exist

---
