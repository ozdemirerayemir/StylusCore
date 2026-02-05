---
description: StylusCore Technical Constitution — 06_PERSISTENCE_DB (v1.0)
---

# 💾 06 — PERSISTENCE & DATABASE (SQLite + Protobuf + LZ4 + Spatial Chunking)

Bu doküman, StylusCore’un **infinite canvas** verisini *yüksek performans + yüksek dayanıklılık* ile saklaması için **katı kuralları** tanımlar.

> **Hedef:** 1GB+ dokümanlarda bile “viewport-based loading”, hızlı açılış/zoom/pan, crash/power-loss sırasında bozulmayan kayıt.

---

## 1) Kırmızı Çizgiler (RED LINES)

### 1.1 Headless Domain (UI bağımsızlığı)
- **MUST:** Domain modelleri (Stroke, TextBlock, Image, Chunk, PageRegion) **tamamen headless** olmalı.
- **FORBIDDEN:** `System.Windows.*`, `Point`, `Rect`, `Color`, `Brush`, `DependencyObject` vb. domain/persistence katmanına giremez.
- **MUST:** Tüm koordinatlar `double X, double Y` veya **quantized** `int` temsiline map edilir.
- **MUST:** Renk `uint ARGB` (0xAARRGGBB) veya `string #RRGGBB/#AARRGGBB`.

### 1.2 UI Thread asla I/O yapmaz
- **MUST:** Tüm save/load işlemleri background thread’lerde yürütülür.
- **MUST:** UI thread sadece:
  - “snapshot üretir”
  - “iş kuyruğuna atar”
  - “render state” günceller

### 1.3 JSON/XML yasak (büyük ink verisi için)
- **FORBIDDEN:** Ink/points/media payload’ları için JSON/XML.
- **MUST:** Binary payload (Protobuf / custom) + LZ4 block compression.

---

## 2) Önerilen Depolama Topolojisi (Default)

### 2.1 Default seçim: Tek dosya SQLite container
- **MUST:** StylusCore dokümanı tek bir SQLite dosyasıdır (örn. `*.stylusdb`).
- **Rationale:** SQLite “file format” gibi davranır; transaction + WAL ile crash-safety sağlar.

### 2.2 İçerik modeli: Spatial Chunking + BLOB
- Canvas “sonsuz düzlem” → veri “seyrek kümeler” halinde.
- **MUST:** Tüm içerikler (strokes/text/media instances) **spatial chunk** mantığıyla saklanır.
- **MUST:** Chunk’lar **viewport kesişimine** göre yüklenir (lazy hydration).

---

## 3) Spatial Chunking Kuralları

### 3.1 Chunk tanımı
- Chunk bir “coğrafi paket”tir: bir grup entity (stroke/text/media instance) + bbox metadata.
- **MUST:** Her entity’nin **AABB/MBR** (axis-aligned bounding box) tutulur.
- **MUST:** Chunk bbox = içindeki entity bbox’larının birleşimi.

### 3.2 Chunk boyutu hedefi
- **MUST:** “Tüm defteri RAM’e al” yaklaşımı yok.
- **Default hedef:** chunk başına **~50–100 stroke** eşdeğeri yoğunluk.
- **SHOULD:** Chunk büyüklüğü “doküman tipi”ne göre adaptif olabilir (yoğun bölgede küçük, boş bölgede büyük).

### 3.3 Viewport-based loading
- **MUST:** Pan/Zoom sonrası “visible chunk set” hesaplanır.
- **MUST:** Visible set dışında kalan chunk’lar RAM’den eviction’a adaydır (LRU/clock).
- **MUST:** Visible chunk query DB seviyesinde yapılır (R-Tree).

---

## 4) SQLite Spatial Index (R-Tree) Kuralları

### 4.1 R-Tree zorunluluğu
- **MUST:** Spatial lookup için SQLite **R-Tree** kullanılır.
- **MUST:** “tüm chunk’ları iterate et” yasak.

### 4.2 Aux columns (JOIN’siz metadata)
- **SHOULD:** SQLite 3.24+ “auxiliary columns” kullan:
  - `+layer_id`, `+kind`, `+content_ref` gibi alanları R-Tree satırında tut
  - Böylece query’de JOIN ihtiyacı azalır.

### 4.3 Önerilen tablo iskeleti (minimum)
- `rtree_chunks(chunk_id, minX, maxX, minY, maxY, +layer_id, +kind)`
- `chunks(chunk_id PRIMARY KEY, revision, payload_blob, compression, checksum_xx64, updated_at)`
- `assets(hash_xx64 PRIMARY KEY, mime, bytes_blob, checksum_xx64, width, height, thumb_blob, thumb_w, thumb_h)`
- `instances(instance_id PRIMARY KEY, chunk_id, kind, asset_hash_xx64 NULL, payload_ref NULL, bbox_minX, bbox_maxX, bbox_minY, bbox_maxY)`
- `meta(key PRIMARY KEY, value)`
- `snapshots(snapshot_id PRIMARY KEY, created_at, note, checkpoint_info)`

> Not: “instances” opsiyonel; basit modelde instance metadata chunk payload içinde de tutulabilir. Kritik olan: **chunk bbox + R-Tree + chunk blob** üçlüsü.

---

## 5) Binary Payload Format (Protobuf + Versioning)

### 5.1 Protobuf-net default
- **MUST:** Ana serializer = **protobuf-net**.
- **SHOULD:** Hot-path’lerde (milyonlarca nokta) custom binary writer opsiyonel; ama versioning maliyetini unutma.

### 5.2 Schema versioning & migration
- **MUST:** Her payload:
  - `schema_version`
  - `payload_kind`
  - `codec` (none/lz4)
  - `checksum`
  bilgisi taşır.
- **MUST:** Forward/backward uyumluluk:
  - Field tag’leri asla yeniden kullanılmaz.
  - Eski field’lar deprecate edilir, silinmez (gerekirse yeni schema’ya map edilir).

### 5.3 Coordinate encoding (performans için)
- **SHOULD:** Diskte point verisi delta + zigzag ile compress edilebilir.
- **Default yaklaşım:**
  - Domain: `double`
  - Persist: `int32/int64` quantized + delta + zigzag
- **MUST:** Quantization birimi spec’te sabitlenmeli (örn. `1 unit = 1/1000 world-unit`).

---

## 6) Ink Blokları + LZ4

### 6.1 LZ4 zorunluluğu
- **MUST:** Ink stream data için LZ4.
- **MUST:** Decompress hızlı olmalı (viewport hydration gecikmesin).

### 6.2 Block size
- **DEFAULT:** **64KB uncompressed** block hedefi (LZ4 için sweet spot).
- **MUST:** “tek dev blob” yaklaşımı yok; block’lara böl.

### 6.3 Zero-copy hedefi
- **SHOULD:** `K4os.Compression.LZ4` block API + `Span<byte>` / `ReadOnlySpan<byte>` kullan.
- **MUST:** GC pressure azaltmak için:
  - `ArrayPool<byte>.Shared`
  - `MemoryPool<T>`
  - “>85KB” LOH allocation kaçınma

---

## 7) Autosave & Threading Model (Channels + Immutable)

### 7.1 DirtyFlag semantiği
- **MUST:** Dirty flag entity + chunk seviyesinde tutulur.
- **MUST:** Autosave sadece “dirty varsa” çalışır.
- **DEFAULT:** 30 saniyede bir (senin ana kuralınla uyumlu).

### 7.2 Snapshot zorunluluğu
- **MUST:** UI thread “structural snapshot” üretir.
- **DEFAULT:** `System.Collections.Immutable` ile copy-on-write.
- **MUST:** Snapshot üretimi UI’yı bloklamayacak kadar hızlı olmalı.

### 7.3 Channel queue
- **MUST:** UI → background persistence arası `Channel<DomainSnapshot>` (bounded) ile yapılır.
- **MUST:** Kuyruk dolarsa strateji:
  - “latest-wins” (eskileri drop) veya
  - “merge dirty chunks”
  seçimi spec’te sabitlenir.

### 7.4 SQLite transaction batching
- **MUST:** Save cycle tek transaction içinde batch’lenir.
- **MUST:** WAL mode açıkken yazma yapılır → okuma/yazma çakışması minimize.

---

## 8) Crash-Safety & Integrity (XXHash64 + Atomic Replace)

### 8.1 Checksum
- **MUST:** Her chunk payload ve media blob için **XXHash64** checksum tutulur.
- **MUST:** Load sırasında verify edilir.
- **MUST:** Mismatch:
  - “son snapshot’tan recovery” dene veya
  - kullanıcıya “corrupted region” raporla (silent corruption yok).

### 8.2 Atomic “Write–Sync–Replace” (özellikle snapshot/compaction)
- **MUST:** Temp file aynı directory’de yazılır.
- **MUST:** `Flush(flushToDisk: true)` / `fsync` yapılır.
- **MUST:** Windows’ta atomik swap için **ReplaceFile** kullanılır.
- **NOTE:** Normal SQLite commit zaten atomic’tir; bu pattern özellikle:
  - `VACUUM INTO` ile yeni kopya üretip swap
  - full snapshot export
  senaryolarında zorunlu.

### 8.3 Antivirus lock gerçeği
- **SHOULD:** `File.Move` gibi naive swap’ler Windows Defender/AV lock ile patlayabilir → ReplaceFile tercih edilir.

---

## 9) Media / Attachments (CAS + Thumbnail + LOD)

### 9.1 Content-addressable storage (dedup)
- **MUST:** Asset eklenince `XXHash64` hesaplanır.
- **MUST:** Aynı hash varsa blob tekrar yazılmaz; referans ile reuse edilir.

### 9.2 Thumbnail & streaming
- **MUST:** Büyük media “streaming read” desteklemeli (tam decode/load yok).
- **MUST:** Thumbnail (preview) ayrı tutulmalı.

### 9.3 WPF decode optimizasyonu
- **MUST:** `BitmapImage.DecodePixelWidth/Height` ile downsample (display size’a göre).
- **SHOULD:** Prefetch: viewport velocity/direction’a göre background decode-cache.

---

## 10) SQLite Operational Tuning (WAL / mmap / page_size / vacuum)

### 10.1 PRAGMA baseline
- **MUST:** `journal_mode=WAL`
- **MUST:** `foreign_keys=ON`
- **SHOULD:** `busy_timeout` ayarla (UI donmasın).
- **SHOULD:** Prepared statements + parameter binding (string concat yok).

### 10.2 Large file tuning
- **SHOULD:** `page_size` 16KB veya 32KB (büyük blob workload için).
- **SHOULD:** `mmap_size` etkinleştir (64-bit sistemlerde I/O hızlanır).

### 10.3 Fragmentation yönetimi
- **MUST:** Büyük dokümanlarda fragmentation olur.
- **SHOULD:** `VACUUM INTO` ile background defrag + sonra ReplaceFile swap.

---

## 11) Encryption-at-Rest (Opsiyonel ama tanımlı)

- **OPTIONAL:** SQLCipher (page-level AES-256).
- **MUST (if enabled):** Key management sınırları net:
  - Windows: DPAPI ile passphrase protect
  - Opsiyonel: TPM-backed sealing
- **NOTE:** Encryption performans/karmaşıklık getirir → default kapalı olabilir.

---

## 12) Test & Validation (Benchmark + Chaos)

### 12.1 Benchmark suite
- **MUST:** `BenchmarkDotNet` ile throughput/latency/allocation ölç.
- Minimum senaryolar:
  - **Extreme Load:** 1 dakikada 1M stylus point ingest (tek chunk).
  - **Cold Start:** 1GB db, OS cache yokken viewport hydrate süresi.
  - **Warp Zoom:** hızlı zoom/pan sırasında sürekli chunk load/unload.

### 12.2 Chaos persistence
- **MUST:** Write cycle sırasında process kill / power loss simülasyonu.
- **MUST:** ReplaceFile/fsync kesilse bile:
  - checksum detection
  - snapshot recovery
  çalışmalı.

---

# 🔧 Config Parameters (Default Değerler)

> Bunlar “engine config” olarak tek yerde tanımlanmalı.

| Key | Default | Notes |
|---|---:|---|
| `AUTOSAVE_INTERVAL` | 30s | Dirty varsa |
| `CHANNEL_CAPACITY` | 2–4 | bounded; “latest-wins” önerilir |
| `CHUNK_STROKE_TARGET` | 80 | 50–100 aralığı |
| `LZ4_BLOCK_UNCOMPRESSED_BYTES` | 65536 | 64KB |
| `SQLITE_PAGE_SIZE` | 16384 | 32KB opsiyonel |
| `SQLITE_MMAP_SIZE` | 256MB–2GB | cihaza göre |
| `WAL_AUTOCHECKPOINT` | 1000 pages | tuning |
| `HASH_ALGO` | XXHash64 | chunk + asset |
| `THUMB_MAX_EDGE` | 512px | preview |
| `ASSET_DEDUP` | true | CAS |

---

# ✅ Uygulama Sınırları (Bu dosyanın kapsadığı / kapsamadığı)

**Bu dosya kapsar:**
- DB şema prensipleri, chunking, rtree, blob codec
- autosave threading ve snapshot
- crash safety, checksum, snapshot/compaction
- media dedup + thumbnail + WPF decode kuralları

**Bu dosya kapsamaz:**
- Canvas render/QuadTree (o `03_RENDERING_KERNEL.md`)
- StylusPlugIn threading (o `04_INPUT_THREADING.md`)
- UI XAML/Components (o `07_UI_COMPONENTS.md`)

---
