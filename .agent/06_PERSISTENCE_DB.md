---
description: StylusCore Technical Constitution — 06_PERSISTENCE_DB
---

# 💾 06 — PERSISTENCE & DATABASE  
(SQLite + Protobuf + LZ4 + Spatial Chunking)

This document defines the **STRICT, non-negotiable rules** for persisting StylusCore’s infinite canvas data with **high performance**, **crash safety**, and **scalability**.

**Goal:**  
Enable viewport-based loading, fast pan/zoom/open times, and corruption-free storage even for **1GB+ documents**, including power loss or process termination.

---

## 0) Normative Language
- **MUST**: required  
- **MUST NOT / FORBIDDEN**: prohibited  
- **SHOULD**: recommended  
- **MAY**: optional  

---

## 1) RED LINES (Non-Negotiable)

### 1.1 Headless Domain Persistence
- **MUST:** All persisted domain models are **UI-agnostic**.
- **FORBIDDEN:** Any `System.Windows.*`, `Point`, `Rect`, `Color`, `Brush`, `DependencyObject` in domain or persistence layers.
- **MUST:** Coordinates stored as:
  - `double X, Y` (domain/runtime), or
  - quantized `int32/int64` (disk format).
- **MUST:** Color encoded as:
  - `uint ARGB (0xAARRGGBB)`, or
  - `#RRGGBB / #AARRGGBB` string.

---

### 1.2 UI Thread Never Performs I/O
- **MUST:** All save/load operations run on background threads.
- **MUST:** UI thread responsibilities are limited to:
  - producing immutable snapshots,
  - enqueueing work,
  - updating render state.

---

### 1.3 JSON / XML Forbidden for Large Payloads
- **FORBIDDEN:** JSON or XML for ink, points, or large media payloads.
- **MUST:** Binary payloads only (Protobuf or custom binary) with **LZ4 compression**.

---

## 2) Storage Topology (Default)

### 2.1 Single SQLite Container
- **MUST:** Each StylusCore document is a **single SQLite file** (e.g. `*.stylusdb`).
- **Rationale:** SQLite acts as a robust file format with transactions, WAL, and crash safety.

---

### 2.2 Spatial Chunking Model
- Infinite canvas data is sparse and spatially clustered.
- **MUST:** All content is stored in **spatial chunks**.
- **MUST:** Chunks are loaded based on **viewport intersection** (lazy hydration).

---

## 3) Spatial Chunking Rules

### 3.1 Chunk Definition
- A chunk is a spatial package containing multiple entities and metadata.
- **MUST:** Every entity has an **AABB** (world-space).
- **MUST:** Chunk AABB is the union of its entities.

---

### 3.2 Chunk Size Target
- **FORBIDDEN:** Loading entire documents into memory.
- **DEFAULT:** ~50–100 stroke-equivalent density per chunk.
- **SHOULD:** Chunk size adapts to density (smaller in dense areas, larger in sparse areas).

---

### 3.3 Viewport-Based Loading
- **MUST:** Visible chunk set recomputed after pan/zoom.
- **MUST:** Non-visible chunks are eviction candidates (LRU/clock).
- **MUST:** Spatial queries executed at the database level (R-Tree).

---

## 4) SQLite Spatial Index (R-Tree)

### 4.1 R-Tree Is Mandatory
- **MUST:** SQLite **R-Tree** used for spatial queries.
- **FORBIDDEN:** Iterating all chunks in memory.

---

### 4.2 Auxiliary Columns
- **SHOULD:** Use SQLite auxiliary columns to avoid JOINs:
  - `layer_id`, `kind`, `content_ref`, etc.

---

### 4.3 Minimum Schema (Guideline)
- `rtree_chunks(chunk_id, minX, maxX, minY, maxY, +layer_id, +kind)`
- `chunks(chunk_id PRIMARY KEY, revision, payload_blob, codec, checksum_xx64, updated_at)`
- `assets(hash_xx64 PRIMARY KEY, mime, bytes_blob, checksum_xx64, width, height, thumb_blob, thumb_w, thumb_h)`
- `instances(instance_id PRIMARY KEY, chunk_id, kind, asset_hash_xx64 NULL, payload_ref NULL, bbox_minX, bbox_maxX, bbox_minY, bbox_maxY)`
- `meta(key PRIMARY KEY, value)`
- `snapshots(snapshot_id PRIMARY KEY, created_at, note)`

> Instance table is optional; critical requirement is **chunk bbox + R-Tree + binary payload**.

---

## 5) Binary Payload Format (Protobuf)

### 5.1 Serializer
- **MUST:** Primary serializer is **protobuf-net**.
- **MAY:** Custom binary writers for extreme hot paths (with explicit versioning).

---

### 5.2 Schema Versioning
- **MUST:** Every payload contains:
  - `schema_version`
  - `payload_kind`
  - `codec`
  - `checksum`
- **MUST:** Forward/backward compatibility:
  - Field tags are never reused.
  - Deprecated fields are mapped, not removed.

---

### 5.3 Coordinate Encoding
- **SHOULD:** Disk encoding uses delta + zigzag compression.
- **DEFAULT:**
  - Runtime: `double`
  - Disk: quantized `int` + delta
- **MUST:** Quantization unit is fixed in spec (e.g. `1/1000 world unit`).

---

## 6) Ink Blocks & LZ4

### 6.1 Compression
- **MUST:** Ink data compressed with **LZ4**.
- **MUST:** Decompression fast enough for viewport hydration.

---

### 6.2 Block Size
- **DEFAULT:** 64KB uncompressed blocks.
- **FORBIDDEN:** Single massive blobs.

---

### 6.3 Zero-Copy Goal
- **SHOULD:** Use `Span<byte>` / `ReadOnlySpan<byte>`.
- **MUST:** Use pooling:
  - `ArrayPool<byte>`
  - `MemoryPool<T>`
- **MUST:** Avoid LOH allocations (>85KB).

---

## 7) Autosave & Threading

### 7.1 Dirty Flag Semantics
- **MUST:** Dirty flags tracked per entity and per chunk.
- **MUST:** Autosave runs only when dirty.
- **DEFAULT:** Every 30 seconds.

---

### 7.2 Snapshot Requirement
- **MUST:** UI thread produces immutable snapshots.
- **DEFAULT:** Copy-on-write via `System.Collections.Immutable`.
- **MUST:** Snapshot creation must be non-blocking.

---

### 7.3 Channel Queue
- **MUST:** UI → persistence via bounded `Channel<DomainSnapshot>`.
- **MUST:** Overflow strategy fixed in spec:
  - latest-wins, or
  - merge-dirty-chunks.

---

### 7.4 Transaction Batching
- **MUST:** Each save cycle uses a single SQLite transaction.
- **MUST:** WAL mode enabled.

---

## 8) Crash Safety & Integrity

### 8.1 Checksums
- **MUST:** **XXHash64** for every chunk and asset blob.
- **MUST:** Verify on load.
- **MUST:** On mismatch:
  - attempt snapshot recovery, or
  - report corrupted region.

---

### 8.2 Atomic Replace
- **MUST:** Temp file written in same directory.
- **MUST:** Flush to disk before swap.
- **MUST:** Use Windows `ReplaceFile` for atomic swap.
- **NOTE:** Required for snapshot export and compaction.

---

## 9) Media & Attachments

### 9.1 Content-Addressable Storage
- **MUST:** Assets deduplicated by hash.
- **MUST:** Identical hashes reused, not duplicated.

---

### 9.2 Thumbnails & Streaming
- **MUST:** Streaming reads for large media.
- **MUST:** Separate thumbnails.

---

### 9.3 WPF Decode Rules
- **MUST:** Use `DecodePixelWidth/Height`.
- **SHOULD:** Background prefetch based on viewport velocity.

---

## 10) SQLite Operational Tuning

### 10.1 PRAGMA Baseline
- **MUST:** `journal_mode=WAL`
- **MUST:** `foreign_keys=ON`
- **SHOULD:** `busy_timeout` set
- **SHOULD:** Prepared statements only

---

### 10.2 Large File Tuning
- **SHOULD:** `page_size` = 16–32KB
- **SHOULD:** Enable `mmap_size` on 64-bit systems

---

### 10.3 Fragmentation
- **SHOULD:** Use `VACUUM INTO` for background defrag
- **MUST:** Swap via `ReplaceFile`

---

## 11) Encryption-at-Rest (Optional)

- **OPTIONAL:** SQLCipher (AES-256).
- **MUST (if enabled):** Key handling defined:
  - Windows DPAPI
  - Optional TPM-backed sealing

---

## 12) Testing & Validation

### 12.1 Benchmarks
- **MUST:** Measure throughput, latency, allocations.
- Required scenarios:
  - 1M stylus points/min ingest
  - Cold start on 1GB DB
  - Rapid zoom/pan with continuous chunk churn

---

### 12.2 Chaos Testing
- **MUST:** Kill process during write.
- **MUST:** Power-loss simulation.
- **MUST:** Recovery via checksum + snapshot.

---

## 13) Default Configuration

| Key | Default |
|---|---:|
| AUTOSAVE_INTERVAL | 30s |
| CHANNEL_CAPACITY | 2–4 |
| CHUNK_STROKE_TARGET | 80 |
| LZ4_BLOCK_BYTES | 65536 |
| SQLITE_PAGE_SIZE | 16384 |
| SQLITE_MMAP_SIZE | 256MB–2GB |
| HASH_ALGO | XXHash64 |
| THUMB_MAX_EDGE | 512px |
| ASSET_DEDUP | true |

---

## 14) Scope

**This document covers:**
- Persistence architecture
- Spatial chunking
- Binary formats
- Autosave & crash safety
- Media storage

**This document does NOT cover:**
- Canvas rendering (see `03_CANVAS_ENGINE.md`)
- Stylus threading (see `04_INPUT_INK_THREADING.md`)
- UI/XAML rules (see `08_XAML_UI_HIERARCHY.md`)
